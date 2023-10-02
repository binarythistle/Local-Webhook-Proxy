using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;

namespace Company.Function
{
    public static class HttpTrigger1
    {

        static readonly string conectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        static readonly string marketplacerTopicName = Environment.GetEnvironmentVariable("MarketplacerTopic");
        static readonly string marketplacerHeader = Environment.GetEnvironmentVariable("MarketplacerHeader");
        static readonly string shopifyTopicName = Environment.GetEnvironmentVariable("ShopifyTopic");
        static readonly string shopifyHeader = Environment.GetEnvironmentVariable("ShopifyHeader");
        
        static ServiceBusClient client;
        static ServiceBusSender sender;

        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Webhook Event Received");


            string topicToUse;

            if (req.Headers.ContainsKey(marketplacerHeader))
            {
                Console.WriteLine("--> Marketplacer webhook");
                topicToUse = marketplacerTopicName;
            }
            else if (req.Headers.ContainsKey(shopifyHeader))
            {
                Console.WriteLine("--> Shopify webhook");
                topicToUse = shopifyTopicName;
            }
            else
            {
                Console.WriteLine("--> Unrecognised webhook, return");
                return new OkObjectResult("No valid routing header.");
            }


            
            WebhookCreateDto webhookEvent = new()
            {
                Headers = JsonConvert.SerializeObject(req.Headers),
                Body = await new StreamReader(req.Body).ReadToEndAsync()
            };
            

            //TO DO: Distinguish between Marketplacer and Shopify events

            client = new ServiceBusClient(conectionString);
            sender = client.CreateSender(topicToUse);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();


            if (!messageBatch.TryAddMessage(new ServiceBusMessage(JsonConvert.SerializeObject(webhookEvent))))
            {
                throw new Exception("The message is too large to fit in the batch");
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of messages has been published to the topic.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            return new OkObjectResult("Webhook Payload Received from Local Webhook Proxy");
        }
    }
}
