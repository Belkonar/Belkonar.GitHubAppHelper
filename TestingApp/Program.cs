// See https://aka.ms/new-console-template for more information

using Belkonar.GitHubAppHelper;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

serviceCollection.AddOptions<GitHubAppConfig>("ghe1")
    .Configure(config =>
    {
        config.AppId = "Iv23li8Mao3KnhxD9omf";
        config.GitHubUri = "https://api.github.com";
        config.GitHubAppPem = Environment.GetEnvironmentVariable("GITHUB_APP_PEM");
    });

serviceCollection.SetupGitHubApp("Deployer/1.0");

var serviceProvider = serviceCollection.BuildServiceProvider();

var gitHubAppFactory = serviceProvider.GetRequiredService<IGitHubAppFactory>();

var installConfig = new GitHubAppInstallationConfig()
    { InstallationType = GitHubAppInstallationType.Organization, Name = "belkonar" };

var client = gitHubAppFactory.CreateGitHubClient("ghe1", installConfig);

var repos = await client.Repository.GetAllForOrg("belkonar") ?? [];

Console.WriteLine(await gitHubAppFactory.GetInstallationToken("ghe1", installConfig));

Console.WriteLine(repos.FirstOrDefault()?.Name ?? "No repos found");