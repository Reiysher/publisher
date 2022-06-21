namespace Publisher.Config;

internal sealed class PublisherOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = 22;
    public string User { get; set; }
    public string Password { get; set; }
    public string ServiceName { get; set; }
    public string PathOnServer { get; set; }
    public string LocalPath { get; set; }
    public string Solution { get; set; }
}
