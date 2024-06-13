using Microsoft.AspNetCore.Mvc;

namespace WebThree.api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FilesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> PostFile([FromBody] File file)
    {
        if (file == null)
        {
            return BadRequest();
        }

        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFile), new { id = file.Id }, file);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFile(int id)
    {
        var file = await _context.Files.FindAsync(id);

        if (file == null)
        {
            return NotFound();
        }

        return Ok(file);
    }
}