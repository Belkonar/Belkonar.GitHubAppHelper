using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Belkonar.GitHubAppHelper;

public interface IGitHubAppService
{
    Task<string> GetInstallationToken(GitHubAppConfig config, GitHubAppInstallationConfig installation);
}

public class GitHubAppService(IHttpClientFactory httpFactory) : IGitHubAppService
{
    public async Task<string> GetInstallationToken(GitHubAppConfig config, GitHubAppInstallationConfig installationConfig)
    {
        // Normally I'd put this in a constructor, but the thing using this is basically a singleton.
        var client = httpFactory.CreateClient("gha");
        
        if (config.GitHubAppPem == null)
        {
            throw new Exception("GitHubAppPem is required");
        }

        byte[] key;

        try
        {
            key = Convert.FromBase64String(config.GitHubAppPem);
        }
        catch (Exception e) // I realise this pattern gets meme-ed on, but I want to add the extra context.
        {
            throw new Exception("Failed to load key", e);
        }
        
        var jwt = GetJwt(config, key);
        
        var tokenUrl = await GetTokenUrl(config, jwt, client, installationConfig);
        
        return await GetTokenResponse(tokenUrl, jwt, client);
    }
    
    public async Task<string> GetTokenResponse(string tokenUrl, string token, HttpClient client)
    {
        using var request = new HttpRequestMessage();
        
        request.RequestUri = new Uri(tokenUrl);
        request.Method = HttpMethod.Post;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        var tokenResponse = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var realToken = tokenResponse?.RootElement.GetProperty("token").GetString();
        
        if (realToken == null)
        {
            throw new Exception("Failed to get installation token");
        }
        
        return realToken;
    }
    
    public async Task<string> GetTokenUrl(GitHubAppConfig config, string token, HttpClient client, GitHubAppInstallationConfig installationConfig)
    {
        using var request = new HttpRequestMessage();
        request.RequestUri = new Uri($"{config.GitHubUri}/app/installations");
        request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

        using var response = await client.SendAsync(request);

        var installations = await response.Content.ReadFromJsonAsync<JsonDocument>();
        
        string? tokenUrl = null;
        
        foreach (var installation in installations!.RootElement.EnumerateArray())
        {
            var targetType = installation.GetProperty("target_type").GetString();
            
            var account = installation.GetProperty("account");
            
            var login = account.GetProperty("login").GetString();
            
            if (targetType == "Organization" && installationConfig.InstallationType == GitHubAppInstallationType.Organization && login?.ToLower() == installationConfig.Name?.ToLower())
            {
                tokenUrl = installation.GetProperty("access_tokens_url").GetString();
                break;
            }
            
            // ReSharper disable once InvertIf // Consistency over all.
            if (targetType == "Repository" && installationConfig.InstallationType == GitHubAppInstallationType.Repository && login?.ToLower() == installationConfig.Name?.ToLower())
            {
                tokenUrl = installation.GetProperty("access_tokens_url").GetString();
                break;
            }
        }

        if (tokenUrl == null)
        {
            throw new Exception("Failed to find installation token URL");
        }

        return tokenUrl;
    }
    public string GetJwt(GitHubAppConfig config, byte[] key)
    {
        using var rsa = RSA.Create();
        
        rsa.ImportRSAPrivateKey(key, out _);

        var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };
        
        var now = DateTime.Now;
        var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

        var jwt = new JwtSecurityToken(
            issuer: config.AppId,
            claims: new [] {
                new Claim(JwtRegisteredClaimNames.Iat, (unixTimeSeconds - 60).ToString(), ClaimValueTypes.Integer64),
            },
            expires: now.AddMinutes(10),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
