using Branchecker.Models;
using Branchecker.Shells;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Branchecker {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private readonly IHost _host;
        public IHost ApplcationHost { get { return _host; } }
        public App() {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
        }
        private void ConfigureServices(HostBuilderContext context, IServiceCollection services) {
            services.Configure<GeminiConfig>(context.Configuration.GetSection("Gemini"));

            services.AddHttpClient<GeminiApiClient>((sp, client) =>
            {
                var config = sp.GetRequiredService<IOptions<GeminiConfig>>().Value;
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

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
