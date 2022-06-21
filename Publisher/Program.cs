namespace Publisher;

internal sealed class Program
{

    public static void Main(string[] args)
    {
        var options = ConfigurationHelper.GetConfiguration();

        if (!PublishLocally(options))
            return;

        var pathOnServer = options.PathOnServer;
        var localPath = options.LocalPath;

        if (!pathOnServer.EndsWith("/")) 
            pathOnServer += "/";

        localPath = Path.GetFullPath(localPath) + Path.DirectorySeparatorChar;

        var localFiles = GetLocalFiles(localPath, options.LoadConfigFiles);

        Console.WriteLine();
        Console.WriteLine($"Uploading {localFiles.Count} files to {options.User}@{options.User}:{options.Port}{options.PathOnServer}");

        try
        {
            var uploader = new Uploader(options);

            uploader.UploadFiles(pathOnServer, options.ServiceName, localFiles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading files to server: {ex.Message}");
        }
        Directory.Delete(localPath, true);
    }

    private static bool PublishLocally(PublisherOptions options)
    {
        Console.WriteLine($"Starting `dotnet publish`");

        var info = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "publish " + options.Project + " --output " + options.LocalPath + " --runtime linux-x64 --self-contained"
        };

        var process = Process.Start(info);
        process.WaitForExit();
        var exitCode = process.ExitCode;

        Console.WriteLine($"dotnet publish exited with code {exitCode}");

        return exitCode == 0;
    }

    private static List<LocalFile> GetLocalFiles(string localPath, bool loadConfigFiles)
    {
        var localFiles = Directory
            .EnumerateFiles(localPath, "*.*", SearchOption.AllDirectories)
            .Where(f => loadConfigFiles || !f.EndsWith(".json"))
            .Select(f => new LocalFile(localPath, f))
            .ToList();
        return localFiles;
    }
    private static string CalculateMD5(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}