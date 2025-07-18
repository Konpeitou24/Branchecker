using Branchecker.Shells;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Branchecker {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private readonly IHost _host;
        public App() {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
        }
        private void ConfigureServices(HostBuilderContext context, IServiceCollection services) {
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
        }
        protected override void OnStartup(StartupEventArgs e) {
            _host.Start();
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e) {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
