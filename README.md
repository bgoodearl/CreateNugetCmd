# Create NuGet Cmd

Console program to create a .CMD file that will register a local NuGet package.

## 2022 Upgrade to use .NET 6
### 2025 Upgrade to use .NET 8

Upgraded project to use appsettings.json instead of App.config.

Optionally, appsettings.localdev.json can be used (and won't be committed to Git)

## Using CreateNugetCmd.exe

Put the files from the bin folder of this project into a folder that in the parent folder
for solutions using it.

e.g. put the binaries and appsettings.config in the folder "createcmd" that is
a sibling of the solution folder that contains the Test.Models project.

Create the local or network share folder for packages and symbols.
e.g.
`C:\Users\_dev\dev_packages`
and
`C:\Users\_dev\dev_packages\_symbols`

Then set up projects that are going to use this as follows:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Product>Local NuGet Demo</Product>
    <AssemblyName>Test.Models</AssemblyName>
	<AssemblyTitle>Test Models</AssemblyTitle>
    <Version>8.0.2501.09170</Version>
    <AssemblyVersion>8.0.2501.09170</AssemblyVersion>
	<PackageId>Test.Models</PackageId>
	<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="$(SolutionDir)..\createcmd\CreateNugetCmd.exe $(TargetDir) $(ProjectPath)" />
  </Target>

</Project>
```
