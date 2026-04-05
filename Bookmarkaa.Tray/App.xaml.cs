using System.Threading;
using System.Windows;
using Application = System.Windows.Application;

namespace Bookmarkaa.Tray
{
    public partial class App : Application
    {
        private const string MutexName = "Bookmarkaa.Tray.SingleInstance";
        private const string EventName  = "Bookmarkaa.Tray.ShowWindow";

        private Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mutex = new Mutex(true, MutexName, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                // Sygnalizuj pierwszej instancji, żeby pokazała okno, i zakończ
                using var ev = EventWaitHandle.OpenExisting(EventName);
                ev.Set();
                Shutdown();
                return;
            }

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            bool showWindow = e.Args.Any(a =>
                a.Equals("show", StringComparison.OrdinalIgnoreCase) ||
                a.Equals("showwindow", StringComparison.OrdinalIgnoreCase));

            var tray = new TrayIcon();
            tray.Initialize(showWindow);

            // Nasłuchuj sygnału od kolejnych instancji
            var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
            var thread = new Thread(() =>
            {
                while (showEvent.WaitOne())
                    Dispatcher.Invoke(tray.ShowMainWindow);
            })
            {
                IsBackground = true
            };
            thread.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
