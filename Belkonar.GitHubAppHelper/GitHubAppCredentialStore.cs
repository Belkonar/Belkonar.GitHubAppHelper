using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Octokit;

namespace Belkonar.GitHubAppHelper;

public class GitHubAppCredentialStore(IServiceProvider provider, string namedClient) : ICredentialStore
{
    // This class should only be instantiated once per client, so we can use a single cache.
    // Each client will have its own cache, but that's fine.
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    
    public async Task<Credentials> GetCredentials()
    {
        var credentials = await _cache.GetOrCreateAsync($"github-app-token-{namedClient}", async entry =>
        {
            var gitHubAppService = provider.GetRequiredService<IGitHubAppService>();
            var optionsSnapshot = provider.GetRequiredService<IOptionsSnapshot<GitHubAppConfig>>();
            
            var config = optionsSnapshot.Get(namedClient);
            
            entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(50);
            
            var token =  await gitHubAppService.GetInstallationToken(config);
            
            return new Credentials(token, AuthenticationType.Bearer);
        });
        
        if (credentials == null)
        {
            throw new Exception("Failed to get token");
        }

        return credentials;
    }
}