namespace Publisher.Config;

internal class ConfigurationHelper
{
    public static PublisherOptions GetConfiguration(string args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(args, optional: false);

        return builder.Build().GetSection(nameof(PublisherOptions)).Get<PublisherOptions>();
    }
        
}
