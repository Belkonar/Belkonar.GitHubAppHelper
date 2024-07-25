using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Belkonar.GitHubAppHelper;

public static class GitHubAppExtensions
{
    public static void SetupGitHubApp(this IServiceCollection service, string userAgent)
    {
        // While I would prefer typed clients, default request headers don't seem to work with them.
        service.AddHttpClient("gha", (client) =>
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");
        });

        service.AddTransient<IGitHubAppService, GitHubAppService>();
        
        service.AddSingleton<IGitHubAppFactory>(provider =>
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            
            return new GitHubAppFactory(provider, userAgent);
        });
    }
}