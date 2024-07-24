// See https://aka.ms/new-console-template for more information

using Belkonar.GitHubAppHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

serviceCollection.AddOptions<GitHubAppConfig>("ghe1")
    .Configure(config =>
    {
        config.AppId = "Iv23li8Mao3KnhxD9omf";
        config.GitHubUri = "https://api.github.com";
        config.TokenEnvironmentVariable = "GITHUB_APP_KEY";
        config.Organization = "belkonar";
    });

serviceCollection.SetupGitHubApp("Deployer/1.0");

var serviceProvider = serviceCollection.BuildServiceProvider();

var gitHubAppService = serviceProvider.GetRequiredService<IGitHubAppFactory>();

var client = await gitHubAppService.CreateGitHubClient("ghe1");

var repos = await client.Repository.GetAllForOrg("belkonar");

foreach (var repo in repos)
{
    Console.WriteLine(repo.Name);
}
