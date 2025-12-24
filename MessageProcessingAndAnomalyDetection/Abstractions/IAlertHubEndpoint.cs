namespace MessageProcessingAndAnomalyDetection.Abstractions;

public interface IAlertHubEndpoint
{
    Task SendAlertAsync(string serverIdentifier, string message);
}

