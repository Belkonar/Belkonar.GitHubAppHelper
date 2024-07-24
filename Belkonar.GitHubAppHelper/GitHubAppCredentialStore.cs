using Microsoft.Extensions.Caching.Memory;
using Octokit;

namespace Belkonar.GitHubAppHelper;

public class GitHubAppCredentialStore(IGitHubAppService gitHubAppService, IMemoryCache cache, GitHubAppConfig config) : ICredentialStore
{
    public async Task<Credentials> GetCredentials()
    {
        var credentials = await cache.GetOrCreateAsync($"github-app-token-{config.AppId}", async entry =>
        {
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