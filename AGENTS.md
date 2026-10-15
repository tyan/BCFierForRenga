# AGENTS.md

This project targets **Windows only**. All commands should be run in **PowerShell**.

## Code style

Code formatting and style rules are defined in the `.editorconfig` file at the repository root.

- Avoid long single-line expressions that wrap during formatting. For complex arguments
  (e.g. LINQ `Where` conditions passed to a constructor), declare an intermediate variable
  with a semantic name and pass it to the constructor/method instead.

## Agent working files

All temporary agent working artifacts must be saved into the `_agents/` folder.

## Requirements

- .NET 8 SDK (target frameworks are `net8.0-windows`)
- Visual Studio 2022 with the Windows Desktop workload (full MSBuild is required to build
  **Bcfier.Renga**, because `dotnet build` does not support `COMFileReference`
  / TLB import — error `MSB4803`)
- Renga SDK installed at `..\..\RengaSDK` (relative to the repo): `Net\Renga.NET8.PluginUtility.dll`
  and `tlb\RengaCOMAPI.tlb`

## Build

MSBuild is used for building. The path to MSBuild depends on your Visual Studio installation (adjust as needed):

```
$MSBuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
```

All commands use `Configuration=Debug` and `Platform=x64` unless otherwise noted.

### Build the entire solution

```
& $MSBuild "BCFier.sln" /p:Configuration=Debug /p:Platform=x64
```

### Build individual projects

**Bcfier** (WPF class library, .NET 8):
```
& $MSBuild "Bcfier\Bcfier.csproj" /p:Configuration=Debug /p:Platform=x64
```

**Bcfier.Renga** (Renga plugin, .NET 8, depends on Bcfier; requires full MSBuild):
```
& $MSBuild "Bcfier.Renga\Bcfier.Renga.csproj" /p:Configuration=Debug /p:Platform=x64
```

**Bcfier.Win** (WPF standalone app, .NET 8, depends on Bcfier):
```
& $MSBuild "Bcfier.Win\Bcfier.Win.csproj" /p:Configuration=Debug /p:Platform=x64
```

**Tests** (NUnit tests, .NET 8, depends on Bcfier and Bcfier.Renga):
```
& $MSBuild "Tests\Tests.csproj" /p:Configuration=Debug /p:Platform=x64
```

### Rebuild (Clean + Build)

To rebuild from scratch, run Clean first, then Build:

**Rebuild the entire solution:**
```
& $MSBuild "BCFier.sln" /t:Clean /p:Configuration=Debug /p:Platform=x64
& $MSBuild "BCFier.sln" /p:Configuration=Debug /p:Platform=x64
```

**Rebuild an individual project** (e.g., Bcfier):
```
& $MSBuild "Bcfier\Bcfier.csproj" /t:Clean /p:Configuration=Debug /p:Platform=x64
& $MSBuild "Bcfier\Bcfier.csproj" /p:Configuration=Debug /p:Platform=x64
```

**Note:** Cleaning any project that depends on Bcfier (Bcfier.Renga, Bcfier.Win, Tests) will also clean Bcfier's output. The subsequent build will automatically rebuild all dependencies.

## Tests

Tests are written using NUnit and target .NET 8. Build the solution with MSBuild first, then use `dotnet test` with `--no-build`. The unit tests are the primary way to verify changes; running Renga with the plugin is a manual smoke test done by the user on a desktop session and is **not** performed by the agent:

### Run all tests

```
& $MSBuild "BCFier.sln" /p:Configuration=Debug /p:Platform=x64
dotnet test "Tests\Tests.csproj" --no-build --configuration Debug -p:Platform=x64 --verbosity normal
```

## Renga plugin deployment

For the plugin to be loaded by Renga as a .NET 8 plugin, the `BCFier.rndesc` manifest must keep
`<PluginType>Net8</PluginType>`. Copy the contents of `Bcfier.Renga\bin\<Configuration>\`
into Renga's plugin folder.