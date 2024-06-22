using Microsoft.AspNetCore.Mvc;
using WebThree.api.Services;

namespace WebThree.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;
    public FilesController(MongoDBService mongoDbService) =>
        _mongoDBService = mongoDbService;

    [HttpGet]
    public async Task<List<File>> Get() =>
        await _mongoDBService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<File>> Get(string id)
    {
        var file = await _mongoDBService.GetAsync(id);

        if (file is null)
        {
            return NotFound();
        }

        return file;
    }

    [HttpPost]
    public async Task<IActionResult> Post(File newFile)
    {
        await _mongoDBService.CreateAsync(newFile);

        return CreatedAtAction(nameof(Get), new { id = newFile.Id }, newFile);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, File updatedFile)
    {
        var file = await _mongoDBService.GetAsync((id));

        if (file is null)
        {
            return NotFound();
        }

        updatedFile.Id = file.Id;

        await _mongoDBService.UpdateAsync(id, updatedFile);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var file = await _mongoDBService.GetAsync(id);

        if (file is null)
        {
            return NotFound();
        }

        await _mongoDBService.RemoveAsync((id));

        return NoContent();
    }

    [HttpGet("user/{userId}")]
    public async Task<List<File>> GetFilesByUser(string userId) =>
        await _mongoDBService.GetFilesByUserAsync(userId);
}