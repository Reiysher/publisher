namespace Publisher.Config;

internal class ConfigurationHelper
{
    public static PublisherOptions GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config/config.json", optional: false);

        return builder.Build().GetSection(nameof(PublisherOptions)).Get<PublisherOptions>();
    }
        
}
