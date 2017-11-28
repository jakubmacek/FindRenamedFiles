using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FindRenamedFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<MatchedFile> _matches { get; set; }
        readonly BackgroundWorker _compareWorker;
        readonly BackgroundWorker _synchronizeWorker;

        public MainWindow()
        {
            _matches = new ObservableCollection<MatchedFile>();

            _compareWorker = new BackgroundWorker();
            _compareWorker.DoWork += _compareWorker_DoWork;

            _synchronizeWorker = new BackgroundWorker();
            _synchronizeWorker.DoWork += _synchronizeWorker_DoWork;

            InitializeComponent();

            matchesDataGrid.ItemsSource = _matches;
        }

        void _compareWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            compareButton.Dispatcher.Invoke(() => { compareButton.IsEnabled = false; });

            matchesDataGrid.Dispatcher.Invoke(() => { _matches.Clear(); });

            var sourceFileIndex = new FileIndex(sourcePathTextBox.Dispatcher.Invoke(() => sourcePathTextBox.Text));
            var targetFileIndex = new FileIndex(targetPathTextBox.Dispatcher.Invoke(() => targetPathTextBox.Text));

            foreach (var match in sourceFileIndex.MatchAgainst(targetFileIndex))
                matchesDataGrid.Dispatcher.Invoke(() => { _matches.Add(match); });

            compareButton.Dispatcher.Invoke(() => { compareButton.IsEnabled = true; });
            synchronizeButton.Dispatcher.Invoke(() => { synchronizeButton.IsEnabled = _matches.Count > 0; });
        }

        void Compare_Click(object sender, RoutedEventArgs e)
        {
            if (_compareWorker.IsBusy)
                return;

            _compareWorker.RunWorkerAsync();
        }

        void _synchronizeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            compareButton.Dispatcher.Invoke(() => { compareButton.IsEnabled = false; });
            synchronizeButton.Dispatcher.Invoke(() => { synchronizeButton.IsEnabled = false; });

            foreach (var match in _matches)
            {
                match.Process();
                //matchesDataGrid.Dispatcher.Invoke(() => { TODO update });
            }

            matchesDataGrid.Dispatcher.Invoke(() => { _matches.Clear(); });
            compareButton.Dispatcher.Invoke(() => { compareButton.IsEnabled = true; });
            synchronizeButton.Dispatcher.Invoke(() => { synchronizeButton.IsEnabled = false; });
        }

        void Synchronize_Click(object sender, RoutedEventArgs e)
        {
            if (_synchronizeWorker.IsBusy)
                return;

            _synchronizeWorker.RunWorkerAsync();
        }
    }
}
