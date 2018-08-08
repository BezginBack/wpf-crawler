using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;

namespace Crawler
{
    // <summary>
    // icon from http://www.iconarchive.com/show/simple-cute-icons-by-tanitakawkaw/folder-icon.html
    // some parts from https://github.com/jasperrietrae/File-Searcher/tree/master/File-Searcher
    // </summary>
    public partial class MainWindow : Window
    {
        private Thread st = null;
        private string sSearchFolder = null;
        private string sKeyPhrase = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStartSearchThread_Clicked(object sender, RoutedEventArgs e)
        {
            lwDisplayResults.Items.Clear();
            sSearchFolder = txtFolderLocation.Text;
            sKeyPhrase = txtKeyPhrase.Text;
            st = new Thread(StartSearching);
            st.Start();
        }

        private void StartSearching()
        {           
            if (String.IsNullOrWhiteSpace(sSearchFolder))
            {
                MessageBox.Show("The document folder field was left empty!", "An error has occurred!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            } else
            {
                if (!sSearchFolder.EndsWith("\\"))
                    sSearchFolder += "\\";
            }

            if (!Directory.Exists(sSearchFolder))
            {
                MessageBox.Show("The document folder could not be found!", "An error has occurred!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (System.IO.Path.HasExtension(sSearchFolder))
            {
                MessageBox.Show("The document folder field contains an extension!", "An error has occurred!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            SetEnabledOfControl(btnStartSearchThread, false);
            SetEnabledOfControl(btnStopSearchThread, true);

            SetProgressBarMaxValue(progressBar, 100);
            SetProgressBarValue(progressBar, 0);

            var filesCountTotal = 0;
            //GetDirectoryCount(sSearchFolder, ref directoryCountTotal);
            GetDocumentNumber(sSearchFolder, ref filesCountTotal, "*.doc");
            SetProgressBarMaxValue(progressBar, filesCountTotal);

            //EnumerateFilesRecursive(sSearchFolder, sKeyPhrase, "*.doc");
            EnumerateFiles(sSearchFolder, sKeyPhrase, "*.doc");
            
            SetEnabledOfControl(btnStartSearchThread, true);
            SetEnabledOfControl(btnStopSearchThread, false);

            SetProgressBarMaxValue(progressBar, 100);
            SetProgressBarValue(progressBar, 0);

        }

        private void lwDisplayResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SearchOperator.FileMember item = (SearchOperator.FileMember) lwDisplayResults.SelectedItems[0];
            Process.Start(item.Location);
        }

        private void GetDocumentNumber(string searchDirectory, ref int filesCountTotal, string pattern = "*")
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(searchDirectory);
                filesCountTotal += di.GetFiles(pattern).Count();
            }
            catch (Exception)
            {

            }
        }

        private void EnumerateFiles(string searchFolder, string keyPhrase, string pattern = "*")
        {
            int i = 1;     
            FileInfo[] files = new FileInfo[0];
            try
            {
                DirectoryInfo di = new DirectoryInfo(searchFolder);
                files = di.GetFiles(pattern);
            }
            catch (Exception)
            {

            }
            foreach (FileInfo fileInfo in files)
            {
                SearchOperator searchOperator = new SearchOperator(fileInfo, keyPhrase);
                searchOperator.Operate(lwDisplayResults);
                SetProgressBarValue(progressBar, i++);
            }
        }

        /* recursion seçeneği ile birlikte kullanılır
        private void GetDirectoryCount(string searchDirectory, ref int directoryCountTotal)
        {
            try
            {
                var directories = Directory.GetDirectories(searchDirectory);
                directoryCountTotal += directories.Count();
                foreach (string dir in directories)
                {
                    GetDirectoryCount(dir, ref directoryCountTotal);
                }
            }
            catch (Exception exception)
            {
                //exceptionStringStore.Add(exception.ToString());
            }
        }*/

        /* recursion seçeneği eklenirse..
        private void EnumerateFilesRecursive(string searchFolder, string keyPharase, string pattern = "*")
        {
            int i = 1;
            var queue = new Queue<string>();
            queue.Enqueue(searchFolder);
            while (queue.Count > 0)
            {
                string dir = queue.Dequeue();
                string[] subdirs = new string[0];
                FileInfo[] files = new FileInfo[0];
                try
                {
                    subdirs = Directory.GetDirectories(dir);
                    DirectoryInfo di = new DirectoryInfo(dir);
                    files = di.GetFiles(pattern);
                }
                catch (IOException)
                {

                }
                catch (System.UnauthorizedAccessException)
                {

                }

                foreach (string subdir in subdirs)
                {
                    queue.Enqueue(subdir);
                    SetProgressBarValue(progressBar, i++);
                }

                foreach (FileInfo fileInfo in files)
                {
                    SearchOperator searchOperator = new SearchOperator(fileInfo, keyPharase);
                    searchOperator.Operate(lwDisplayResults);
                    //yield return fileInfo;
                }
            }
        }*/

        // anlaşılmayan bölge
        private delegate void SetEnabledOfControlDelegate(Control control, bool v);
        private void SetEnabledOfControl(Control control, bool v)
        {
            if (control.Dispatcher.Thread == Thread.CurrentThread)
            {
                control.IsEnabled = v;
            } else
            {
                control.Dispatcher.BeginInvoke(new SetEnabledOfControlDelegate(SetEnabledOfControl), new object[] { control, v });
            }
        }

        private delegate void SetProgressBarMaxValueDelegate(ProgressBar progressBar, int i);
        private void SetProgressBarMaxValue(ProgressBar progressBar, int i)
        {
            if (progressBar.Dispatcher.Thread == Thread.CurrentThread)
            {
                progressBar.Maximum = i;
            }
            else
            {
                progressBar.Dispatcher.BeginInvoke(new SetProgressBarMaxValueDelegate(SetProgressBarMaxValue), new object[] { progressBar, i });
            }
        }

        private delegate void SetProgressBarValueDelegate(ProgressBar progressBar, int i);
        private void SetProgressBarValue(ProgressBar progressBar, int i)
        {
            if (progressBar.Dispatcher.Thread == Thread.CurrentThread)
            {
                progressBar.Value = i;
            }
            else
            {
                progressBar.Dispatcher.BeginInvoke(new SetProgressBarValueDelegate(SetProgressBarValue), new object[] { progressBar, i });
            }
        }
        //anlaşılmayan bölge

        private void btnFolderBrowser_Clicked(object sender, RoutedEventArgs e)
        {
            var fbd = new WinForms.FolderBrowserDialog { Description = "Select a folder which contains your documents" };

            if (txtFolderLocation.Text.Length > 0 && Directory.Exists(txtFolderLocation.Text))
            {
                fbd.SelectedPath = txtFolderLocation.Text;
            }

            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                txtFolderLocation.Text = fbd.SelectedPath;
            }
        }

        private void btnStopSearchThread_Clicked(object sender, RoutedEventArgs e)
        {
            if (st != null && st.IsAlive)
            {
                st.Abort();
                st = null;
            }
            SetEnabledOfControl(btnStartSearchThread, true);
            SetEnabledOfControl(btnStopSearchThread, false);
        }
    }
}
