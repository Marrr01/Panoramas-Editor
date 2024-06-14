using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    /// <summary>
    /// Логика взаимодействия для ImageCenterSelector.xaml
    /// </summary>
    public partial class ImageCenterSelector : UserControl
    {
        public ImageCenterSelector()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<ImageCenterSelectorVM>();
        }
    }
}
