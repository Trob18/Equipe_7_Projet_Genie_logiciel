using EasySave.WPF.Models;
using EasySave.WPF.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EasySave.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = (MainViewModel)this.DataContext;

            viewModel.SelectedJobsList = JobsDataGrid.SelectedItems.Cast<BackupJob>().ToList();
        }
    }
}