using EasySave.WPF.ViewModels;
using System.Windows;

namespace EasySave.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}