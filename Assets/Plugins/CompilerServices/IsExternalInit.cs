namespace System.Runtime.CompilerServices;

/// <summary>
/// Unity's C# compiler does not support the `init` accessor, except for declaring this type in the same assembly.
/// <see href="https://docs.unity3d.com/6000.0/Documentation/Manual/csharp-compiler.html"/>
/// </summary>
public static class IsExternalInit
{
}