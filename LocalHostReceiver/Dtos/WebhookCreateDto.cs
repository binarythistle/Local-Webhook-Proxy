namespace LocalHostReceiver.Dtos;

public class WebhookCreateDto
{
    public required string Headers { get; set; }

    public required string Body { get; set; }
}