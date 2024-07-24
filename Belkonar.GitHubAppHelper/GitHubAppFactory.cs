using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;

namespace Belkonar.GitHubAppHelper;

public interface IGitHubAppFactory
{
    /// <summary>
    /// The primary method of using the factory, this will create a GitHubClient for the named config and
    /// cache it for 50 minutes.
    /// </summary>
    /// <param name="namedClient">Name of an IOptions instance with the config</param>
    /// <returns>The cached client</returns>
    IGitHubClient CreateGitHubClient(string namedClient);
    
    /// <summary>
    /// An alternative method for simply pulling a token, this will cache the token for 50 minutes.
    /// </summary>
    /// <param name="namedClient">Name of an IOptions instance with the config</param>
    /// <returns>A JWT token</returns>
    Task<string> GetInstallationToken(string namedClient);
}

public class GitHubAppFactory(IGitHubAppService gitHubAppService, IMemoryCache cache, IOptionsSnapshot<GitHubAppConfig> optionsSnapshot, string agent) : IGitHubAppFactory
{
    public IGitHubClient CreateGitHubClient(string namedClient)
    {
        return GetGitHubClient(namedClient);
    }
    
    public async Task<string> GetInstallationToken(string namedClient)
    {
        var token = await cache.GetOrCreateAsync($"github-token-{namedClient}", async entry =>
        {
            entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(50);
            
            var config = optionsSnapshot.Get(namedClient);
            
            return await gitHubAppService.GetInstallationToken(config);
        });
        
        if (token == null)
        {
            throw new Exception("Failed to get token");
        }

        return token;
    }
    
    private IGitHubClient GetGitHubClient(string namedClient)
    {
        var config = optionsSnapshot.Get(namedClient);

        var agentParts = agent.Split('/');
        
        var productHeaderValue = agentParts.Length == 2 ? 
            new ProductHeaderValue(agentParts[0], agentParts[1]) : 
            new ProductHeaderValue(agent);
        
        return new GitHubClient(productHeaderValue, new GitHubAppCredentialStore(gitHubAppService, cache, config));
    }
}