using System.Windows;
using InternalDevHelper.ViewModels;

namespace InternalDevHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel m_MainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            m_MainViewModel = new MainViewModel();
            DataContext = m_MainViewModel;
        }
    }
}
