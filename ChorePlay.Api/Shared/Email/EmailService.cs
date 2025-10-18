using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using ChorePlay.Api.Shared.Configuration;

namespace ChorePlay.Api.Shared.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null);
}

public class SesEmailService : IEmailService
{
    private readonly SesSettings _settings;
    private readonly IAmazonSimpleEmailService _sesClient;

    public SesEmailService(SesSettings settings)
    {
        _settings = settings;
        _sesClient = new AmazonSimpleEmailServiceClient(
            _settings.AccessKey,
            _settings.SecretKey,
            RegionEndpoint.GetBySystemName(_settings.Region)
        );
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null
    )
    {
        var request = new SendEmailRequest
        {
            Source = _settings.SenderEmail,
            Destination = new Destination { ToAddresses = [to] },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content(htmlBody),
                    Text = textBody != null ? new Content(textBody) : null,
                },
            },
        };
        await _sesClient.SendEmailAsync(request);
    }
}
