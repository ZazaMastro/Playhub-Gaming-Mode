namespace GamingModeSetup;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var app = new System.Windows.Application
        {
            ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
        };

        while (true)
        {
            var languageWindow = new LanguageWindow();
            if (languageWindow.ShowDialog() != true)
            {
                app.Shutdown();
                return;
            }

            var noticeWindow = new PluginNoticeWindow();
            if (noticeWindow.ShowDialog() == true)
            {
                break;
            }

            if (!noticeWindow.BackRequested)
            {
                app.Shutdown();
                return;
            }
        }

        var setupWindow = new SetupWindow();
        app.MainWindow = setupWindow;
        app.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
        app.Run(setupWindow);
    }
}
