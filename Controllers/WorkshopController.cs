using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class WorkshopController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkshopController(AppDbContext context)
    {
        _context = context;
    }

   
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkshopDto>>> GetWorkshops()
    {
        var workshops = await _context.Workshops
            .Select(w => new WorkshopDto
            {
                Id = w.Id,
                Name = w.Name,
                Responsible = w.Responsible
            })
            .ToListAsync();

        return Ok(workshops);
    }

 
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkshopDto>> GetWorkshop(int id)
    {
        var workshop = await _context.Workshops
            .Where(w => w.Id == id)
            .Select(w => new WorkshopDto
            {
                Id = w.Id,
                Name = w.Name,
                Responsible = w.Responsible
            })
            .FirstOrDefaultAsync();

        if (workshop == null)
        {
            return NotFound();
        }

        return Ok(workshop);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkshop([FromBody] CreateWorkshopDto dto)
    {
        if (dto == null) return BadRequest("Invalid data.");

        var workshop = new Workshop
        {
            Name = dto.Name,
            Responsible = dto.Responsible
        };

        _context.Workshops.Add(workshop);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkshop), new { id = workshop.Id }, new WorkshopDto
        {
            Id = workshop.Id,
            Name = workshop.Name,
            Responsible = workshop.Responsible
        });
    }



    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkshop(int id, [FromBody] UpdateWorkshopDto dto)
    {
        var workshop = await _context.Workshops.FindAsync(id);
        if (workshop == null) return NotFound();

        workshop.Name = dto.Name;
        workshop.Responsible = dto.Responsible;

        _context.Workshops.Update(workshop);
        await _context.SaveChangesAsync();

        return Ok(new WorkshopDto
        {
            Id = workshop.Id,
            Name = workshop.Name,
            Responsible = workshop.Responsible
        });
    }

   
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkshop(int id)
    {
        var workshop = await _context.Workshops.FindAsync(id);
        if (workshop == null)
        {
            return NotFound();
        }

        _context.Workshops.Remove(workshop);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}



public class CreateWorkshopDto
{
    public string Name { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
}

public class UpdateWorkshopDto
{
    public string Name { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
}
