using BenchmarkDotNet.Build.Meta;
using BenchmarkDotNet.Build.Options;
using Cake.Common;
using Cake.Frosting;

namespace BenchmarkDotNet.Build;

public static class Program
{
    public static int Main(string[] args)
    {
        var cakeArgs = CommandLineParser.Instance.Parse(args);
        return cakeArgs == null
            ? 0
            : new CakeHost().UseContext<BuildContext>().Run(cakeArgs);
    }
}

[TaskName(Name)]
[TaskDescription("Restore NuGet packages")]
public class RestoreTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "restore";

    public override void Run(BuildContext context) => context.BuildRunner.Restore();

    public HelpInfo GetHelp()
    {
        return new HelpInfo
        {
            Examples =
            [
                new Example(Name)
            ]
        };
    }
}

[TaskName(Name)]
[TaskDescription("Build BenchmarkDotNet.sln solution")]
[IsDependentOn(typeof(RestoreTask))]
public class BuildTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "build";
    public override void Run(BuildContext context) => context.BuildRunner.Build();

    public HelpInfo GetHelp()
    {
        return new HelpInfo
        {
            Examples =
            [
                new Example(Name).WithMsBuildArgument("Configuration", "Debug")
            ]
        };
    }
}

[TaskName(Name)]
[TaskDescription("Install wasm-tools workload")]
public class InstallWasmToolsWorkload : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "install-wasm-tools";

    public override void Run(BuildContext context) => context.BuildRunner.InstallWorkload("wasm-tools");

    public HelpInfo GetHelp()
    {
        return new HelpInfo
        {
            Examples =
            [
                new Example(Name)
            ]
        };
    }
}

[TaskName(Name)]
[TaskDescription("Run unit tests (fast)")]
[IsDependentOn(typeof(BuildTask))]
public class UnitTestsTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "unit-tests";
    public override void Run(BuildContext context) => context.UnitTestRunner.RunUnitTests();

    public HelpInfo GetHelp()
    {
        return new HelpInfo
        {
            Examples =
            [
                new Example(Name)
                    .WithArgument(KnownOptions.Exclusive)
                    .WithArgument(KnownOptions.Verbosity, "Diagnostic")
            ]
        };
    }
}

[TaskName(Name)]
[TaskDescription("Run integration tests using .NET Framework 4.6.2+ (slow)")]
[IsDependentOn(typeof(BuildTask))]
public class InTestsFullTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "in-tests-full";

    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context) => context.UnitTestRunner.RunInTests("net462");

    public HelpInfo GetHelp() => new();
}

[TaskName(Name)]
[TaskDescription("Run integration tests using .NET 8 (slow)")]
[IsDependentOn(typeof(BuildTask))]
public class InTestsCoreTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "in-tests-core";
    public override void Run(BuildContext context) => context.UnitTestRunner.RunInTests("net8.0");
    public HelpInfo GetHelp() => new();
}

[TaskName(Name)]
[TaskDescription("Run all unit and integration tests (slow)")]
[IsDependentOn(typeof(UnitTestsTask))]
[IsDependentOn(typeof(InTestsFullTask))]
[IsDependentOn(typeof(InTestsCoreTask))]
public class AllTestsTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "all-tests";
    public HelpInfo GetHelp() => new();
}

[TaskName(Name)]
[TaskDescription("Pack Nupkg packages")]
[IsDependentOn(typeof(BuildTask))]
public class PackTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "pack";
    public override void Run(BuildContext context) => context.BuildRunner.Pack();

    public HelpInfo GetHelp()
    {
        return new HelpInfo
        {
            Examples =
            [
                new Example(Name)
                    .WithMsBuildArgument("VersionPrefix", "0.1.1729")
                    .WithMsBuildArgument("VersionSuffix", "preview"),
                new Example(Name).WithArgument(KnownOptions.Stable)
            ]
        };
    }
}

[TaskName(Name)]
[TaskDescription("Fetch changelog files")]
public class DocsFetchTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "docs-fetch";
    public override void Run(BuildContext context) => context.DocumentationRunner.Fetch();

    public HelpInfo GetHelp()
    {
        return new HelpInfo
        {
            Description = $"This task updates the following files:\n" +
                          $"* Clones branch 'docs-changelog' to docs/_changelog\n" +
                          $"* Last changelog footer (if {KnownOptions.Stable.CommandLineName} is specified)\n" +
                          $"* All changelog details in docs/_changelog\n" +
                          $"  (This dir is a cloned version of this repo from branch {Repo.ChangelogBranch})",
            Options = [KnownOptions.DocsPreview, KnownOptions.DocsDepth, KnownOptions.ForceClone],
            EnvironmentVariables = [EnvVar.GitHubToken],
            Examples =
            [
                new Example(Name)
                    .WithArgument(KnownOptions.DocsDepth, "3")
                    .WithArgument(KnownOptions.DocsPreview)
                    .WithArgument(KnownOptions.ForceClone)
            ]
        };
    }
}

[TaskName(Name)]
[TaskDescription("Generate auxiliary documentation files")]
public class DocsGenerateTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "docs-generate";
    public override void Run(BuildContext context) => context.DocumentationRunner.Generate();

    public HelpInfo GetHelp()
    {
        return new HelpInfo
        {
            Options = [KnownOptions.DocsPreview],
            Examples =
            [
                new Example(Name).WithArgument(KnownOptions.DocsPreview)
            ]
        };
    }
}

[TaskName(Name)]
[TaskDescription("Build final documentation")]
[IsDependentOn(typeof(DocsGenerateTask))]
public class DocsBuildTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "docs-build";
    public override void Run(BuildContext context) => context.DocumentationRunner.Build();

    public HelpInfo GetHelp() => new()
    {
        Description = "The 'build' task should be run manually to build api docs",
        Options = [KnownOptions.DocsPreview],
        Examples =
        [
            new Example(Name).WithArgument(KnownOptions.DocsPreview)
        ]
    };
}

[TaskName(Name)]
[TaskDescription("Release new version")]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(PackTask))]
[IsDependentOn(typeof(DocsFetchTask))]
[IsDependentOn(typeof(DocsGenerateTask))]
[IsDependentOn(typeof(DocsBuildTask))]
public class ReleaseTask : FrostingTask<BuildContext>, IHelpProvider
{
    private const string Name = "release";
    public override void Run(BuildContext context) => context.ReleaseRunner.Run();

    public HelpInfo GetHelp() => new()
    {
        Options = [KnownOptions.NextVersion, KnownOptions.Push],
        EnvironmentVariables = [EnvVar.GitHubToken, EnvVar.NuGetToken],
        Examples =
        [
            new Example(Name)
                .WithArgument(KnownOptions.Stable)
                .WithArgument(KnownOptions.NextVersion, "0.1.1729")
                .WithArgument(KnownOptions.Push),
            new Example(Name)
                .WithArgument(KnownOptions.Stable)
                .WithArgument(KnownOptions.Push)
        ]
    };
}