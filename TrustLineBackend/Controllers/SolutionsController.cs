using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.Helpers;




namespace AnonymousComplaintsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SolutionsController : ControllerBase
    {
        private readonly ISolutionService _solutionService;

        public SolutionsController(ISolutionService solutionService)
        {
            _solutionService = solutionService;
        }

        // GET: api/Solutions
        // Récupère uniquement les solutions non archivées
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolutionResponse>>> GetSolutions()
        {
            try
            {
                var solutions = await _solutionService.GetAllSolutionsAsync();
                return Ok(solutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des solutions : {ex.Message}");
            }
        }

        // GET: api/Solutions/all
        // Récupère toutes les solutions, y compris celles archivées
        [HttpGet("all")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<ActionResult<IEnumerable<SolutionResponse>>> GetAllSolutions()
        {
            try
            {
                var solutions = await _solutionService.GetAllSolutionsIncludingArchivedAsync();
                return Ok(solutions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de toutes les solutions : {ex.Message}");
            }
        }

        // GET: api/Solutions/5
        // Récupère une solution par son ID
        [HttpGet("{id}")]
        public async Task<ActionResult<SolutionResponse>> GetSolution(int id)
        {
            try
            {
                var solution = await _solutionService.GetSolutionAsync(id);

                if (solution == null)
                    return NotFound();

                return Ok(solution);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de la solution : {ex.Message}");
            }
        }

        // PUT: api/Solutions/5
        // Met à jour une solution
        [HttpPut("{id}")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<IActionResult> PutSolution(int id, SendResponseRequest dto)
        {
            try
            {
                if (id != dto.SolutionID)
                    return BadRequest();

                var updatedSolution = await _solutionService.UpdateSolutionAsync(id, dto);

                if (updatedSolution == null)
                    return NotFound();

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour de la solution : {ex.Message}");
            }
        }

        // Crée une nouvelle solution
        [HttpPost]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<ActionResult<SolutionResponse>> PostSolution(SendResponseRequest dto)
        {
            try
            {
                // Utilise le service qui gère la logique de fusion automatiquement
                var createdSolution = await _solutionService.CreateSolutionForComplaintAndMergedAsync(dto);

                return CreatedAtAction("GetSolution", new { id = createdSolution.SolutionID }, createdSolution);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de la solution : {ex.Message}");
            }
        }


        // PATCH: api/Solutions/archive/5
        // Effectue un soft delete (archive)
        [HttpPatch("archive/{id}")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<IActionResult> ArchiveSolution(int id)
        {
            try
            {
                await _solutionService.ArchiveSolutionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'archivage de la solution : {ex.Message}");
            }
        }

        // PATCH: api/Solutions/restore/5
        // Restaure une solution archivée
        [HttpPatch("restore/{id}")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<IActionResult> RestoreSolution(int id)
        {
            try
            {
                await _solutionService.RestoreSolutionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la restauration de la solution : {ex.Message}");
            }
        }

        // DELETE: api/Solutions/5
        // Supprime définitivement une solution (hard delete)
        [HttpDelete("{id}")]
        [RequirePermission("tl-v-reportmanagement")]
        public async Task<IActionResult> DeleteSolution(int id)
        {
            try
            {
                await _solutionService.DeleteSolutionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de la solution : {ex.Message}");
            }
        }
    }
}