using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AnonymousComplaintsAPI.DTOs.Requests;
using AnonymousComplaintsAPI.DTOs.Responses;
using AnonymousComplaintsAPI.Services.Interfaces;
using AnonymousComplaintsAPI.Helpers;

namespace AnonymousComplaintsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get categories with server-side pagination and filtering
        /// </summary>
        /// <param name="request">Pagination and filter parameters</param>
        [HttpGet]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<PaginatedResponse<CategoryResponse>>> GetCategories([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _categoryService.GetCategoriesPaginatedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des catégories paginées : {ex.Message}");
            }
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
        //{
        //    try
        //    {
        //        var categories = await _categoryService.GetAllCategoriesAsync(includeArchived: false);
        //        return Ok(categories);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Erreur lors de la récupération des catégories : {ex.Message}");
        //    }
        //}



        [HttpGet("byType/{typeId}")]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategoriesByType(int typeId)
        {
            try
            {
                var categories = await _categoryService.GetCategoriesByTypeAsync(typeId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des catégories par type : {ex.Message}");
            }
        }


        [HttpGet("listByType/{typeId}")]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategoriesListByType(int typeId)
        {
            try
            {
                var categories = await _categoryService.GetCategoriesByTypeAsync(typeId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des catégories par type : {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [RequirePermission("tl-c-report")]
        public async Task<ActionResult<CategoryResponse>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryAsync(id);

                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de la catégorie : {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [RequirePermission("tl-u-category")]
        public async Task<IActionResult> PutCategory(int id, UpdateCategoryRequest dto)
        {
            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, dto);

                if (updatedCategory == null)
                    return NotFound();

                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour de la catégorie : {ex.Message}");
            }
        }

        [HttpPost]
        [RequirePermission("tl-c-category")]
        public async Task<ActionResult<CategoryResponse>> PostCategory(CategoryResponse dto)
        {
            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(dto);

                return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de la catégorie : {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [RequirePermission("tl-d-category")]
        public async Task<IActionResult> SoftDeleteCategory(int id)
        {
            try
            {
                await _categoryService.ArchiveCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'archivage de la catégorie : {ex.Message}");
            }
        }


        [HttpPut("restore/{id}")]
        [RequirePermission("tl-a-category")]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            try
            {
                await _categoryService.RestoreCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la restauration de la catégorie : {ex.Message}");
            }
        }

        [HttpDelete("hard-delete/{id}")]
        [RequirePermission("tl-d-category")]
        public async Task<IActionResult> HardDeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de la catégorie : {ex.Message}");
            }
        }
    }
}
