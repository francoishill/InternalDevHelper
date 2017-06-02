using System.Windows;
using InternalDevHelper.ViewModels;
using System.Windows.Input;

namespace InternalDevHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMainViewModel m_MainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            m_MainViewModel = new MainViewModel();
            DataContext = m_MainViewModel;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            m_MainViewModel.HandleKeyPressed(e.Key, Keyboard.Modifiers);
        }
    }
}
