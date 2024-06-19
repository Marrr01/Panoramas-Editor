using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Panoramas_Editor
{
    public class App : Application
    {
        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }
        public IConfiguration Configuration { get; set; }
        public App()
        {
            Services = ConfigureServices();
            Configuration = Configure();
            Resources.Source = new Uri("pack://application:,,,/Panoramas Editor;component/Resources/GuiResourceDictionary.xaml");
            new MainWindow().Show();
        }

        private static IConfiguration Configure()
        {
            var assembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["version"] = "2024.06.19",
                    ["manual"] = Path.Combine(assembly, "manual.pdf"),
                    ["logs"] = Path.Combine(assembly, "logs"),
                    ["temp"] = Path.Combine(Path.GetTempPath(), "Panoramas Editor")
                });
            return builder.Build();
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            //services.AddSingleton<App>();
            //services.AddSingleton<MainWindow>();

            services.AddTransient<IDirectorySelectionDialog, DirDialogService>();
            services.AddTransient<IImagesSelectionDialog, ImageDialogService>();
            services.AddTransient<IImageCompressor, ImageHelper>();
            services.AddTransient<IImageEditor, ImageHelper>();
            services.AddTransient<IBitmapConverter, ImageHelper>();
            services.AddTransient<IImageReader, ImageHelper>();
            //services.AddTransient<IContext, WpfDispatcherContext>();
            services.AddTransient<IMathHelper, MathHelper>();

            services.AddSingleton<MainWindowVM>();
            services.AddSingleton<ExecutionSetupVM>();
            services.AddSingleton<ExecutionVM>();

            services.AddTransient<EditorVM>();
            services.AddTransient<ImageCenterSelectorVM>();
            services.AddTransient<PreviewVM>();

            return services.BuildServiceProvider();
        }
    }
}
