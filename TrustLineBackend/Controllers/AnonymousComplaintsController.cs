using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using AnonymousComplaintsAPI.Services.EnsureServices;
using AnonymousComplaintsAPI.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AnonymousComplaintsAPI.Helpers;

namespace AnonymousComplaintsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnonymousComplaintsController : ControllerBase
    {
        private readonly IEnsureService _ensureService;
        private readonly IAnonymousComplaintService _complaintService;
        private readonly ITypeService _typeService;
        private readonly IAttachmentService _attachmentService;

        public AnonymousComplaintsController(
            IEnsureService ensureService,
            IAnonymousComplaintService complaintService,
            ITypeService typeService,
            IAttachmentService attachmentService)
        {
            _ensureService = ensureService;
            _complaintService = complaintService;
            _typeService = typeService;
            _attachmentService = attachmentService;
        }

        [HttpGet]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<ActionResult<PaginatedResponse<AnonymousComplaintResponse>>> GetAnonymousComplaints([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _complaintService.GetComplaintsPaginatedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des plaintes paginées : {ex.Message}");
            }
        }


        [HttpGet("{id}/fused")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<ActionResult<IEnumerable<AnonymousComplaintResponse>>> GetFusedComplaints(int id)
        {
            try
            {
                // Use service to get fused complaints
                var fusedComplaints = await _complaintService.GetFusedComplaintsAsync(id);
                return Ok(fusedComplaints);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des plaintes fusionnées pour la plainte {id} : {ex.Message}");
            }
        }


        [HttpGet("user")]
        [RequirePermission("tl-v-report")]
        public async Task<ActionResult<ComplaintsByUserResponse>> GetComplaintsByUserAsync([FromQuery] PaginationRequest request)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst("Id");
                if (userIdClaim == null)
                {
                    return Unauthorized("Utilisateur non authentifié ou token invalide");
                }
                if (!int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized("Id utilisateur invalide dans le token");
                }

                var user = await _ensureService.EnsureUserExistsAsync(userId);
                //request.UserId = userId;
                var result = await _complaintService.GetComplaintsByUserAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des plaintes paginées : {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        [RequirePermission("tl-v-report")]
        public async Task<ActionResult<ComplaintsByUserResponse>> GetComplaintsByUserId(int userId, [FromQuery] PaginationRequest request)
        {
            try
            {
                await _ensureService.EnsureUserExistsAsync(userId);
                var result = await _complaintService.GetComplaintsByUserAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des plaintes paginées : {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [RequirePermission("tl-r-report")]
        public async Task<ActionResult<AnonymousComplaintResponse>> GetComplaintDetails(int id)
        {
            try
            {
                var complaint = await _complaintService.GetComplaintDetailsAsync(id);

                if (complaint == null)
                    return NotFound(new { message = $"Aucun signalement trouvé avec l'ID {id}." });

                return Ok(complaint);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur interne du serveur." });
            }
        }

        [HttpPost]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<CreateAnonymousComplaintRequest>> PostAnonymousComplaint([FromForm] CreateAnonymousComplaintRequest dto)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst("Id");
                var userId = int.Parse(userIdClaim!.Value);
                var user = await _ensureService.EnsureUserExistsAsync(userId);

                // Vérification du type via le service
                if (dto.TypeID.HasValue)
                {
                    var type = await _typeService.GetTypeAsync(dto.TypeID.Value);
                    if (type == null)
                    {
                        return BadRequest("Erreur : Le TypeID fourni n'existe pas.");
                    }
                }

                var createdComplaint = await _complaintService.CreateComplaintWithAttachmentsAsync(dto, Request.Form.Files, userId);
                return Ok("OK");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de la réclamation : {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("tl-v-report")]
        public async Task<IActionResult> PutAnonymousComplaint(int id, [FromForm] CreateAnonymousComplaintRequest dto)
        {
            try
            {
                // Vérifier si le model state est valide
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return BadRequest(new { errors, message = "Erreur de validation" });
                }

                if (id != dto.Id)
                    return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans le corps de la requête.");

                // Utilise le service qui gère toute la logique métier et de fichiers
                var updatedComplaint = await _complaintService.UpdateComplaintWithAttachmentsAsync(id, dto, Request.Form.Files);

                return Ok(updatedComplaint);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur est survenue lors de la mise à jour: {ex.Message}");
            }
        }


        [HttpPut("ChangeState/{id}")]
        [RequirePermission("tl-u-report")]
        public async Task<IActionResult> ChangeState(int id)
        {
            try
            {
                // Get complaint through service to check existence
                var complaint = await _complaintService.GetComplaintAsync(id);

                if (complaint == null || complaint.Archived == true)
                    return NotFound("Plainte non trouvée ou déjà archivée.");

                // Transition state if currently SUBMITTED
                if (complaint.State == "DÉPOSÉ")
                    await _complaintService.TransitionComplaintStateAsync(id, "IN PROGRESS");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors du changement d'état : {ex.Message}");
            }
        }

        //[HttpPut("AddResponse")]
        //public async Task<IActionResult> AddComplaintResponse(SendResponseRequest dto)
        //{
        //    try
        //    {
        //        // Utilise le service qui gère l'ajout de réponse et la mise à jour de l'état
        //        var solution = await _complaintService.AddComplaintResponseAsync(dto);

        //        return Ok("Reponse Ajouter");
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, "Une erreur est survenue lors de l'ajout d'une reponse.");
        //    }
        //}

        [HttpDelete("{id}")]
        [RequirePermission("tl-d-report")]
        public async Task<IActionResult> ArchiveAnonymousComplaint(int id)
        {
            try
            {
                await _complaintService.ArchiveComplaintAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'archivage : {ex.Message}");
            }
        }

        [HttpPut("restore/{id}")]
        [RequirePermission("tl-a-report")]
        public async Task<IActionResult> RestoreAnonymousComplaint(int id)
        {
            try
            {
                await _complaintService.RestoreComplaintAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la restauration : {ex.Message}");
            }
        }


        [HttpPost("merge")]
        [RequirePermission("tl-m-report")]
        public async Task<IActionResult> MergeComplaints([FromBody] List<int> complaintIds)
        {
            try
            {
                if (complaintIds == null || complaintIds.Count < 2)
                    return BadRequest("Veuillez sélectionner au moins deux réclamations à fusionner.");

                var mainComplaintId = await _complaintService.MergeComplaintsAsync(complaintIds);

                return Ok(new
                {
                    message = $"Fusion effectuée avec succès : {complaintIds.Count()} signalements ont été fusionnés avec le signalement numéro {mainComplaintId}.",
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la fusion des plaintes : {ex.Message}");
            }
        }



    }
}