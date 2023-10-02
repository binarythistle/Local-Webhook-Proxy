using System.Security.Cryptography.X509Certificates;
using Azure.Messaging.ServiceBus;
using LocalHostReceiver;
using LocalHostReceiver.Dtos;
using LocalHostReceiver.HttpServices;
using LocalHostReceiver.Utilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .Build();

//This is stored in User Secrets
string connectionString = configuration["ServiceBusConnectionString"]!;
string marketplacerTopicName = configuration["MarketplacerTopic"]!;
string marketplacerSubscriptionName = configuration["MarketplacerSubscription"]!;
string shopifyTopicName = configuration["ShopifyTopic"]!;
string shopifySubscriptionName = configuration["ShopifySubscription"]!;

string forwardDestination = configuration["ForwardDestination"]!;


ServiceBusClient client = new(connectionString);
ServiceBusProcessor processor1 = client.CreateProcessor(marketplacerTopicName, marketplacerSubscriptionName, new ServiceBusProcessorOptions());
ServiceBusProcessor processor2 = client.CreateProcessor(shopifyTopicName, shopifySubscriptionName, new ServiceBusProcessorOptions());
HttpPostService postService = new();
TextLoader textLoader  = new();

ConsoleWriter.PrintColorMessage("--> Loading permitted headers...", ConsoleColor.Magenta);

List<string> permittedHeaders = textLoader.LoadPermittedHeaders("PermittedHeaders.txt");
ConsoleWriter.PrintColorMessage($"---> {permittedHeaders.Count} headers loaded", ConsoleColor.Magenta);


ConsoleWriter.PrintColorMessage("--> Connecting to Service Bus...", ConsoleColor.Magenta);

try
{
    processor1.ProcessMessageAsync += MessageHandler;
    processor2.ProcessMessageAsync += MessageHandler;

    processor1.ProcessErrorAsync += ErrorHandler;
    processor2.ProcessErrorAsync += ErrorHandler;

    await processor1.StartProcessingAsync();
    await processor2.StartProcessingAsync();

    ConsoleWriter.PrintColorMessage($"\n---> Listenting on Topic {marketplacerTopicName} on Subscription {marketplacerSubscriptionName}", ConsoleColor.White);
    ConsoleWriter.PrintColorMessage($"\n---> Listenting on Topic {shopifyTopicName} on Subscription {shopifySubscriptionName}", ConsoleColor.DarkGreen);
    Console.ReadKey();

    ConsoleWriter.PrintColorMessage("\n---> Stopping the receiver...", ConsoleColor.White);
    await processor1.StopProcessingAsync();
    ConsoleWriter.PrintColorMessage("\n--> Stopped Receiving Messages", ConsoleColor.White);
}
finally
{
    await processor1.DisposeAsync();
    await client.DisposeAsync();
}


async Task MessageHandler(ProcessMessageEventArgs args)
{
    string payload = args.Message.Body.ToString();

    var json = JsonConvert.DeserializeObject<WebhookCreateDto>(payload);

    //ConsoleWriter.PrintColorMessage($"---> Headers: {json!.Headers}", ConsoleColor.Yellow);
    //ConsoleWriter.PrintColorMessage($"---> Body: {json!.Body}", ConsoleColor.Blue);

    await args.CompleteMessageAsync(args.Message);

    if (await postService.ForwardWebhookAsync(json!, forwardDestination, permittedHeaders))
        ConsoleWriter.PrintColorMessage("---> Webhook forwarded OK", ConsoleColor.Green);
    else
        ConsoleWriter.PrintColorMessage("---> Issue forwarding webhook", ConsoleColor.Green);    

}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    ConsoleWriter.PrintColorMessage($"--> Service Bus Error: {args.Exception}", ConsoleColor.Red);
    return Task.CompletedTask;
}


