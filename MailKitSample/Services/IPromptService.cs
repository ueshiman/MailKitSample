namespace MailKitSample.Services;

public interface IPromptService
{
    string? GetPrompt(string key);
    string? RetailBusiness { get; }
    string? UserSupport { get; }
    string? EndUser { get; }
    string? ComplianceViolation { get; }
}