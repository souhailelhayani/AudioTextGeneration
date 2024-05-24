using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AudioTextGeneration.src.main.Services 
{
    public class TranscriptionService
    {
        private readonly string _textContainerName = "texts";

        private readonly string _tempPath = @"assets\temp";

        private StorageService _storageService;

        public TranscriptionService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task TranscribeFromBlob(string containerName, string blobName) 
        {
            // download the audio file temporarily
            Task audioRetrieval = _storageService.Retrieve(containerName, blobName, _tempPath);

            // file to be transcribed is now in the _tempDownloadPath
            string audioFilePath = Path.Combine(_tempPath, blobName);
            string textFilePath = Path.Combine(_tempPath, blobName.Replace(Path.GetExtension(blobName),".txt"));

            // transcribe the file in audioFilePath and write result to textFilePath
            await audioRetrieval;
            await Transcribe(audioFilePath, textFilePath);

            // store the text file in blob storage
            Task storeTextInBlob = _storageService.Store(_textContainerName, textFilePath);

            // delete the temp files
            _storageService.Clean(audioFilePath);

            await storeTextInBlob;
            _storageService.Clean(textFilePath);
        }

        private async Task Transcribe(string audioFilePath, string textFilePath)
        {
            // TESTING. Write text to the file
            // await File.WriteAllTextAsync(textFilePath, "Hello, World!");
        }
         
    }
}