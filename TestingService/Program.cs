using Belkonar.GitHubAppHelper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var gitHubSection = builder.Configuration.GetSection("GitHub");

builder.Services.AddOptions<GitHubAppConfig>("pub")
    .BindConfiguration("GitHub");

builder.Services.SetupGitHubApp("Deployer/1.0");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/weatherforecast", async (IGitHubAppFactory factory) =>
    {
        var client = factory.CreateGitHubClient("pub", new GitHubAppInstallationConfig()
        {
            InstallationType = GitHubAppInstallationType.Organization,
            Name = "belkonar"
        });
        
        return await client.Repository.GetAllForOrg("belkonar");
    })
.WithName("GetWeatherForecast")
.WithOpenApi();

var githubTestConfig = new GitHubAppConfig()
{

};
gitHubSection.Bind(githubTestConfig);

app.Run();
