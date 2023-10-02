using System.Text;
using LocalHostReceiver.Dtos;
using Newtonsoft.Json;

namespace LocalHostReceiver.HttpServices;

public class HttpPostService
{
    public async Task<bool> ForwardWebhookAsync(WebhookCreateDto webhookCreateDto, string destination, List<string> permittedHeaders)
    {
        var httpContent = new StringContent(
            webhookCreateDto.Body,
            Encoding.UTF8,
            "application/json"
        );

        HttpClient client = new HttpClient();

        Dictionary<string, List<string>> headersDictionary = new();
        headersDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(webhookCreateDto!.Headers) ?? new Dictionary<string, List<string>>();

        
        if (headersDictionary != null)
        {
            ConsoleWriter.PrintColorMessage("---> Attaching headers...", ConsoleColor.Green);

            foreach (var header in headersDictionary)
            {
                

                foreach (string perm in permittedHeaders)
                {
                    if (perm.ToLower() == header.Key.ToLower())
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value[0]);
                        ConsoleWriter.PrintColorMessage($"----> {header.Key}", ConsoleColor.DarkGreen);
                        ConsoleWriter.PrintColorMessage($"-----> {header.Value[0]}", ConsoleColor.DarkBlue);
                    }
                }

            }
            ConsoleWriter.PrintColorMessage("---> Headers attached", ConsoleColor.Green);
        }
        else
        {
            ConsoleWriter.PrintColorMessage("---> Something went wrong desrializing the headers...", ConsoleColor.Red);
        }
        
        ConsoleWriter.PrintColorMessage("---> Forwarding request...", ConsoleColor.Green);
        var response = await client.PostAsync(destination, httpContent);

        if (response.IsSuccessStatusCode)
            return true;

        return false;

    }
}