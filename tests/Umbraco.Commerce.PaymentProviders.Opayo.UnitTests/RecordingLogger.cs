using System.Reflection;
using Umbraco.Commerce.Common.Logging;

namespace Umbraco.Commerce.PaymentProviders.Opayo.UnitTests;

/// <summary>
/// Records every call made on the Commerce logger interface.
/// </summary>
internal sealed class RecordingLogger
{
    public RecordingLogger()
    {
        Proxy = DispatchProxy.Create<ILogger<OpayoServerPaymentProvider>, RecordingLoggerProxy>();
        ((RecordingLoggerProxy)(object)Proxy).Owner = this;
    }

    public ILogger<OpayoServerPaymentProvider> Proxy { get; }

    public List<Entry> Entries { get; } = new();

    internal sealed record Entry(string Level, object?[] Args)
    {
        public override string ToString()
            => string.Join(" | ", Args.Select(a => a is object[] arr ? string.Join(" | ", arr) : a?.ToString()));
    }
}

public class RecordingLoggerProxy : DispatchProxy
{
    internal RecordingLogger? Owner { get; set; }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        Owner!.Entries.Add(new RecordingLogger.Entry(targetMethod.Name, args ?? []));
        return targetMethod.ReturnType == typeof(bool) ? true : null;
    }
}
