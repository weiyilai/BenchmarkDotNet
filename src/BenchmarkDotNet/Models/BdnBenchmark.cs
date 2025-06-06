using System.Text;
using BenchmarkDotNet.Extensions;
using Perfolizer.Models;

namespace BenchmarkDotNet.Models;

internal class BdnBenchmark : BenchmarkInfo
{
    public string Namespace { get; set; } = "";
    public string Type { get; set; } = "";
    public string Method { get; set; } = "";
    public string Parameters { get; set; } = "";
    public string? HardwareIntrinsics { get; set; } = "";

    // TODO: Improve
    public override string? GetDisplay()
    {
        if (Display != null) return Display;

        var builder = new StringBuilder();
        builder.Append($"{Namespace}");
        if (Type.IsNotBlank())
        {
            if (builder.Length > 0)
                builder.Append('.');
            builder.Append(Type);
        }
        if (Method.IsNotBlank())
        {
            if (builder.Length > 0)
                builder.Append('.');
            builder.Append(Method);
        }
        return builder.ToString();
    }
}