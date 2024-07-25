using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace Belkonar.GitHubAppHelper;

public class GitHubAppCredentialStore(IServiceProvider provider, IMemoryCache cache, GitHubAppConfig config) : ICredentialStore
{
    public async Task<Credentials> GetCredentials()
    {
        var credentials = await cache.GetOrCreateAsync($"github-app-token-{config.AppId}", async entry =>
        {
            var gitHubAppService = provider.GetRequiredService<IGitHubAppService>();
            
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