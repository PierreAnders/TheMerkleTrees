using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using TheMerkleTrees.Domain.Interfaces.Repositories;
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

        // [HttpGet("{id:length(24)}")]
        // public async Task<ActionResult<File>> Get(string id)
        // {
        //     var file = await _fileRepository.GetAsync(id);
        //
        //     if (file is null)
        //     {
        //         return NotFound();
        //     }
        //
        //     return file;
        // }

        [HttpPost]
        public async Task<IActionResult> Post(File newFile)
        {
            await _fileRepository.CreateAsync(newFile);

            return CreatedAtAction(nameof(Get), new { id = newFile.Id }, newFile);
        }

        // [HttpPut("{id:length(24)}")]
        // public async Task<IActionResult> Update(string id, File updatedFile)
        // {
        //     var file = await _fileRepository.GetAsync(id);
        //
        //     if (file is null)
        //     {
        //         return NotFound();
        //     }
        //
        //     updatedFile.Id = file.Id;
        //
        //     await _fileRepository.UpdateAsync(id, updatedFile);
        //
        //     return NoContent();
        // }

        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            await _fileRepository.RemoveAsync(name, userId);

            return NoContent();
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<File>>> GetFilesByUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var files = await _fileRepository.GetFilesByUserAsync(userId);

            return files;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string category,
            [FromForm] bool isPublic, [FromForm] string userAddress)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }
            
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

            byte[] encryptedContent;
            string key = null;
            string iv = null;

            if (!isPublic)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.GenerateKey();
                    aes.GenerateIV();
                    key = Convert.ToBase64String(aes.Key);
                    iv = Convert.ToBase64String(aes.IV);

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        await csEncrypt.WriteAsync(fileContent, 0, fileContent.Length);
                        await csEncrypt.FlushFinalBlockAsync();
                        encryptedContent = msEncrypt.ToArray();
                    }
                }
            }
            else
            {
                encryptedContent = fileContent;
            }
            
            var formData = new MultipartFormDataContent();
            formData.Add(new ByteArrayContent(encryptedContent), "file", file.FileName);

            var response = await _httpClient.PostAsync("http://localhost:5001/api/v0/add", formData);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AddResponse>();
            var cid = result.Hash;
            var url = $"ipfs://{cid}";
            
            var fileRecord = new File
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = file.FileName,
                Hash = cid,
                Category = category,
                IsPublic = isPublic,
                Owner = userId,
                Key = key,
                IV = iv,
                Extension = Path.GetExtension(file.FileName) // Store the file extension
            };

            await _fileRepository.CreateAsync(fileRecord);

            return Ok(new { Message = "File uploaded successfully", Url = url });
        }

        [HttpGet("decrypt/{name}")]
        public async Task<IActionResult> DecryptFile(string name)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var file = await _fileRepository.GetAsync(name, userId);
            if (file == null)
            {
                return NotFound("Fichier non trouvé.");
            }

            byte[] fileContent = null;
            try
            {
                fileContent = await GetFileFromLocalIPFSNode(file.Hash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération via le nœud local : {ex.Message}");
                return BadRequest("Impossible de récupérer le fichier depuis IPFS.");
            }

            if (file.IsPublic)
            {
                return File(fileContent, "application/octet-stream", file.Name);
            }

            try
            {
                byte[] key = Convert.FromBase64String(file.Key);
                byte[] iv = Convert.FromBase64String(file.IV);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var msDecrypt = new MemoryStream(fileContent))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var msPlain = new MemoryStream())
                    {
                        await csDecrypt.CopyToAsync(msPlain);
                        byte[] decryptedContent = msPlain.ToArray();

                        return File(decryptedContent, "application/octet-stream", file.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du déchiffrement : {ex.Message}");
                return StatusCode(500, "Erreur interne du serveur lors du déchiffrement du fichier.");
            }
        }

        private async Task<byte[]> GetFileFromLocalIPFSNode(string cid)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ipfs",
                Arguments = $"cat {cid}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                using (var ms = new MemoryStream())
                {
                    await process.StandardOutput.BaseStream.CopyToAsync(ms);
                    process.WaitForExit();
                    return ms.ToArray();
                }
            }
        }

        private class AddResponse
        {
            public string Hash { get; set; }
        }
    }
}