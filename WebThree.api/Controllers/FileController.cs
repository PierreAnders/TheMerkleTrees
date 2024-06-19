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

    [HttpGet("{hash}")]
    public async Task<ActionResult<File>> Get(string hash)
    {
        var file = await _mongoDBService.GetAsync(hash);

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

        return CreatedAtAction(nameof(Get), new { hash = newFile.Hash }, newFile);
    }

    [HttpPut("{hash}")]
    public async Task<IActionResult> Update(string hash, File updatedFile)
    {
        var file = await _mongoDBService.GetAsync((hash));

        if (file is null)
        {
            return NotFound();
        }

        updatedFile.Hash = file.Hash;

        await _mongoDBService.UpdateAsync(hash, updatedFile);

        return NoContent();
    }

    [HttpDelete("{hash}")]
    public async Task<IActionResult> Delete(string hash)
    {
        var file = await _mongoDBService.GetAsync(hash);

        if (file is null)
        {
            return NotFound();
        }

        await _mongoDBService.RemoveAsync((hash));

        return NoContent();
    }
}