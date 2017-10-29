using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YoutubeDL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        string selectedDirectory;
        string youtubeDlLocation;
        string youtubeURL;
        Process cmd;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            menuTitleBar.MouseDown += (s, e) => DragMove();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            checkDefaultSettings();
        }

        public void checkDefaultSettings()
        {
            var defaults = Properties.Settings.Default;
            if (defaults.defaultDirectory != null)
            {
                selectedDirectory = defaults.defaultDirectory.ToString();
                directorySelect.Content = defaults.defaultDirectory.ToString();
            }
            if (defaults.youtubedlLocation != null)
            {
                youtubeDlLocation = defaults.youtubedlLocation.ToString();
                youtubeLocation.Content = defaults.youtubedlLocation.ToString();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (cmd != null && cmd.HasExited != true)
            {
                cmd.CancelErrorRead();
                cmd.CancelOutputRead();
                cmd.Close();
                cmd.WaitForExit();
            }
        }

        private void clickSetDownloadDirectory(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedDirectory = dialog.FileName;
                directorySelect.Content = dialog.FileName;
                
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.defaultDirectory = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void clickSetYoutubeDLDirectory(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                youtubeDlLocation = dialog.FileName;
                youtubeLocation.Content = dialog.FileName;
                
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.youtubedlLocation = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void clickGetAudio(object sender, RoutedEventArgs e)
        {
            youtubeURL = txtYoutubeURL.Text;
            getMedia(youtubeURL, selectedDirectory, youtubeDlLocation, "140");
        }

        private void clickGetVideo(object sender, RoutedEventArgs e)
        {
            youtubeURL = txtYoutubeURL.Text;
            getMedia(youtubeURL, selectedDirectory, youtubeDlLocation, "best");
        }

        void cmdOutputRecieved(object sender, DataReceivedEventArgs e)
        {
            updateStatus(e.Data);
        }

        void cmdErrorReceived(object sender, DataReceivedEventArgs e)
        {
            updateStatus(e.Data);
        }

        void cmdExit(object sender, EventArgs e)
        {
            cmd.OutputDataReceived -= new DataReceivedEventHandler(cmdOutputRecieved);
            cmd.Exited -= new EventHandler(cmdExit);
        }

        public void getMedia(string youtubeURL, string targetDirectory, string youtubeDLLocation, string typeWanted)
        {
            statusConsole.Text = "";

            string gotoTargetDirectory = "cd " + targetDirectory;
            string convertCommandAudio = youtubeDLLocation + " -f " + typeWanted + " " + youtubeURL;

            ProcessStartInfo pStart = new ProcessStartInfo();
            pStart.FileName = "cmd";
            pStart.WorkingDirectory = targetDirectory;
            pStart.Arguments = "/K " + convertCommandAudio;
            pStart.RedirectStandardInput = true;
            pStart.RedirectStandardOutput = true;
            pStart.RedirectStandardError = true;
            pStart.UseShellExecute = false;
            pStart.CreateNoWindow = true;
            pStart.WindowStyle = ProcessWindowStyle.Hidden;

            cmd = new Process();
            cmd.StartInfo = pStart;

            if (cmd.Start() == true)
            {
                cmd.OutputDataReceived += new DataReceivedEventHandler(cmdOutputRecieved);
                cmd.ErrorDataReceived += new DataReceivedEventHandler(cmdErrorReceived);
                cmd.Exited += new EventHandler(cmdExit);

                cmd.BeginOutputReadLine();
                cmd.BeginErrorReadLine();
            }
            else
            {
                cmd = null;
            }
        }

        private void updateStatus(string text)
        {
            if (!statusConsole.Dispatcher.CheckAccess())
            {
                statusConsole.Dispatcher.Invoke(new Action(()
                    => { writeToConsole(text); }));
            }
            else
            {
                writeToConsole(text);
            }
        }

        private void writeToConsole(string text)
        {
            if (text != null)
            {
                Span line = new Span();
                foreach (string textLine in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    line.Inlines.Add(new Run(textLine));
                }
                line.Inlines.Add(new LineBreak());
                statusConsole.Inlines.Add(line);
                statusScroll.ScrollToBottom();
            }
        }

        private void btnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void btnMinimizeApp_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
    }
}
