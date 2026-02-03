namespace MailKitSample.Services;

public interface IConfigurationService
{

    string ValidDomain { get; }

    string DeploymentName { get; }

    string MailCreateAIKey { get; }
    string EndPoint { get; }
}