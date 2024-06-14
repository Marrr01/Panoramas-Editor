using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    /// <summary>
    /// Логика взаимодействия для ExecutionSetup.xaml
    /// </summary>
    public partial class ExecutionSetup : UserControl
    {
        public ExecutionSetup()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<ExecutionSetupVM>();
        }
    }
}
