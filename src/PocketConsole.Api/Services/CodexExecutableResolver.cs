using Microsoft.Extensions.Options;
using PocketConsole.Api.Options;

namespace PocketConsole.Api.Services;

public sealed class CodexExecutableResolver(IOptions<CodexOptions> options)
{
    public string Resolve()
    {
        if (!string.IsNullOrWhiteSpace(options.Value.ExecutablePath))
        {
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(options.Value.ExecutablePath));
        }

        var pathExecutable = ResolveFromPath();
        if (!string.IsNullOrWhiteSpace(pathExecutable)) return pathExecutable;

        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin");
        return Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "codex.exe", SearchOption.AllDirectories).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault() ?? "codex"
            : "codex";
    }

    private static string? ResolveFromPath()
    {
        var path = Environment.GetEnvironmentVariable("Path");
        if (string.IsNullOrWhiteSpace(path)) return null;

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var executable = Path.Combine(directory, "codex.exe");
                if (File.Exists(executable)) return executable;
            }
            catch (ArgumentException) { }
        }

        return null;
    }
}



