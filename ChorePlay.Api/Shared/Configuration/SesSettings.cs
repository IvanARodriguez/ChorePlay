namespace ChorePlay.Api.Shared.Configuration;

public class SesSettings
{
    public string Region { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
}
