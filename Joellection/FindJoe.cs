using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Joellection
{
    public static class FindJoe
    {
        [FunctionName("FindJoe")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Uploading Joe");

       

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<JoeRequest>(requestBody);
            log.LogInformation($"Attempting upload of {requestBody}");

            await BlobStorageService.UpdateJoellection(data, log);

            return new OkObjectResult(await BlobStorageService.GetJoellection(log));
        }
    }
}
