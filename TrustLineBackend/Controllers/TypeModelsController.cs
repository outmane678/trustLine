using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.Helpers;



namespace AnonymousComplaintsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TypeModelsController : ControllerBase
    {
        private readonly ITypeService _typeService;

        public TypeModelsController(ITypeService typeService)
        {
            _typeService = typeService;
        }

        /// <summary>
        /// Get types with server-side pagination and filtering
        /// </summary>
        /// <param name="request">Pagination and filter parameters</param>
        [HttpGet]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<PaginatedResponse<TypeModelResponse>>> GetTypes([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _typeService.GetTypesPaginatedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des types paginés : {ex.Message}");
            }
        }

       
        // GET: api/TypeModels/all
        // Récupère tous les types, y compris ceux archivés
        [HttpGet("all")]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<IEnumerable<TypeModelResponse>>> GetAllTypes()
        {
            try
            {
                var types = await _typeService.GetAllTypesAsync(includeArchived: true, includeCategories: true);
                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de tous les types : {ex.Message}");
            }
        }

        // GET: api/TypeModels/5
        // Récupère un type par son ID
        [HttpGet("{id}")]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<TypeModelResponse>> GetTypeModel(int id)
        {
            try
            {
                var typeModel = await _typeService.GetTypeAsync(id, includeCategories: true);

                if (typeModel == null)
                    return NotFound();

                return Ok(typeModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération du type : {ex.Message}");
            }
        }

        // PUT: api/TypeModels/5
        // Met à jour un type
        [HttpPut("{id}")]
        [RequirePermission("tl-u-type")]
        public async Task<ActionResult<TypeModelResponse>> UpdateTypeModel(int id, [FromBody] TypeModelResponse dto)
        {
            try
            {
                var updatedType = await _typeService.UpdateTypeAsync(id, dto);

                if (updatedType == null)
                    return NotFound();

                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour du type : {ex.Message}");
            }
        }

        // POST: api/TypeModels
        // Crée un nouveau type
            [HttpPost]
            [RequirePermission("tl-c-type")]
            public async Task<ActionResult<TypeModelResponse>> PostTypeModel(TypeModelResponse dto)
            {
                try
                {
                    var createdType = await _typeService.CreateTypeAsync(dto);

                    return CreatedAtAction("GetTypeModel", new { id = createdType.TypeId }, createdType);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erreur lors de la création du type : {ex.Message}");
                }
            }

            [HttpPatch("archive/{id}")]
            [RequirePermission("tl-d-type")]
            public async Task<IActionResult> ArchiveTypeModel(int id)
            {
                try
                {
                    await _typeService.ArchiveTypeWithCategoriesAsync(id);
                    return NoContent();
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erreur lors de l'archivage du type : {ex.Message}");
                }
            }


            [HttpPut("restore/{id}")]
            [RequirePermission("tl-a-type")]
            public async Task<IActionResult> RestoreTypeModel(int id)
            {
                try
                {
                    await _typeService.RestoreTypeWithCategoriesAsync(id);
                    return NoContent();
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erreur lors de la restauration du type : {ex.Message}");
                }
            }



                // DELETE: api/TypeModels/5
                // Supprime définitivement un type
            [HttpDelete("{id}")]
            [RequirePermission("tl-d-type")]
            public async Task<IActionResult> DeleteTypeModel(int id)
            {
                try
                {
                    await _typeService.ArchiveTypeWithCategoriesAsync(id);
                    return NoContent();
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erreur lors de la suppression du type : {ex.Message}");
                }
            }

       
    }
}
