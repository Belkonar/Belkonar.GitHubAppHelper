namespace Belkonar.GitHubAppHelper;

// Generally I'd use only init props but IOptions is dumb.
public class GitHubAppConfig
{
    public required string AppId { get; set; }
    public string GitHubUri { get; set; } = "https://api.github.com";
    public string? GitHubAppPem { get; set; }
}

public enum GitHubAppInstallationType
{
    Organization,
    Repository
}

public class GitHubAppInstallationConfig
{
    public required GitHubAppInstallationType InstallationType { get; init; }
    public required string Name { get; init; }

    public override string ToString()
    {
        return $"{InstallationType} {Name}";
    }
    
    public static GitHubAppInstallationConfig New(GitHubAppInstallationType type, string name)
    {
        return new GitHubAppInstallationConfig
        {
            InstallationType = type,
            Name = name
        };
    }
}