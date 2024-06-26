﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
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
        public IConfiguration Configuration { get; private set; }
        public Logger Logger { get; private set; }

        public App()
        {
            Services = ConfigureServices();
            Configuration = ConfigurePaths();
            Logger = ConfigureLogging();
            Resources.Source = new Uri("pack://application:,,,/Panoramas Editor;component/Resources/GuiResourceDictionary.xaml");
            new MainWindow().Show();

            //Logger.Info("test info");
            //Logger.Warn("test warning");
            //Logger.Error("test error");
            //Logger.Fatal("test fatal");
        }

        private Logger ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();
            
            var txtTarget = new NLog.Targets.FileTarget()
            {
                FileName = Path.Combine(Current.Configuration["logs"], $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff")}.txt")
            };
            txtTarget.Layout = "${longdate} | ${level:uppercase=true} | ${message:withexception=true}";

            var uiTarget = new UITarget();
            uiTarget.Layout = "${longdate} | ${level:uppercase=true} | ${message:withexception=true}";

            config.AddRule(LogLevel.Info, LogLevel.Fatal, txtTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, uiTarget);
            LogManager.Configuration = config;
            return LogManager.GetCurrentClassLogger();
        }

        private IConfiguration ConfigurePaths()
        {
            var assembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["version"] = @"2024.06.28",
                    ["manual"] = Path.Combine(assembly, "manual.pdf"),
                    ["logs"] = Path.Combine(assembly, "logs"),
                    ["temp"] = Path.Combine(assembly, "temp"),
                    ["github"] = @"https://github.com/Marrr01/Panoramas-Editor"
                    //["temp"] = Path.Combine(Path.GetTempPath(), "Panoramas Editor")
                });
            return builder.Build();
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<DirDialogService>();
            services.AddTransient<ImageDialogService>();

            services.AddTransient<IImageCompressor, ImageCompressor>();
            //services.AddTransient<IImageCompressor, ImageHelperStub>();

            //services.AddTransient<IImageEditor, ImageEditor>();
            services.AddTransient<IImageEditor, ImageHelperStub>();

            services.AddTransient<IImageReader, ImageReader>();
            //services.AddTransient<IImageReader, ImageHelperStub>();

            services.AddTransient<WpfDispatcherContext>();
            services.AddTransient<MathHelper>();
            services.AddTransient<ProgressBarController>();

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
