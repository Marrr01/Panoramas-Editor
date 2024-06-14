using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Panoramas_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainWindowVM>();
        }
    }
}
