using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AudioTextGeneration.src.main.Services 
{
    public class TranscriptionService
    {
        private readonly string _textContainerName = "texts";

        private StorageService _storageService;

        public TranscriptionService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task TranscribeFromBlob(string containerName, string blobName) 
        {
            // download the audio file temporarily
            MemoryStream audioStream = new MemoryStream();
            MemoryStream outputTextStream = new MemoryStream();
            Task audioRetrieval = _storageService.Retrieve(containerName, blobName, audioStream);

            // transcribe from audioStream and store in outputTextStream
            await audioRetrieval;
            System.Console.WriteLine("audio stream length in bytes: " + audioStream.Length + audioStream.CanRead + audioStream.CanWrite);

            await Transcribe(audioStream, outputTextStream);

            System.Console.WriteLine("text stream length in bytes: " + outputTextStream.Length + outputTextStream.CanRead + outputTextStream.CanWrite);

            // store the text file in blob storage
            Task storeTextInBlob = _storageService.Store(_textContainerName, blobName.Replace(".wav", ".txt"), outputTextStream);

            await storeTextInBlob;
            
            audioStream.Close();
            outputTextStream.Close();
        }

        private async Task Transcribe(MemoryStream audioStream, MemoryStream textStream)
        {
            // TESTING. Write text to the file
            // await File.WriteAllTextAsync(textFilePath, "Hello, World!");

            audioStream.Position = 0; // Reset the stream position to the beginning
            textStream.Position = 0; // Reset the stream position to the beginning
        }
         
    }
}