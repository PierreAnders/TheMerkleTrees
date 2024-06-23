using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebThree.api.Services;

namespace WebThree.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly HttpClient _httpClient;

        public FilesController(MongoDBService mongoDbService, HttpClient httpClient)
        {
            _mongoDBService = mongoDbService;
            _httpClient = httpClient;
        }

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
            var file = await _mongoDBService.GetAsync(id);

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

            await _mongoDBService.RemoveAsync(id);

            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<List<File>> GetFilesByUser(string userId) =>
            await _mongoDBService.GetFilesByUserAsync(userId);

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string category, [FromForm] bool isPublic, [FromForm] string userAddress)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Convert file to byte array
            byte[] fileContent;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileContent = ms.ToArray();
            }

            // Upload file to IPFS
            var formData = new MultipartFormDataContent();
            formData.Add(new ByteArrayContent(fileContent), "file", file.FileName);

            var response = await _httpClient.PostAsync("http://localhost:5001/api/v0/add", formData);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AddResponse>();
            var cid = result.Hash;
            var url = $"ipfs://{cid}";

            // Save file metadata to database
            var fileRecord = new File
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = file.FileName,
                Hash = cid,
                Category = category,
                IsPublic = isPublic,
                Owner = userAddress
            };

            await _mongoDBService.CreateAsync(fileRecord);

            return Ok(new { Message = "File uploaded successfully", Url = url });
        }

        private class AddResponse
        {
            public string Hash { get; set; }
        }
    }
}