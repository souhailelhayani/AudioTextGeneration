using Microsoft.AspNetCore.Mvc;

namespace AudioTextGeneration.src.main.Controllers
{
    [ApiController]
    [Route("Audio")]
    public class AudioController : Controller
    {
        private readonly string storagePath = Path.Combine(Directory.GetCurrentDirectory(), @"assets\outputs");

        [HttpPost("Upload")]
        public  async Task<IActionResult> UploadAudio([FromForm] IFormFile audioFile)
        {
            //TODO optimize async later

            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Ensure the storage directory exists
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            string filePath = Path.Combine(storagePath, audioFile.FileName);
            Console.WriteLine($"file path is {filePath}");
            Console.WriteLine("file size before copying: "+audioFile.Length);

            using (var stream = System.IO.File.Create(filePath))
            {
                await audioFile.CopyToAsync(stream);
                Console.WriteLine("reached here in async");
            }
            Console.WriteLine("reached here in return");

            FileInfo fi = new FileInfo(filePath);
            Console.WriteLine("file size after copying to stream: " + fi.Length);

            //TODO convert file to wav file
            

            return Ok(new { FilePath = filePath });
        }
    }
}