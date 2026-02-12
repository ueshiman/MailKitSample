namespace MailKitSample.Services;

public interface IAiMailBuilder
{
    Task<string?> GenerateMessage(string userPrompt);
}