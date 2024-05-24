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

        public void Retrieve() {

        }
    }
}