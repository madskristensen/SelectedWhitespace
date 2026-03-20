// Polyfill for C# 'init' keyword support on .NET Framework 4.8.
// This type is provided by the runtime in .NET 5+ but must be
// defined manually when targeting older frameworks.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
