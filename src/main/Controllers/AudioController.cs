using AudioTextGeneration.src.main.Services;
using Microsoft.AspNetCore.Mvc;

namespace AudioTextGeneration.src.main.Controllers
{
    [ApiController]
    [Route("Audio")]
    public class AudioController : Controller
    {
        private readonly string _audioContainerName = "audios";
        private TranscriptionService _transcriptionService;
        private StorageService _storageService;

        public AudioController(TranscriptionService transcriptionService, StorageService storageService)
        {   
            _transcriptionService = transcriptionService;
            _storageService = storageService;
        }

        // Uploads an audio file and performs its transcription. Stores both audio and text files in blob storage
        // Returns the transcribed text in a text file
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadAudioAndTranscribe([FromForm] IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            
            if(Path.GetExtension(audioFile.FileName).ToLowerInvariant() != ".wav") 
            {
                return BadRequest("Audio file not in wav format");
            }

            //save audio file in blob storage
            await _storageService.Store(_audioContainerName, audioFile);
            

            var textStream = new MemoryStream();
            await _transcriptionService.TranscribeFromBlob(_audioContainerName, audioFile.FileName, textStream);
            return File(textStream, "text/plain", audioFile.FileName.Replace(".wav", ".txt"));

        }

        // [HttpPost("test")]
        // public async Task<String> Test() 
        // {
        //     await _storageService.TestStore();

        //     return "testing";
        // }
    }
}