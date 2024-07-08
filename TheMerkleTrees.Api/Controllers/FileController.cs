using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using TheMerkleTrees.Domain.Interfaces.Repositories;
using TheMerkleTrees.Domain.Models;
using File = TheMerkleTrees.Domain.Models.File;

namespace TheMerkleTrees.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;
        private readonly HttpClient _httpClient;

        public FilesController(IFileRepository mongoDbService, HttpClient httpClient)
        {
            _fileRepository = mongoDbService;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<List<File>> Get() =>
            await _fileRepository.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<File>> Get(string id)
        {
            var file = await _fileRepository.GetAsync(id);

            if (file is null)
            {
                return NotFound();
            }

            return file;
        }

        [HttpPost]
        public async Task<IActionResult> Post(File newFile)
        {
            await _fileRepository.CreateAsync(newFile);

            return CreatedAtAction(nameof(Get), new { id = newFile.Id }, newFile);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, File updatedFile)
        {
            var file = await _fileRepository.GetAsync(id);

            if (file is null)
            {
                return NotFound();
            }

            updatedFile.Id = file.Id;

            await _fileRepository.UpdateAsync(id, updatedFile);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var file = await _fileRepository.GetAsync(id);

            if (file is null)
            {
                return NotFound();
            }

            await _fileRepository.RemoveAsync(id);

            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<List<File>> GetFilesByUser(string userId) =>
            await _fileRepository.GetFilesByUserAsync(userId);

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string category,
            [FromForm] bool isPublic, [FromForm] string userAddress)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            byte[] fileContent;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileContent = ms.ToArray();
            }

            // Generate AES key and IV
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                byte[] encryptedContent;
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    await csEncrypt.WriteAsync(fileContent, 0, fileContent.Length);
                    await csEncrypt.FlushFinalBlockAsync();
                    encryptedContent = msEncrypt.ToArray();
                }

                // Upload encrypted file to IPFS
                var formData = new MultipartFormDataContent();
                formData.Add(new ByteArrayContent(encryptedContent), "file", file.FileName);

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
                    Owner = userAddress,
                    Key = Convert.ToBase64String(aes.Key),
                    IV = Convert.ToBase64String(aes.IV),
                    Extension = Path.GetExtension(file.FileName) // Store the file extension
                };

                await _fileRepository.CreateAsync(fileRecord);

                return Ok(new { Message = "File uploaded successfully", Url = url });
            }
        }


        [HttpGet("decrypt/{id}")]
        public async Task<IActionResult> DecryptFile(string id)
        {
            var file = await _fileRepository.GetAsync(id);
            if (file == null)
            {
                return NotFound("Fichier non trouvé.");
            }

            // Utiliser une passerelle publique IPFS pour récupérer le contenu chiffré
            // var response = await _httpClient.GetAsync($"https://ipfs.io/ipfs/{file.Hash}");
            var response = await _httpClient.GetAsync($"ipfs://{file.Hash}");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Impossible de récupérer le fichier depuis IPFS.");
            }

            var encryptedContent = await response.Content.ReadAsByteArrayAsync();

            // Déchiffrer le contenu
            byte[] key = Convert.FromBase64String(file.Key);
            byte[] iv = Convert.FromBase64String(file.IV);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var msDecrypt = new MemoryStream(encryptedContent))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var msPlain = new MemoryStream())
                {
                    await csDecrypt.CopyToAsync(msPlain);
                    byte[] decryptedContent = msPlain.ToArray();

                    // Retourner le contenu déchiffré avec la bonne extension
                    return File(decryptedContent, "application/octet-stream", file.Name);
                }
            }
        }

        private class AddResponse
        {
            public string Hash { get; set; }
        }
    }
}