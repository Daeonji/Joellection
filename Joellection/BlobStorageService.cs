using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Logging;
using Azure.Storage;
using Azure.Core;

namespace Joellection
{
    public static class BlobStorageService
    {
    
        public static async Task<List<JoeEntry>> GetJoellection (ILogger log)
        {
            try
            {
                BlobContainerClient blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "test");

                BlobClient blobClient = blobContainerClient.GetBlobClient("joellection");
       
                MemoryStream download = new MemoryStream();
                if (!blobClient.Exists())
                {
                    var newFile = JsonConvert.SerializeObject(new List<JoeEntry>());
                    await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(newFile)), true);
                    return new List<JoeEntry>();
                }
                await blobClient.DownloadToAsync(download);
                download.Position = 0;
                
                string reader;
                using (StreamReader sr = new StreamReader(download))
                {
                    reader = sr.ReadToEnd();
                    if (reader == string.Empty)
                    {
                        log.LogInformation("string is empty");
                        return new List<JoeEntry>();
                    }
                    var response = JsonConvert.DeserializeObject<List<JoeEntry>>(reader);
                    return response;
                }
                
            }
            catch(Exception ex)
            {
                log.LogError(ex.Message);
                log.LogError(ex.StackTrace);
                throw new Exception("fuck");
            }
            
        }

        public static async Task UpdateJoellection(JoeRequest req, ILogger log)
        {
            Random rnd = new Random();
            var currentCollection = await GetJoellection(log);

            var updateRequest = new JoeEntry();
            updateRequest.Name = req.Name;
            updateRequest.Description = req.Description;
            updateRequest.ImageLink = req.ImageLink;
            updateRequest.JoeID = rnd.Next(200000).ToString();


            currentCollection.Add(updateRequest);

            BlobContainerClient blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "test");

            BlobClient blobClient = blobContainerClient.GetBlobClient("joellection");
            var request = JsonConvert.SerializeObject(currentCollection);
            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(request)), true);

        }

        public static async Task<bool> DeleteJoe(JoeEntry req, ILogger log)
        {
            try
            {
                var currentCollection = await GetJoellection(log);
                log.LogInformation($"Current Collection: {currentCollection.Count}");
                
                


                var itemToRemove = currentCollection.Single(r => r.JoeID == req.JoeID);
                currentCollection.Remove(itemToRemove);
                log.LogInformation($"Current Collection now: {currentCollection.Count}");

                BlobContainerClient blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "test");

                BlobClient blobClient = blobContainerClient.GetBlobClient("joellection");
                var request = JsonConvert.SerializeObject(currentCollection);
                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(request)), true);
                return true;
            }catch (Exception ex)
            {
                return false;
            }
        }
    }
}
