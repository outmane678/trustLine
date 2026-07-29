using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.Helpers;

namespace AnonymousComplaintsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrequenciesController : ControllerBase
    {
        private readonly IFrequencyService _frequencyService;

        public FrequenciesController(IFrequencyService frequencyService)
        {
            _frequencyService = frequencyService;
        }

        // GET: api/Frequencies
        [HttpGet]
        [RequirePermission("tl-v-type")]
        public async Task<ActionResult<IEnumerable<FrequencyResponse>>> GetFrequencies()
        {
            try
            {
                var frequencies = await _frequencyService.GetAllFrequenciesAsync(includeArchived: true);
                return Ok(frequencies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des fréquences : {ex.Message}");
            }
        }

        // GET: api/Frequencies/5
        [HttpGet("{id}")]
        [RequirePermission("tl-v-type")]
        public async Task<ActionResult<FrequencyResponse>> GetFrequency(int id)
        {
            try
            {
                var frequency = await _frequencyService.GetFrequencyAsync(id);

                if (frequency == null)
                    return NotFound();

                return Ok(frequency);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de la fréquence : {ex.Message}");
            }
        }

        // PUT: api/Frequencies/5
        [HttpPut("{id}")]
        [RequirePermission("tl-u-type")]
        public async Task<IActionResult> PutFrequency(int id, FrequencyResponse dto)
        {
            try
            {
                if (id != dto.FrequencyID)
                    return BadRequest();

                var updatedFrequency = await _frequencyService.UpdateFrequencyAsync(id, dto);

                if (updatedFrequency == null)
                    return NotFound();

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour de la fréquence : {ex.Message}");
            }
        }

        // POST: api/Frequencies
        [HttpPost]
        [RequirePermission("tl-c-type")]
        public async Task<ActionResult<FrequencyResponse>> PostFrequency(FrequencyResponse dto)
        {
            try
            {
                var createdFrequency = await _frequencyService.CreateFrequencyAsync(dto);

                return CreatedAtAction(nameof(GetFrequency), new { id = createdFrequency.FrequencyID }, createdFrequency);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de la fréquence : {ex.Message}");
            }
        }

        // SOFT DELETE (archive)
        [HttpPatch("archive/{id}")]
        [RequirePermission("tl-a-type")]
        public async Task<IActionResult> ArchiveFrequency(int id)
        {
            try
            {
                await _frequencyService.ArchiveFrequencyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'archivage de la fréquence : {ex.Message}");
            }
        }

        // RESTORE (désarchiver)
        [HttpPatch("restore/{id}")]
        [RequirePermission("tl-a-type")]
        public async Task<IActionResult> RestoreFrequency(int id)
        {
            try
            {
                await _frequencyService.RestoreFrequencyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la restauration de la fréquence : {ex.Message}");
            }
        }

        // HARD DELETE
        [HttpDelete("{id}")]
        [RequirePermission("tl-d-type")]
        public async Task<IActionResult> DeleteFrequency(int id)
        {
            try
            {
                await _frequencyService.DeleteFrequencyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de la fréquence : {ex.Message}");
            }
        }
    }
}
