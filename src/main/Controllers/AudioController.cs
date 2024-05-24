using AudioTextGeneration.src.main.Services;
using Microsoft.AspNetCore.Mvc;

namespace AudioTextGeneration.src.main.Controllers
{
    [ApiController]
    [Route("Audio")]
    public class AudioController : Controller
    {
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), @"assets\outputs");
        private readonly string _audioContainerName = "audios";
        private TranscriptionService _transcriptionService;
        private StorageService _storageService;

        public AudioController(TranscriptionService transcriptionService, StorageService storageService)
        {   
            _transcriptionService = transcriptionService;
            _storageService = storageService;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadAudioAndTranscribe([FromForm] IFormFile audioFile)
        {
            //TODO optimize async later
            //TODO security ??

            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            
            if(Path.GetExtension(audioFile.FileName).ToLowerInvariant() != ".wav") 
            {
                return BadRequest("Audio file not in wav format");
            }

            // copy the file locally

            // Ensure the storage directory exists
            // if (!Directory.Exists(_storagePath))
            // {
            //     Directory.CreateDirectory(_storagePath);
            // }

            // string filePath = Path.Combine(_storagePath, audioFile.FileName);
            // using (var stream = System.IO.File.Create(filePath))
            // {
            //     await audioFile.CopyToAsync(stream);
            // }

            //save audio file in blob storage
            await _storageService.Store(_audioContainerName, audioFile);

            var audioTranscribingTask = _transcriptionService.TranscribeFromBlob(_audioContainerName, audioFile.FileName);
            
            await audioTranscribingTask;

            return Ok("audio uploaded and transcribed successsfully");
        }

        [HttpPost("test")]
        public async Task<String> Test() 
        {
            await _storageService.TestStore();

            return "testing";
        }
    }
}