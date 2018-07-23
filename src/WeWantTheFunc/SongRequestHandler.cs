
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace WeWantTheFunc
{
    public static class SongRequestHandler
    {
        [FunctionName("SongRequestHandler")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, 
            TraceWriter log)
        {
            // Get the body of the request
            var requestBody = new StreamReader(req.Body).ReadToEnd();

            // Check the header for the event type.            
            if (!req.Headers.TryGetValue("Aeg-Event-Type", out var headerValues))
                return new BadRequestObjectResult("Not a valid request");

            var eventTypeHeaderValue = headerValues.FirstOrDefault();
            if (eventTypeHeaderValue == "SubscriptionValidation")
            {
                // Validate the subscription
                var events = JsonConvert.DeserializeObject<EventGridEvent[]>(requestBody);
                dynamic data = events[0].Data;
                var validationCode = data["validationCode"];
                return new JsonResult(new
                {
                    validationResponse = validationCode
                });
            }
            else if (eventTypeHeaderValue == "Notification")
            {
                // Handle the song request
                log.Info(requestBody);
                var events = JsonConvert.DeserializeObject<EventGridEvent[]>(requestBody);

                // Reject the request if it does not
                // match the genre for the station.
                if (events[0].Subject != "genre/blues")
                    return new BadRequestObjectResult("Sorry, this is a blues station");

                return new OkObjectResult("");
            }

            return new BadRequestObjectResult("Not a valid request");
        }
    }
}
