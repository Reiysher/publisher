namespace Publisher.Config;

internal class ConfigurationHelper
{
    private static IConfiguration _configuration;

    public static PublisherOptions GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false);

        return builder.Build().GetSection(nameof(PublisherOptions)).Get<PublisherOptions>();
    }
        
}
