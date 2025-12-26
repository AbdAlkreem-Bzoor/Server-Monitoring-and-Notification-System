namespace MessageBroker.RabbitMQ.Options;

public sealed class ConnectionOptions
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public int Port { get; set; } = 5672;
    public string ClientProvidedName { get; set; } = "MyApp";
    public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);
    public bool AutomaticRecoveryEnabled { get; set; } = true;
}
