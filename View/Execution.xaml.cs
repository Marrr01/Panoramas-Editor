using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    /// <summary>
    /// Логика взаимодействия для Execution.xaml
    /// </summary>
    public partial class Execution : UserControl
    {
        public Execution()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<ExecutionVM>();
        }
    }
}
