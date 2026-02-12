namespace MailKitSample.Services;

public interface IPromptDispatcher
{
    string GetWeightedRandomPrompt();
    Task<string?> GenerateRandomMessageAsync();
}