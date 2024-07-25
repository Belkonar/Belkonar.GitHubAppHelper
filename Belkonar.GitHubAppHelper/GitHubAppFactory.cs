using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Octokit;

namespace Belkonar.GitHubAppHelper;

public interface IGitHubAppFactory
{
    /// <summary>
    /// The primary method of using the factory, this will create a GitHubClient for the named config and
    /// cache it for 50 minutes.
    /// </summary>
    /// <param name="namedClient">Name of an IOptions instance with the config</param>
    /// <param name="installationConfig"></param>
    /// <returns>The cached client</returns>
    IGitHubClient CreateGitHubClient(string namedClient, GitHubAppInstallationConfig installationConfig);

    /// <summary>
    /// An alternative method for simply pulling a token, this will cache the token for 50 minutes.
    /// </summary>
    /// <param name="namedClient">Name of an IOptions instance with the config</param>
    /// <param name="installationConfig"></param>
    /// <returns>A JWT token</returns>
    Task<string> GetInstallationToken(string namedClient, GitHubAppInstallationConfig installationConfig);
}

public class GitHubAppFactory(IServiceProvider provider, string agent) : IGitHubAppFactory
{
    private ProductHeaderValue? _agentHeader;
    
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    
    // Reusing the clients *should* be safe since the methods inside are treated as transient instances in the DI.
    // The clients are cached with a sliding expiration of 2 hours and an absolute expiration of 5 days.
    // The tokens are cached with an absolute expiration of 50 minutes.
    public IGitHubClient CreateGitHubClient(string namedClient, GitHubAppInstallationConfig installationConfig)
    {
        var client = _cache.GetOrCreate<IGitHubClient>($"github-client-{namedClient}-{installationConfig}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(2);
            entry.AbsoluteExpiration = DateTimeOffset.Now.AddDays(5);
            
            return GetGitHubClient(namedClient, installationConfig);
        });
        
        if (client == null)
        {
            throw new Exception("Failed to get client");
        }
        
        return client;
    }
    
    public async Task<string> GetInstallationToken(string namedClient, GitHubAppInstallationConfig installationConfig)
    {
        var gitHubAppService = provider.GetRequiredService<IGitHubAppService>();
        var optionsSnapshot = provider.GetRequiredService<IOptionsMonitor<GitHubAppConfig>>();
        
        var token = await _cache.GetOrCreateAsync($"github-token-{namedClient}", async entry =>
        {
            entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(50);
            
            var config = optionsSnapshot.Get(namedClient);
            
            return await gitHubAppService.GetInstallationToken(config, installationConfig);
        });
        
        if (token == null)
        {
            throw new Exception("Failed to get token");
        }

        return token;
    }
    
    private GitHubClient GetGitHubClient(string namedClient, GitHubAppInstallationConfig installationConfig)
    {
        // ReSharper disable once InvertIf // This is more readable
        if (_agentHeader == null)
        {
            var agentParts = agent.Split('/');
        
            _agentHeader = agentParts.Length == 2 ? 
                new ProductHeaderValue(agentParts[0], agentParts[1]) : 
                new ProductHeaderValue(agent);
        }

        return new GitHubClient(_agentHeader, new GitHubAppCredentialStore(provider, namedClient, installationConfig));
    }
}