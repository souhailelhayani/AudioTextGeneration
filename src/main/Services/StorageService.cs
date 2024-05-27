using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;

namespace AudioTextGeneration.src.main.Services 
{
    public class StorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
  
        //inject the blob service client singleton
        public StorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task Store(string containerName, IFormFile file) 
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if(!await containerClient.ExistsAsync())
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
            }

            var blobClient = containerClient.GetBlobClient(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }
        }

        public async Task Store(string containerName, string blobName, MemoryStream stream) 
        {
            // if(stream.Length == 0) {
            //     System.Console.WriteLine("length of stream 0, skipped");
            //     return;
            // }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if(!await containerClient.ExistsAsync())
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
            }

            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(stream, true);
        }

        public async Task Retrieve(string containerName, string blobName, MemoryStream stream) {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DownloadToAsync(stream);
            stream.Position = 0; // Reset the stream position to the beginning
        }

        public async Task TestStore()
        {
            string containerName = "quickstartblobs" + Guid.NewGuid().ToString();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if(!await containerClient.ExistsAsync())
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
            }

            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "data";
            Directory.CreateDirectory(localPath);
            string fileName = "quickstart" + Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);

            // Write text to the file
            await File.WriteAllTextAsync(localFilePath, "Hello, World!");

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Upload data from the local file
            await blobClient.UploadAsync(localFilePath, true);

            string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");

            Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

            // Download the blob's contents and save it to a file
            await blobClient.DownloadToAsync(downloadFilePath);

            // Clean up
            Console.Write("Press any key to begin clean up");
            Console.ReadLine();

            Console.WriteLine("Deleting blob container...");
            await containerClient.DeleteAsync();

            Console.WriteLine("Deleting the local source and downloaded files...");
            File.Delete(localFilePath);
            File.Delete(downloadFilePath);

            Console.WriteLine("Done");
        }
    }
}