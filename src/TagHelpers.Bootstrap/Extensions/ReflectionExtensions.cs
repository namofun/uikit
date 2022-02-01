using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

internal static class ReflectionExtensions
{
    const BindingFlags FindPrivate = BindingFlags.Instance | BindingFlags.NonPublic;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static object? Private(this object obj, string privateField)
    {
        return obj?.GetType().GetField(privateField, FindPrivate)?.GetValue(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static T Private<T>(this object obj, string privateField)
    {
        return (T)obj?.GetType().GetField(privateField, FindPrivate)?.GetValue(obj)!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static Type UnwrapNullableType(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public static bool IsAnonymousType(this Type type)
        => type.FullName!.StartsWith("<>f__AnonymousType");
}