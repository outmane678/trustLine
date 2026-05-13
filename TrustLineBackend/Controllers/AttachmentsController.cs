using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AnonymousComplaintsAPI.Helpers;

namespace AnonymousComplaintsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentsController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpGet("download-all/{complaintId}")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<IActionResult> DownloadAllFiles(int complaintId)
        {
            try
            {
                var zipStream = await _attachmentService.GenerateComplaintAttachmentsZipAsync(complaintId);

                if (zipStream == null)
                {
                    return NotFound("Aucun fichier trouvé pour cette réclamation");
                }

                zipStream.Seek(0, SeekOrigin.Begin);

                // Nom du fichier ZIP
                var zipFileName = $"Reclamation_{complaintId}_Fichiers_{DateTime.Now:yyyyMMddHHmmss}.zip";

                return File(zipStream, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors du téléchargement des fichiers : {ex.Message}");
            }
        }

        // GET: api/Attachments
        // Get all non-archived attachments
        [HttpGet]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetAttachments()
        {
            try
            {
                var attachments = await _attachmentService.GetAllAttachmentsAsync();
                return Ok(attachments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des pièces jointes : {ex.Message}");
            }
        }

        // GET: api/Attachments/5
        // Get a specific attachment by ID
        [HttpGet("{id}")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<ActionResult<AttachmentResponse>> GetAttachment(int id)
        {
            try
            {
                var attachment = await _attachmentService.GetAttachmentAsync(id);

                if (attachment == null)
                    return NotFound();

                return Ok(attachment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de la pièce jointe : {ex.Message}");
            }
        }

        // PUT: api/Attachments/5
        // Update an existing attachment
        [HttpPut("{id}")]
        [RequirePermission("tl-u-report")]
        public async Task<IActionResult> PutAttachment(int id, AttachmentResponse dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest();

                var updatedAttachment = await _attachmentService.UpdateAttachmentAsync(id, dto);

                if (updatedAttachment == null)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour de la pièce jointe : {ex.Message}");
            }
        }

        // POST: api/Attachments
        // Create a new attachment
        [HttpPost]
        [RequirePermission("tl-u-report")]
        public async Task<ActionResult<AttachmentResponse>> PostAttachment(AttachmentResponse dto)
        {
            try
            {
                var createdAttachment = await _attachmentService.CreateAttachmentAsync(dto);

                return CreatedAtAction(nameof(GetAttachment), new { id = createdAttachment.Id }, createdAttachment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de la pièce jointe : {ex.Message}");
            }
        }

        [HttpPost("upload")]
        [RequirePermission("tl-u-report")]
        public async Task<ActionResult<AttachmentResponse>> UploadAttachment(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                var createdAttachment = await _attachmentService.UploadAttachmentAsync(file);

                return CreatedAtAction(nameof(GetAttachment), new { id = createdAttachment.Id }, createdAttachment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'upload de la pièce jointe : {ex.Message}");
            }
        }

        // DELETE: api/Attachments/5
        // Soft delete (archive) an attachment
        [HttpDelete("{id}")]
        [RequirePermission("tl-d-report")]
        public async Task<IActionResult> SoftDeleteAttachment(int id)
        {
            try
            {
                await _attachmentService.ArchiveAttachmentAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'archivage de la pièce jointe : {ex.Message}");
            }
        }

        // PUT: api/Attachments/restore/5
        // Restore an archived attachment
        [HttpPut("restore/{id}")]
        [RequirePermission("tl-d-report")]
        public async Task<IActionResult> RestoreAttachment(int id)
        {
            try
            {
                await _attachmentService.RestoreAttachmentAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la restauration de la pièce jointe : {ex.Message}");
            }
        }

        // DELETE: api/Attachments/hard-delete/5
        // Hard delete an attachment from the database
        [HttpDelete("hard-delete/{id}")]
        [RequirePermission("tl-d-report")]
        public async Task<IActionResult> HardDeleteAttachment(int id)
        {
            try
            {
                await _attachmentService.DeleteAttachmentAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de la pièce jointe : {ex.Message}");
            }
        }
    }
}
