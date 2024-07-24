using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;

namespace Belkonar.GitHubAppHelper;

public interface IGitHubAppFactory
{
    Task<IGitHubClient> CreateGitHubClient(string namedClient);
}

public class GitHubAppFactory(IGitHubAppService gitHubAppService, IOptionsSnapshot<GitHubAppConfig> optionsSnapshot, string agent) : IGitHubAppFactory
{
    // Take a look at this later
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    
    public async Task<IGitHubClient> CreateGitHubClient(string namedClient)
    {
        var client = await _cache.GetOrCreateAsync($"github-client-{namedClient}", async entry =>
        {
            entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(50);
            
            return await GetGitHubClient(namedClient);
        });
        
        if (client == null)
        {
            throw new Exception("Failed to get client");
        }
        
        return client;
    }
    
    private async Task<IGitHubClient> GetGitHubClient(string namedClient)
    {
        var config = optionsSnapshot.Get(namedClient);
        var token = await gitHubAppService.GetInstallationToken(config);
        var credentials = new Credentials(token, AuthenticationType.Bearer);

        ProductHeaderValue productHeaderValue;
        var agentParts = agent.Split('/');
        
        if (agentParts.Length == 2)
        {
            productHeaderValue = new ProductHeaderValue(agentParts[0], agentParts[1]);
        }
        else
        {
            productHeaderValue = new ProductHeaderValue(agent);
        }
        
        var client = new GitHubClient(productHeaderValue, new InMemoryCredentialStore(credentials));
        return client;
    }
}