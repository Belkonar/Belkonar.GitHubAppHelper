namespace Belkonar.GitHubAppHelper;

// Generally I'd use only init props but IOptions is dumb.
public class GitHubAppConfig
{
    public required string AppId { get; set; }
    public string GitHubUri { get; set; } = "https://api.github.com";
    public string? GitHubAppPem { get; set; }
    
    public string? Organization { get; set; }
    public string? Repository { get; set; }
}