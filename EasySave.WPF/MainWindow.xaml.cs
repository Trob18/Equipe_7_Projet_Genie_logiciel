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
            DataContext = new MainViewModel();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            if (JobsDataGrid?.SelectedItems == null) return;

            vm.SelectedJobsList = JobsDataGrid.SelectedItems
                                              .OfType<BackupJob>()
                                              .ToList();
        }
    }
}