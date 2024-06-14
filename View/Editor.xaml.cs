using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    /// <summary>
    /// Логика взаимодействия для Editor.xaml
    /// </summary>
    public partial class Editor : UserControl
    {
        public Editor()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<EditorVM>();
        }
    }
}
