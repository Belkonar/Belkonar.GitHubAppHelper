# Belkonar.GitHubAppHelper

A method for simplifying the authentication requirements of GitHub Apps.

## Install

```shell
dotnet add package Belkonar.GitHubAppHelper
```

## Usage

There are two steps to using the helper. The first is to use the extension to set up the DI.

```csharp
serviceCollection.SetupGitHubApp("YourUserAgent/1.0");
```

The second is to bind an IOptions instance of the configuration.

```csharp
// TBD
```

Once you've done that, you can inject the factory to get a client.

```csharp
class MyClass(IGitHubAppFactory gitHubAppFactory) {
    private readonly IGitHubClient = gitHubAppFactory.CreateGitHubClient("myconfig")
}
```

The clients themselves are cached for a max of five days with a two-hour sliding window,
and the tokens are cached for fifty minutes.
