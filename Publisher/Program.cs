using System.Reflection;
using System.Text.Json;

namespace Publisher;

internal sealed class Program
{
    private static PublisherOptions _options;

    public static async Task Main(string[] args)
    {
        var configFile = args[0];
        _options = ConfigurationHelper.GetConfiguration(configFile);

        if (!PublishLocally())
            return;

        var pathOnServer = _options.PathOnServer;
        var localPath = _options.LocalPath;

        if (!pathOnServer.EndsWith("/"))
            pathOnServer += "/";

        localPath = Path.GetFullPath(localPath) + Path.DirectorySeparatorChar;

        var localFiles = await GetLocalFiles(localPath);

        Console.WriteLine();
        Console.WriteLine($"Uploading {localFiles.Count} files to {_options.User}@{_options.User}:{_options.Port}{_options.PathOnServer}");

        try
        {
            var uploader = new Uploader(_options);

            uploader.UploadFiles(pathOnServer, _options.ServiceName, localFiles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading files to server: {ex.Message}");
        }
        Directory.Delete(localPath, true);
    }

    private static bool PublishLocally()
    {
        Console.WriteLine($"Starting `dotnet publish`");

        var info = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "publish " + _options.Project + " --output " + _options.LocalPath + " --runtime linux-x64 --self-contained"
        };

        var process = Process.Start(info);
        process.WaitForExit();
        var exitCode = process.ExitCode;

        Console.WriteLine($"dotnet publish exited with code {exitCode}");

        return exitCode == 0;
    }

    private static async Task<List<LocalFile>> GetLocalFiles(string localPath)
    {
        Dictionary<string, string> oldCache = new();
        // чтение данных
        var cacheDirectory = _options.Cache;
        if (!Directory.Exists(cacheDirectory))
            Directory.CreateDirectory(cacheDirectory);

        var cacheFile = $"{cacheDirectory}/{_options.ServiceName}.json";
        if (File.Exists(cacheFile))
        {
            using FileStream fs = new(cacheFile, FileMode.OpenOrCreate);
            oldCache = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);
        }

        var localFiles = Directory
            .EnumerateFiles(localPath, "*.*", SearchOption.AllDirectories)
            .Where(f => _options.LoadConfigFiles || !f.EndsWith(".json"))
            .Select(f => new LocalFile(localPath, f))
            .ToList();

        Dictionary<string, string> newCache = new();
        localFiles.ForEach(file =>
        {
            newCache.Add(file.FileName, CalculateHash(file.FileName));
        });

        var result = newCache
            .Where(kvp => !oldCache.ContainsKey(kvp.Key) || kvp.Value != oldCache[kvp.Key])
            .Select(kvp => new LocalFile(localPath, kvp.Key))
            .ToList();

        // сохранение данных
        using (FileStream fs = new FileStream(cacheFile, FileMode.OpenOrCreate))
        {
            await JsonSerializer.SerializeAsync(fs, newCache);
        }

        return result;
    }

    private static string CalculateHash(string filename)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filename);

        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}