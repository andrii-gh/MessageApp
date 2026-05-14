using System.Windows;

namespace MessageApp
{
    public partial class App : Application
    {
        private LocalServer server;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            server = new LocalServer();
            server.Start();
            MessageBox.Show("Server started");
        }
    }
}
