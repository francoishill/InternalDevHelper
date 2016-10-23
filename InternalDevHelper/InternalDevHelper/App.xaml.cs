using System.Windows;
using WinForms = System.Windows.Forms;

namespace InternalDevHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            WinForms.Application.EnableVisualStyles();
        }
    }
}
