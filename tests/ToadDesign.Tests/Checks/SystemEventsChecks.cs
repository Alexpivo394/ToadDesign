using System.Threading;
using System.Windows.Threading;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class SystemEventsChecks
{
    /// <summary>
    /// SystemEvents delivers to the subscriber's SynchronizationContext. Subscribing from a UI thread would tie every
    /// later notification to that thread (deadlock if it ends without shutdown); the subscription must be context-free.
    /// </summary>
    public static void Subscription_IsMadeWithoutSynchronizationContext()
    {
        SynchronizationContext? seen = new SynchronizationContext();
        var apartment = ApartmentState.STA;
        var ui = StartUiThread(_ => { });
        ui.Invoke(() =>
        {
            True(SynchronizationContext.Current is DispatcherSynchronizationContext, "precondition: caller has a dispatcher context");
            SystemTheme.RunContextFree(() =>
            {
                seen = SynchronizationContext.Current;
                apartment = Thread.CurrentThread.GetApartmentState();
            });
        });
        True(seen == null, "subscription code runs without a synchronization context");
        Equal(ApartmentState.MTA, apartment, "subscription code runs on an MTA thread");
        ui.InvokeShutdown();
    }

    public static void Symbols_MatchIconData()
    {
        foreach (var symbol in new[] { Symbol.Check, Symbol.Settings, Symbol.X, Symbol.Number123, Symbol.AB2 })
            Equal(symbol.ToString(), IconGeometry.MemberNameOf(symbol), $"icon data row of {symbol}");
    }
}
