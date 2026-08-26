# AGENTS.md

This project targets **Windows only**. All commands should be run in **PowerShell**.

## Agent working files

All temporary agent working artifacts must be saved into the `_agents/` folder.

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

**Bcfier** (class library, .NET Framework 4.8):
```
& $MSBuild "Bcfier\Bcfier.csproj" /p:Configuration=Debug /p:Platform=x64
```

**Bcfier.Renga** (Renga plugin, .NET Framework 4.8, depends on Bcfier):
```
& $MSBuild "Bcfier.Renga\Bcfier.Renga.csproj" /p:Configuration=Debug /p:Platform=x64
```

**Bcfier.Win** (WPF standalone app, .NET Framework 4.8, depends on Bcfier):
```
& $MSBuild "Bcfier.Win\Bcfier.Win.csproj" /p:Configuration=Debug /p:Platform=x64
```

**Tests** (NUnit tests, .NET 6.0, depends on Bcfier and Bcfier.Renga):
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

Tests are written using NUnit and target .NET 6.0. Build the solution with MSBuild first, then use `dotnet test` with `--no-build`:

### Run all tests

```
& $MSBuild "BCFier.sln" /p:Configuration=Debug /p:Platform=x64
dotnet test "Tests\Tests.csproj" --no-build --configuration Debug -p:Platform=x64 --verbosity normal
```