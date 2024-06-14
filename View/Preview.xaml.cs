using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    /// <summary>
    /// Логика взаимодействия для Preview.xaml
    /// </summary>
    public partial class Preview : UserControl
    {
        public Preview()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<PreviewVM>();
        }
    }
}
