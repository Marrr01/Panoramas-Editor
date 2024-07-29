using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Panoramas_Editor
{
    public class App : Application
    {
        public new static App Current => (App)Application.Current;
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
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, uiTarget);
            LogManager.Configuration = config;
            return LogManager.GetCurrentClassLogger();
        }

        private IConfiguration ConfigurePaths()
        {
            var assembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["version"] = @"2024.07.29",
                    //["manual"] = Path.Combine(assembly, "manual.pdf"),
                    ["logs"] = Path.Combine(assembly, "logs"),
                    ["temp"] = Path.Combine(assembly, "temp"),
                    ["github"] = @"https://github.com/Marrr01/Panoramas-Editor",

                    ["min"] = "-1", /* минимальное значение смещения */
                    ["max"] = "1",  /* максимальное значение смещения */
                    ["decimals"] = "3", /* количество чисел после запятой для смещения */
                    ["center"] = "0", /* центр = начальное значение */

                    // Регулярные выражения для валидации смещений в текстбоксах:
                    // Актуально для диапазона [-1.000; 1.000]
                    // ^[+-]? - начало строки может начинаться с + или -
                    // \d - одна цифра
                    // [{_decimalSeparator}] - один из возможных разделителей чисел с плавающей точкой
                    // \d{{0,2}} - от нуля до двух цифр
                    // [1-9]$ - в конце строки одна из цифр, кроме 0
                    ["integerAndFractionalParts"] = @$"^[+-]?\d[{decimalSeparator}]\d{{0,2}}[1-9]$",
                    ["integerPart"] = @"^[+-]?\d$"
                });
            return builder.Build();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<DirDialogService>();
            services.AddTransient<ImageDialogService>();
            services.AddTransient<TableDialogService>();
            services.AddTransient<SaveTableDialogService>();
            services.AddTransient<ISerializer, ExcelHelper>();
            services.AddTransient<IDeserializer, ExcelHelper>();
            services.AddTransient<IImageCompressor, ImageCompressor>();
            services.AddTransient<IImageEditor, ImageEditor>();
            services.AddTransient<IImageReader, ImageReader>();
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
