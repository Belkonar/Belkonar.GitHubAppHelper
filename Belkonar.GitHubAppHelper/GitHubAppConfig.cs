namespace Belkonar.GitHubAppHelper;

// Generally I'd use only init propers but IOptions is dumb.
public class GitHubAppConfig
{
    public required string AppId { get; set; }
    public string GitHubUri { get; set; } = "https://api.github.com";
    
    public string? Organization { get; set; }
    public string? Repository { get; init; }
    public string? TokenEnvironmentVariable { get; set; }
}