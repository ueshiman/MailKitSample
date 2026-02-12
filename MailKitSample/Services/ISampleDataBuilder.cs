namespace MailKitSample.Services;

public interface ISampleDataBuilder
{
    Task BuildSampleData(int start, int count, int all);
}