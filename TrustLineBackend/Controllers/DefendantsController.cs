//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using AnonymousComplaintsAPI.Models;
//using AnonymousComplaintsAPI.DTOs.Responses;
//using AnonymousComplaintsAPI.Models.Entities;
 

//namespace AnonymousComplaintsAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class DefendantsController : ControllerBase
//    {
//        private readonly AnonymousComplaintsV002Context _context;

//        public DefendantsController(AnonymousComplaintsV002Context context)
//        {
//            _context = context;
//        }

//        // GET: api/Defendants
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<DefendantResponse>>> GetDefendants()
//        {
//            return await _context.Defendants
//                //.Where(d => (d.Archived) !=true)
//                .Select(d => new DefendantDTO
//                {
//                    UserID = d.UserId,
//                    AnonymousComplaintID = d.AnonymousComplaintId
//                })
//                .ToListAsync();
//        }

//        // GET: api/Defendants/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<DefendantDTO>> GetDefendant(int id)
//        {
//            var defendant = await _context.Defendants.FindAsync(id);

//            if (defendant == null)
//            {
//                return NotFound();
//            }

//            var dto = new DefendantDTO
//            {
//                UserID = defendant.UserId,
//                AnonymousComplaintID = defendant.AnonymousComplaintId
//            };

//            return dto;
//        }

//        // PUT: api/Defendants/5
//        [HttpPut("{id}")]
//        public async Task<IActionResult> PutDefendant(int id, DefendantDTO dto)
//        {
//            if (id != dto.AnonymousComplaintID)
//            {
//                return BadRequest();
//            }

//            var d = await _context.Defendants.FindAsync(id);
//            if (d == null) return NotFound();

//            d.UserId = dto.UserID;
//            d.AnonymousComplaintId = dto.AnonymousComplaintID;

//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        // POST: api/Defendants
//        [HttpPost]
//        public async Task<ActionResult<DefendantDTO>> PostDefendant(DefendantDTO dto)
//        {
//            var defendant = new Defendant
//            {
//                UserId = dto.UserID,
//                AnonymousComplaintId = dto.AnonymousComplaintID
//            };

//            _context.Defendants.Add(defendant);
//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateException)
//            {
//                if (DefendantExists(defendant.AnonymousComplaintId))
//                {
//                    return Conflict();
//                }
//                else
//                {
//                    throw;
//                }
//            }

//            return CreatedAtAction("GetDefendant", new { id = defendant.AnonymousComplaintId }, dto);
//        }

//        // DELETE: api/Defendants/5
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteDefendant(int id)
//        {
//            var d = await _context.Defendants.FindAsync(id);
//            if (d == null) return NotFound();

//            _context.Defendants.Remove(d);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        private bool DefendantExists(int id)
//        {
//            return _context.Defendants.Any(e => e.AnonymousComplaintId == id);
//        }
//    }
//}
