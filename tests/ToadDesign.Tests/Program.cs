using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ToadDesign.Tests;

/// <summary>
/// Runs every public static void method of every *Checks class, each on a fresh STA thread.
/// By default the run happens on a separate, invisible Windows desktop, so no window, pop-up or shadow flashes on the
/// user's screen. <c>--visible</c> runs on the current desktop and adds the checks that need it (Snap_: Win+Left).
/// Other options: <c>--filter text</c>, <c>--skip text</c>.
/// </summary>
internal static class Program
{
    private const string HiddenArg = "--on-hidden-desktop";
    private const string VisibleOnly = "Snap_";
    private static volatile string _current = "startup";

    private static int Main(string[] args)
    {
        if (!args.Contains("--visible") && !args.Contains(HiddenArg)) return HiddenDesktop.RunSelf(args, HiddenArg);

        LogFile.Attach(args);

        // A crash on a background or finalizer thread kills the process; print it where the log captures it.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Console.WriteLine($"CRASH (unhandled, terminating={e.IsTerminating}) during {_current}: {e.ExceptionObject}");
            Console.Out.Flush();
        };
        var hidden = args.Contains(HiddenArg);
        var filter = Option(args, "--filter");
        var skip = Option(args, "--skip");
        var checks = typeof(Program).Assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Checks", StringComparison.Ordinal) && t.IsClass)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0))
            .Where(m => filter == null || Name(m).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(m => skip == null || Name(m).IndexOf(skip, StringComparison.OrdinalIgnoreCase) < 0)
            .Where(m => !hidden || !m.Name.StartsWith(VisibleOnly, StringComparison.Ordinal))
            .OrderBy(m => m.DeclaringType!.Name).ThenBy(m => m.Name)
            .ToList();

        var failed = 0;
        foreach (var check in checks)
        {
            Exception? error = null;
            _current = check.Name;
            Console.Out.Flush();
            var thread = new Thread(() =>
            {
                try { check.Invoke(null, null); }
                catch (TargetInvocationException e) { error = e.InnerException; }
                catch (Exception e) { error = e; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            if (!thread.Join(TimeSpan.FromSeconds(30)))
                error = new TimeoutException("check did not finish in 30 s");

            if (error == null)
            {
                Console.WriteLine($"PASS {check.Name}");
            }
            else
            {
                failed++;
                Console.WriteLine($"FAIL {check.Name}: {error.GetType().Name}: {error.Message}");
            }
        }

        Console.WriteLine($"{checks.Count - failed}/{checks.Count} passed" + (hidden ? $" (hidden desktop; {VisibleOnly}* need --visible)" : ""));
        Console.Out.Flush();
        return failed;
    }

    private static string Name(MethodInfo m) => m.DeclaringType!.Name + "." + m.Name;

    private static string? Option(string[] args, string name) => args.SkipWhile(a => a != name).Skip(1).FirstOrDefault();
}

/// <summary>Starts this program again on a new, invisible desktop and relays its output and exit code.</summary>
internal static class HiddenDesktop
{
    public static int RunSelf(string[] args, string marker)
    {
        var log = Path.Combine(Path.GetTempPath(), $"toaddesign-checks-{Process.GetCurrentProcess().Id}.log");
        var host = Process.GetCurrentProcess().MainModule!.FileName!;
        var self = typeof(HiddenDesktop).Assembly.Location;
        var prefix = string.Equals(Path.GetFileNameWithoutExtension(host), "dotnet", StringComparison.OrdinalIgnoreCase) ? Quote(self) + " " : "";
        var commandLine = Quote(host) + " " + prefix + string.Join(" ", args.Select(Quote)) + " " + marker + " --log " + Quote(log);

        var desktopName = "ToadDesignChecks" + Process.GetCurrentProcess().Id;
        var desktop = Native.CreateDesktop(desktopName, IntPtr.Zero, IntPtr.Zero, 0, Native.GENERIC_ALL, IntPtr.Zero);
        if (desktop == IntPtr.Zero)
        {
            Console.WriteLine("Could not create a hidden desktop; run with --visible.");
            return 1;
        }

        try
        {
            var startup = new Native.STARTUPINFO { cb = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Native.STARTUPINFO)), lpDesktop = desktopName };
            if (!Native.CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, Environment.CurrentDirectory, ref startup, out var info))
            {
                Console.WriteLine("Could not start the checks on the hidden desktop; run with --visible.");
                return 1;
            }

            Native.WaitForSingleObject(info.hProcess, uint.MaxValue);
            Native.GetExitCodeProcess(info.hProcess, out var exitCode);
            Native.CloseHandle(info.hThread);
            Native.CloseHandle(info.hProcess);
            if (File.Exists(log))
            {
                Console.Write(File.ReadAllText(log));
                File.Delete(log);
            }

            return (int)exitCode;
        }
        finally
        {
            Native.CloseDesktop(desktop);
        }
    }

    private static string Quote(string s) => s.Contains(' ') || s.Contains('"') ? "\"" + s.Replace("\"", "\\\"") + "\"" : s;
}
