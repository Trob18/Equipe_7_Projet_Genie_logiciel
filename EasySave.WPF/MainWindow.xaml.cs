using EasySave.WPF.Models;
using EasySave.WPF.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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

        private void OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.CommandParameter is string path)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Le répertoire n'existe pas : {path}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ouverture du répertoire : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}