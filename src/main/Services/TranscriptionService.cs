using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AudioTextGeneration.src.main.Services 
{
    public class TranscriptionService
    {
        private readonly string _textContainerName = "texts";
        static string azureKey = "68b2fe0f646e47408eb9589123254428";
        static string azureLocation = "eastus2";

        private StorageService _storageService;

        public TranscriptionService(StorageService storageService)
        {
            _storageService = storageService;
        }

        // Transcribes audio file from blob with name blobName in container containerName
        public async Task TranscribeFromBlob(string containerName, string blobName) 
        {
            MemoryStream audioStream = new MemoryStream();
            MemoryStream outputTextStream = new MemoryStream();
            Task audioRetrieval = _storageService.Retrieve(containerName, blobName, audioStream);

            // transcribe from audioStream and store in outputTextStream
            await audioRetrieval;
            System.Console.WriteLine("audio stream length in bytes: " + audioStream.Length);
            System.Console.WriteLine("start transcription");
            
            //temporarily calculate how long transcription takes
            var watch = System.Diagnostics.Stopwatch.StartNew();

            await Transcribe(audioStream, outputTextStream);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            System.Console.WriteLine($"Transcription done in {elapsedMs} ms");

            System.Console.WriteLine("text stream length in bytes: " + outputTextStream.Length);

            // store the text file in blob storage
            Task storeTextInBlob = _storageService.Store(_textContainerName, blobName.Replace(".wav", ".txt"), outputTextStream);

            await storeTextInBlob;
            
            audioStream.Close();
            outputTextStream.Close();
        }

        // Performs the transcription of audio stored in a stream, into a textStream representing the text
        private async Task Transcribe(MemoryStream audioStream, MemoryStream textStream)
        {
            audioStream.Position = 0; // Reset the stream position to the beginning
            textStream.Position = 0; // Reset the stream position to the beginning

            var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);
            //speechConfig.SetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguages, "fr-FR,ar-EG");

            var (sampleRate, bitsPerSample, channels) = GetWavFileParameters(audioStream);
            var audioFormat = AudioStreamFormat.GetWaveFormatPCM(sampleRate, bitsPerSample, channels);
            var audioPushStream = AudioInputStream.CreatePushStream(audioFormat);

            //write into the PushAudioInputStream object
            audioStream.Position = 44; //skip 44 bytes of header
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = audioStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                audioPushStream.Write(buffer, bytesRead);
            }
            //signal the end of the stream
            audioPushStream.Close();

            var audioConfig = AudioConfig.FromStreamInput(audioPushStream);

            // language config
            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "fr-FR", "ar-EG", "en-US"});

            // set up recognizer
            var recognizer = new SpeechRecognizer(speechConfig, autoDetectSourceLanguageConfig, audioConfig);

            var stopRecognition = new TaskCompletionSource<int>();

            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    byte[] textBytes = Encoding.UTF8.GetBytes(e.Result.Text /* + Environment.NewLine*/);
                    textStream.Write(textBytes, 0, textBytes.Length);
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine("No speech could be recognized.");
                }
            };

            recognizer.Canceled += (s, e) =>
            {
                Console.WriteLine($"Recognition canceled: {e.Reason}, {e.ErrorDetails}");
                stopRecognition.TrySetResult(0);
            };

            recognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("Session stopped.");
                stopRecognition.TrySetResult(0);
            };

            await recognizer.StartContinuousRecognitionAsync();
            Task.WaitAny(stopRecognition.Task);
            await recognizer.StopContinuousRecognitionAsync(); 
        }

        // Gets the wav format parameters of a .wav audio file
        private static (uint sampleRate, byte bitsPerSample, byte channels) GetWavFileParameters(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                reader.BaseStream.Seek(22, SeekOrigin.Begin);
                byte channels = reader.ReadByte();
                //System.Console.WriteLine(channels);
                reader.BaseStream.Seek(24, SeekOrigin.Begin);
                uint sampleRate = (uint) reader.ReadInt32();
                //System.Console.WriteLine(sampleRate);
                reader.BaseStream.Seek(34, SeekOrigin.Begin);
                byte bitsPerSample = reader.ReadByte();
                //System.Console.WriteLine(bitsPerSample);
                //reset position
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                return (sampleRate, bitsPerSample, channels);
            }
        }
         
    }
}