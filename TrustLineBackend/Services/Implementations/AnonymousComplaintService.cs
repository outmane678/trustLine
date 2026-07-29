using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Mappers;
using AnonymousComplaintsAPI.Models.Entities;
using AnonymousComplaintsAPI.Repositories.Interfaces;
using AnonymousComplaintsAPI.Services.EnsureServices;
using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AnonymousComplaintsAPI.Services.Implementations;

/// <summary>
/// Implementation of Anonymous Complaint Service with complex business logic
/// </summary>
public class AnonymousComplaintService : IAnonymousComplaintService
{
    private readonly IAnonymousComplaintRepository _complaintRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ISolutionRepository _solutionRepository;
    private readonly IFileService _fileService;
    private readonly ILogger<AnonymousComplaintService> _logger;
    private readonly IEnsureService _ensureService;
    private readonly IHrLinkService _hrLinkService;

    public AnonymousComplaintService(
        IAnonymousComplaintRepository complaintRepository,
        ICategoryRepository categoryRepository,
        ITypeRepository typeRepository,
        IAttachmentRepository attachmentRepository,
        ISolutionRepository solutionRepository,
        IFileService fileService,
        ILogger<AnonymousComplaintService> logger,
        IEnsureService ensureService,
        IHrLinkService hrLinkService
        )
    {
        _complaintRepository = complaintRepository;
        _categoryRepository = categoryRepository;
        _typeRepository = typeRepository;
        _attachmentRepository = attachmentRepository;
        _solutionRepository = solutionRepository;
        _fileService = fileService;
        _logger = logger;
        _ensureService = ensureService;
        _hrLinkService = hrLinkService;
       
        
    }

    public async Task<AnonymousComplaintResponse?> GetComplaintAsync(int id)
    {
        var complaint = await _complaintRepository.GetWithDetailsAsync(id);
        var externalProfiles = await _ensureService.GetExternalProfilesAsync();
        return complaint != null ? AnonymousComplaintMapper.ToResponse(complaint, externalProfiles) : null;
    }

    public async Task<AnonymousComplaintResponse> CreateComplaintWithAttachmentsAsync(
       CreateAnonymousComplaintRequest request,
       IFormFileCollection? files,
       int userId)
    {
        // Validate files BEFORE starting any database operation
        if (files != null && files.Count > 0)
        {
            var allowedExtensions = new[]
            {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            ".mp4", ".avi", ".mov", ".wmv", ".webm", ".mkv",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"
        };
            const long maxFileSize = 20 * 1024 * 1024; // 20 MB

            foreach (var file in files)
            {
                if (!_fileService.ValidateFile(file, maxFileSize, allowedExtensions))
                {
                    _logger.LogWarning("File validation failed for {FileName} before complaint creation", file.FileName);
                    throw new ArgumentException($"Le fichier '{file.FileName}' n'est pas valide. Seuls les fichiers image, vidéo, PDF, Word, Excel et PowerPoint sont autorisés (max 20 MB).");
                }
            }
        }

        // Validate required data before starting transaction
        if (request.TypeID == null)
            throw new ArgumentException("TypeID is required");

        var typeExists = await _typeRepository.ExistsAsync(request.TypeID.Value);
        if (!typeExists)
            throw new ArgumentException($"Type with ID {request.TypeID.Value} does not exist");

        var strategy = _complaintRepository.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            IDbContextTransaction? transaction = null;
            try { transaction = await _complaintRepository.BeginTransactionAsync(); }
            catch (InvalidOperationException) { /* InMemory does not support transactions */ }

            try
            {
                // Create complaint entity
                var complaint = new AnonymousComplaint
                {
                    Description = request.Description?.Trim(),
                    CreatedBy = userId,
                    State = "DÉPOSÉ", // Initial state
                    CreatedAt = DateTime.Now,
                    Archived = false,
                    CategoryId = request.CategoryID,
                    TypeId = request.TypeID,
                    FrequencyId = request.FrequencyID,
                    IncidentDate = request.IncidentDate,
                    IsIdentityVisible = request.IsIdentityVisible ?? false
                };

                // Save complaint to database
                var createdComplaint = await _complaintRepository.CreateAsync(complaint);
                _logger.LogInformation("Complaint created with ID: {ComplaintId}", createdComplaint.AnonymousComplaintId);

                // Handle file uploads if provided
                if (files != null && files.Count > 0)
                {
                    await HandleFileUploadsAsync(createdComplaint.AnonymousComplaintId, files, userId);
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed successfully for complaint {ComplaintId}", createdComplaint.AnonymousComplaintId);
                }

                // Reload complaint with details
                var complaintWithDetails = await _complaintRepository.GetWithDetailsAsync(createdComplaint.AnonymousComplaintId);
                return AnonymousComplaintMapper.ToResponse(complaintWithDetails!, null);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating complaint - transaction rolled back");
                }
                throw;
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }
        });
    }

    public async Task<AnonymousComplaintResponse> UpdateComplaintWithAttachmentsAsync(
    int id,
    CreateAnonymousComplaintRequest request,
    IFormFileCollection? files)
    {
        // Validate files BEFORE any DB operation
        if (files != null && files.Count > 0)
        {
            var allowedExtensions = new[]
            {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            ".mp4", ".avi", ".mov", ".wmv", ".webm", ".mkv",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"
        };
            const long maxFileSize = 20 * 1024 * 1024; // 20 MB

            foreach (var file in files)
            {
                if (!_fileService.ValidateFile(file, maxFileSize, allowedExtensions))
                {
                    _logger.LogWarning("File validation failed for {FileName} before complaint update", file.FileName);
                    throw new ArgumentException($"Le fichier '{file.FileName}' n'est pas valide. Seuls les fichiers image, vidéo, PDF, Word, Excel et PowerPoint sont autorisés (max 20 MB).");
                }
            }
        }

        var strategy = _complaintRepository.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            IDbContextTransaction? transaction = null;
            try { transaction = await _complaintRepository.BeginTransactionAsync(); }
            catch (InvalidOperationException) { /* InMemory does not support transactions */ }

            try
            {
                // Get existing complaint
                var complaint = await _complaintRepository.GetByIdAsync(id);
                if (complaint == null || complaint.Archived == true)
                {
                    throw new KeyNotFoundException($"Complaint with ID {id} not found or archived");
                }

                // Validate TypeID if provided
                if (request.TypeID != null)
                {
                    var typeExists = await _typeRepository.ExistsAsync(request.TypeID.Value);
                    if (!typeExists)
                    {
                        throw new ArgumentException($"Type with ID {request.TypeID.Value} does not exist");
                    }
                }

                // Validate CategoryID if provided
                if (request.CategoryID != null)
                {
                    var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryID.Value);
                    if (!categoryExists)
                    {
                        throw new ArgumentException($"Category with ID {request.CategoryID.Value} does not exist");
                    }
                }

                // Update complaint fields
                complaint.Description = request.Description?.Trim();
                complaint.CategoryId = request.CategoryID;
                complaint.TypeId = request.TypeID;
                complaint.FrequencyId = request.FrequencyID;
                complaint.IncidentDate = request.IncidentDate;
                complaint.IsIdentityVisible = request.IsIdentityVisible;

                await _complaintRepository.UpdateAsync(complaint);
                _logger.LogInformation("Complaint {ComplaintId} updated", id);

                // Handle file deletion (archive files)
                if (request.FilesToDelete != null && request.FilesToDelete.Count > 0)
                {
                    foreach (var attachmentId in request.FilesToDelete)
                    {
                        await _attachmentRepository.ArchiveAsync(attachmentId);
                        _logger.LogInformation("Attachment {AttachmentId} archived for complaint {ComplaintId}", attachmentId, id);
                    }
                }

                // Handle new file uploads
                if (files != null && files.Count > 0)
                {
                    await HandleFileUploadsAsync(id, files, complaint.CreatedBy.Value);
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed successfully for complaint {ComplaintId}", id);
                }

                // Reload complaint with details
                var complaintWithDetails = await _complaintRepository.GetWithDetailsAsync(id);
                return AnonymousComplaintMapper.ToResponse(complaintWithDetails!, null);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating complaint {ComplaintId} - transaction rolled back", id);
                }
                throw;
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }
        });
    }


    public async Task<int> MergeComplaintsAsync(List<int> complaintIds)
    {
        try
        {
            if (complaintIds == null || complaintIds.Count < 2)
            {
                throw new ArgumentException("At least 2 complaints are required for merging");
            }
            _logger.LogInformation("Starting merge of {Count} complaints", complaintIds.Count);

            // Get all complaints
            var complaints = new List<AnonymousComplaint>();
            foreach (var id in complaintIds)
            {
                var complaint = await _complaintRepository.GetByIdAsync(id);
                if (complaint != null)
                {
                    complaints.Add(complaint);
                }
            }

            if (complaints.Count < complaintIds.Count)
            {
                throw new KeyNotFoundException("One or more specified complaints were not found");
            }

            if (complaints.Count < 2)
            {
                throw new ArgumentException("At least 2 valid complaints are required for merging");
            }

            // Vérification du type
            var distinctTypeIds = complaints.Select(c => c.TypeId).Distinct().ToList();
            if (distinctTypeIds.Count > 1)
            {
                var typeNames = string.Join(", ", complaints
                    .Select(c => c.Type?.Name ?? "Type inconnu")
                    .Distinct());
                throw new ArgumentException($"Impossible de fusionner : les réclamations appartiennent à plusieurs types ({typeNames}).");
            }

            // Find the oldest complaint (by CreatedAt)
            var mainComplaint = complaints.OrderBy(c => c.CreatedAt).First();
            _logger.LogInformation("Main complaint selected: {ComplaintId}", mainComplaint.AnonymousComplaintId);

            // Pour chaque signalement à fusionner, récupérer aussi ses enfants fusionnés
            var allComplaintsToMerge = new List<AnonymousComplaint>();

            foreach (var complaint in complaints.Where(c => c.AnonymousComplaintId != mainComplaint.AnonymousComplaintId))
            {
                // Ajouter le signalement lui-même
                allComplaintsToMerge.Add(complaint);

                // Si ce signalement a déjà des signalements fusionnés avec lui, les récupérer aussi
                var childComplaints = await _complaintRepository.GetMergedComplaintsAsync(complaint.AnonymousComplaintId);
                if (childComplaints != null && childComplaints.Any())
                {
                    allComplaintsToMerge.AddRange(childComplaints);
                    _logger.LogInformation("Found {Count} child complaints for complaint {ComplaintId}",
                        childComplaints.Count(), complaint.AnonymousComplaintId);
                }
            }

            // Déterminer le state le plus avancé parmi tous les signalements
            // Ordre de priorité: RESOLVED > IN PROGRESS > SUBMITTED
            var allComplaints = new List<AnonymousComplaint> { mainComplaint };
            allComplaints.AddRange(allComplaintsToMerge);
            
            string mostAdvancedState = GetMostAdvancedState(allComplaints.Select(c => c.State).ToList());
            _logger.LogInformation("Most advanced state determined: {State}", mostAdvancedState);

            // Collecter toutes les solutions de tous les signalements
            var allSolutions = new List<Solution>();
            foreach (var complaint in allComplaints)
            {
                var solutions = await _solutionRepository.GetByComplaintIdAsync(complaint.AnonymousComplaintId);
                if (solutions != null && solutions.Any())
                {
                    allSolutions.AddRange(solutions);
                }
            }
            _logger.LogInformation("Total solutions collected from all complaints: {Count}", allSolutions.Count);

            // Mettre à jour le state du signalement principal avec le state le plus avancé
            if (mainComplaint.State != mostAdvancedState)
            {
                mainComplaint.State = mostAdvancedState;
                await _complaintRepository.UpdateAsync(mainComplaint);
                _logger.LogInformation("Updated main complaint {ComplaintId} state to: {State}", 
                    mainComplaint.AnonymousComplaintId, mostAdvancedState);
            }

            // Update FusionWithId and State for all complaints to merge
            foreach (var complaint in allComplaintsToMerge)
            {
                complaint.FusionWithId = mainComplaint.AnonymousComplaintId;
                complaint.State = mostAdvancedState;
                await _complaintRepository.UpdateAsync(complaint);
                _logger.LogInformation("Updated FusionWithId and State for complaint {ComplaintId} to State: {State}", 
                    complaint.AnonymousComplaintId, mostAdvancedState);
            }

            // Partager toutes les solutions entre tous les signalements (principal + fusionnés)
            foreach (var targetComplaint in allComplaints)
            {
                // Récupérer les solutions existantes du signalement cible
                var existingSolutions = await _solutionRepository.GetByComplaintIdAsync(targetComplaint.AnonymousComplaintId);
                var existingSolutionContents = existingSolutions?.Select(s => s.Content).ToList() ?? new List<string>();

                foreach (var solution in allSolutions)
                {
                    // Ne pas copier une solution vers son propre signalement d'origine
                    if (solution.AnonymousComplaintId == targetComplaint.AnonymousComplaintId)
                        continue;

                    // Éviter les doublons en vérifiant si la solution existe déjà
                    if (!existingSolutionContents.Contains(solution.Content))
                    {
                        var newSolution = new Solution
                        {
                            Content = solution.Content,
                            AnonymousComplaintId = targetComplaint.AnonymousComplaintId,
                            CreatedBy = solution.CreatedBy,
                            CreatedAt = solution.CreatedAt,
                            Archived = false
                        };
                        await _solutionRepository.CreateAsync(newSolution);
                        existingSolutionContents.Add(solution.Content); // Éviter les doublons dans la même boucle
                        _logger.LogInformation("Copied solution to complaint {ComplaintId}", targetComplaint.AnonymousComplaintId);
                    }
                }
            }

            return mainComplaint.AnonymousComplaintId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging complaints");
            throw;
        }
    }

    /// <summary>
    /// Détermine le state le plus avancé parmi une liste de states
    /// Ordre de priorité: RESOLVED > IN PROGRESS > SUBMITTED/SUBMITED
    /// </summary>
    private string GetMostAdvancedState(List<string> states)
    {
        if (states.Any(s => s == "RESOLVED"))
            return "RESOLVED";
        if (states.Any(s => s == "IN PROGRESS"))
            return "IN PROGRESS";
        return "DÉPOSÉ";
    }

    public async Task<IEnumerable<AnonymousComplaintResponse>> GetFusedComplaintsAsync(int complaintId)
    {
        try
        {
            _logger.LogInformation("Getting fused complaints for complaint {ComplaintId}", complaintId);

            // Get all merged/fused complaints
            var fusedComplaints = await _complaintRepository.GetMergedComplaintsAsync(complaintId);

            // Get external profiles for mapping
            var externalProfiles = await _ensureService.GetExternalProfilesAsync();

            // Map to response DTOs
            var response = AnonymousComplaintMapper.ToResponseList(fusedComplaints, externalProfiles);

            _logger.LogInformation("Found {Count} fused complaints", response.Count());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fused complaints for complaint {ComplaintId}", complaintId);
            throw;
        }
    }

    public async Task TransitionComplaintStateAsync(int id, string newState)
    {
        try
        {
            var complaint = await _complaintRepository.GetByIdAsync(id);
            if (complaint == null)
            {
                throw new KeyNotFoundException($"Complaint with ID {id} not found");
            }

            if (complaint.Archived == true)
            {
                throw new InvalidOperationException("Cannot change state of an archived complaint");
            }

            // Mettre à jour le state du signalement principal
            await _complaintRepository.UpdateStateAsync(id, newState);
            _logger.LogInformation("Complaint {ComplaintId} state changed to {NewState}", id, newState);

            // Mettre à jour aussi les signalements fusionnés avec ce signalement
            var mergedComplaints = await _complaintRepository.GetMergedComplaintsAsync(id);
            if (mergedComplaints != null && mergedComplaints.Any())
            {
                foreach (var mergedComplaint in mergedComplaints)
                {
                    await _complaintRepository.UpdateStateAsync(mergedComplaint.AnonymousComplaintId, newState);
                    _logger.LogInformation("Merged complaint {ComplaintId} state changed to {NewState}", 
                        mergedComplaint.AnonymousComplaintId, newState);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning complaint {ComplaintId} state", id);
            throw;
        }
    }

    public async Task ArchiveComplaintAsync(int id)
    {
        var entity = await _complaintRepository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Complaint {id} not found");
        await _complaintRepository.ArchiveAsync(id);
        _logger.LogInformation("Complaint {ComplaintId} archived", id);
    }

    public async Task RestoreComplaintAsync(int id)
    {
        var entity = await _complaintRepository.GetByIdAsync(id);
        if (entity == null || entity.Archived != true)
            throw new KeyNotFoundException($"Complaint {id} not found or not archived");
        await _complaintRepository.RestoreAsync(id);
        _logger.LogInformation("Complaint {ComplaintId} restored", id);
    }

    public async Task<SolutionResponse> AddComplaintResponseAsync(SendResponseRequest request)
    {
        try
        {
            var complaintId = request.AnonymousComplaintID ?? throw new ArgumentException("AnonymousComplaintID is required");

            // Get complaint
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                throw new KeyNotFoundException($"Complaint with ID {complaintId} not found");
            }

            // Create solution
            var solution = new Solution
            {
                Content = request.Content,
                AnonymousComplaintId = complaintId,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                Archived = false
            };

            var createdSolution = await _solutionRepository.CreateAsync(solution);
            _logger.LogInformation("Solution created with ID: {SolutionId}", createdSolution.SolutionId);

            // Update complaint state to RESOLVED
            await _complaintRepository.UpdateStateAsync(complaintId, "RESOLVED");
            _logger.LogInformation("Complaint {ComplaintId} marked as RESOLVED", complaintId);

            return SolutionMapper.ToResponse(createdSolution,null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding response to complaint {ComplaintId}", request.SolutionID);
            throw;
        }
    }

    private async Task HandleFileUploadsAsync(int complaintId, IFormFileCollection files, int userId)
    {
        // Define allowed file types and size limit
        var allowedExtensions = new[]
        {
            // Images
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            // Videos
            ".mp4", ".avi", ".mov", ".wmv", ".webm", ".mkv",
            // Documents
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"
        };

        const long maxFileSize = 20 * 1024 * 1024; // 20 MB

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        var baseFolder = Path.Combine(uploadsFolder, "ReclamationFiles");

        // Déterminer l'année et le mois
        var complaintDate = DateTime.Now;
        var yearFolderName = complaintDate.Year.ToString();
        var monthFolderName = complaintDate.ToString("MMMM", new System.Globalization.CultureInfo("fr-FR"));

        var yearFolder = Path.Combine(baseFolder, yearFolderName);
        var monthFolder = Path.Combine(yearFolder, monthFolderName);

        if (!Directory.Exists(monthFolder))
            Directory.CreateDirectory(monthFolder);

        foreach (var file in files)
        {
            try
            {
                // Validate file before saving
                if (!_fileService.ValidateFile(file, maxFileSize, allowedExtensions))
                {
                    _logger.LogWarning("File validation failed for {FileName} in complaint {ComplaintId}",
                        file.FileName, complaintId);
                    throw new ArgumentException($"Le fichier '{file.FileName}' n'est pas valide. Seuls les fichiers image, vidéo, PDF, Word, Excel et PowerPoint sont autorisés (max 20 MB).");
                }

                if (file.Length > 0)
                {
                    // Créer d'abord l'attachment en DB pour obtenir l'ID
                    var attachment = new Attachment
                    {
                        FileName = file.FileName,
                        FilePath = "temp", // Temporaire, sera mis à jour après
                        FileType = file.ContentType,
                        CreatedAt = DateTime.Now,
                        Archived = false,
                        CreatedBy = userId,
                        AnonymousComplaintId = complaintId
                    };

                    await _attachmentRepository.CreateAsync(attachment);

                    // Maintenant qu'on a l'ID, créer le nom de fichier unique
                    var uniqueFileName = $"{attachment.AttachmentId}_{file.FileName}";
                    var fileSavePath = Path.Combine(monthFolder, uniqueFileName);

                    using (var stream = new FileStream(fileSavePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Mettre à jour le FilePath avec le bon chemin incluant l'ID
                    var relativePath = Path.Combine(yearFolderName, monthFolderName, uniqueFileName)
                        .Replace("\\", "/");

                    // Récupérer l'attachment depuis la DB pour éviter les problèmes de tracking
                    var attachmentToUpdate = await _attachmentRepository.GetByIdAsync(attachment.AttachmentId);
                    if (attachmentToUpdate != null)
                    {
                        attachmentToUpdate.FilePath = relativePath;
                        await _attachmentRepository.UpdateAsync(attachmentToUpdate);
                    }

                    _logger.LogInformation("Attachment created for complaint {ComplaintId}: {FileName}",
                        complaintId, file.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} for complaint {ComplaintId}",
                    file.FileName, complaintId);
                throw; // Re-throw to fail the entire operation if a file is invalid
            }
        }
    }

    public async Task<DTOs.Responses.PaginatedResponse<AnonymousComplaintResponse>> GetComplaintsPaginatedAsync(DTOs.Requests.PaginationRequest request)
    {
        try
        {
            var (data, total) = await _complaintRepository.GetPaginatedAsync(
                request.Archive,
                request.TypeId,
                request.CategoryId,
                request.State,
                request.DateFrom,
                request.DateTo
            );

            var externalProfiles = await _ensureService.GetExternalProfilesAsync();

            //Recherche 
            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var lower = request.Q.ToLower();

                data = data.Where(c =>
                {
                    // Recherche dans la description
                    bool matchDescription = c.Description != null
                                && c.Description.ToLower().Contains(lower);

                    // Recherche par nom uniquement si ce n est pas anonyme
                    bool matchName = false;
                    if (c.IsIdentityVisible == true || c.TypeId != 34)
                    {
                        var user = externalProfiles.FirstOrDefault(p => p.UserID == c.CreatedBy);
                        if (user != null)
                        {
                            var first = user.FirstName?.ToLower() ?? "";
                            var last = user.LastName?.ToLower() ?? "";
                            var full = $"{user.FirstName} {user.LastName}".ToLower();

                            matchName = first.Contains(lower) || last.Contains(lower) || full.Contains(lower);
                        }
                    }

                    return matchName || matchDescription;
                }).ToList();

                total = data.Count();
            }

            var complaintResponses = AnonymousComplaintMapper.ToResponseList(data, externalProfiles);

            var mainComplaints = complaintResponses
            .Where(c => c.FusionWithId == null)
            .ToList();

            var pagedData = mainComplaints
            .Skip(request.Skip)
            .Take(request.PerPage)
            .ToList();


            return new DTOs.Responses.PaginatedResponse<AnonymousComplaintResponse>
            {
                Total = total,
                MainComplaintsTotal = mainComplaints.Count,
                Data = pagedData,
                AllData = complaintResponses,
                Params = request,
                Page = request.Page,
                PerPage = request.PerPage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated complaints");
            throw;
        }
    }

    public async Task<ComplaintsByUserResponse> GetComplaintsByUserAsync(int UserId,DTOs.Requests.PaginationRequest request)
    {
        try
        {
            var (data, total) = await _complaintRepository.GetByUserAsync(
                request.Q,
                request.Archive,
                UserId,
                request.TypeId,
                request.CategoryId,
                request.State,
                request.DateFrom,
                request.DateTo
            );

            var pagedData = data
                .Skip(request.Skip)
                .Take(request.PerPage)
                .ToList();

            var userProfile = await _hrLinkService.GetProfileByUserIdAsync(UserId);

            return AnonymousComplaintMapper.ToComplaintsByUserResponse(userProfile, pagedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated complaints");
            throw;
        }
    }

    public async Task<AnonymousComplaintResponse?> GetComplaintDetailsAsync(int id)
    {
        try
        {
            var complaint = await _complaintRepository.GetDetailsByIdAsync(id);
            if (complaint == null)
                return null;

            // Récupérer tous les profils en cache
            var externalProfiles = await _ensureService.GetExternalProfilesAsync();

            var response = AnonymousComplaintMapper.ToResponse(complaint, externalProfiles);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting complaint details for ID {id}");
            throw;
        }
    }


}
