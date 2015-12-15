using System;
using System.Collections.Generic;
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
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace waifu2x_i18n_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dirInfo = new DirectoryInfo(App.directory);
            var langlist = dirInfo.GetFiles("UILang.*.xaml");
            string[] langcodelist = new string[langlist.Length];
            for (int i = 0; i < langlist.Length; i++)
            {
                var fn_parts = langlist[i].ToString().Split('.');
                langcodelist[i] = fn_parts[1];
            }

            foreach (var langcode in langcodelist)
            {
                MenuItem mi = new MenuItem();
                mi.Tag = langcode;
                mi.Header = langcode;
                mi.Click += new RoutedEventHandler(MenuItem_Style_Click);
                menuLang.Items.Add(mi);
            }
            foreach (MenuItem item in menuLang.Items)
            {
                if(item.Tag.ToString().Equals(CultureInfo.CurrentUICulture.Name))
                {
                    item.IsChecked = true;
                }
            }
            btn128.IsChecked = true;
            btnCUDA.IsChecked = true;
            btnNoDenoise.IsChecked = true;
            btnRGB.IsChecked = true;
        }

        public static StringBuilder param_src= new StringBuilder("");
        public static StringBuilder param_dst = new StringBuilder("");
        public static StringBuilder param_informat = new StringBuilder("png:jpg:jpeg:bmp:tif:tiff");
        public static StringBuilder param_outformat = new StringBuilder("png");
        public static StringBuilder param_mag = new StringBuilder("1.0");
        public static StringBuilder param_denoise= new StringBuilder("");
        public static StringBuilder param_color = new StringBuilder(@"--model_dir models/anime_style_art_rgb");
        public static StringBuilder param_device = new StringBuilder("-p gpu");
        public static StringBuilder param_block = new StringBuilder("-c 128");
        public static StringBuilder param_mode = new StringBuilder("-m noise_scale");
        public static Process pHandle = new Process();
        public static ProcessStartInfo psinfo = new ProcessStartInfo();

        public static StringBuilder console_buffer = new StringBuilder();

        public static bool flagAbort = false;

        private void OnMenuHelpClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "This is a multilingual graphical user-interface\n" +
                "for the waifu2x-caffe commandline program.\n" +
                "You need a working copy of waifu2x-caffe first\n" +
                "then copy everything from the GUI archive to\n" +
                "waifu2x-caffe folder.\n" +
                "DO NOT rename any subdirectories inside waifu2x-caffe folder\n" +
                "To make a translation, copy one of the bundled xaml file\n" +
                "then edit the copy with a text editor.\n" +
                "Whenever you see a language code like en-US, change it to\n" +
                "the target language code like zh-TW, ja-JP.\n" +
                "The filename needs to be changed too.";
            MessageBox.Show(msg);
        }

        private void OnMenuVersionClick(object sender, RoutedEventArgs e)
        {
            string msg =
                "Multilingual GUI for waifu2x-caffe\n" +
                "By Maverick Tse (2015)\n" +
                "Version 1.0.3\n" +
                "BuildDate: 15 Dec,2015\n" +
                "License: Do What the Fuck You Want License";
            MessageBox.Show(msg);
        }

        private void OnBtnSrc(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fdlg= new OpenFileDialog();
            if (fdlg.ShowDialog() == true)
            {
                this.txtSrcPath.Text = fdlg.FileName;
            }
        }

        private void OnBtnDst(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            if (fdlg.ShowDialog() == true)
            {
                this.txtDstPath.Text = fdlg.FileName;
            }
        }

        private void OnFormatReset(object sender, RoutedEventArgs e)
        {
            this.txtExt.Text = "png:jpg:jpeg:tif:tiff:bmp:gif";
        }

        private void MenuItem_Style_Click(object sender, RoutedEventArgs e)
        {
            foreach(MenuItem item in menuLang.Items)
            {
                item.IsChecked = false;
            }
            MenuItem mi = (MenuItem)sender;
            mi.IsChecked = true;
            App.Instance.SwitchLanguage(mi.Tag.ToString());
        }

        private void On_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects= DragDropEffects.None;
            }
            
            e.Handled = true;
        }

        private void On_SrcDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[]) e.Data.GetData(DataFormats.FileDrop);
                this.txtSrcPath.Text = fn[0];
            }
        }

        private void On_DstDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.txtDstPath.Text = fn[0];
            }
        }

        private void OnDenoiseChecked(object sender, RoutedEventArgs e)
        {
            param_denoise.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_denoise.Append(optsrc.Tag.ToString());
        }

        private void OnColorChecked(object sender, RoutedEventArgs e)
        {
            param_color.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_color.Append(optsrc.Tag.ToString());
        }

        private void OnDeviceChecked(object sender, RoutedEventArgs e)
        {
            param_device.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_device.Append(optsrc.Tag.ToString());
        }

        private void OnBlockChecked(object sender, RoutedEventArgs e)
        {
            param_block.Clear();
            RadioButton optsrc= sender as RadioButton;
            param_block.Append(optsrc.Tag.ToString());
        }

        private void OnConsoleDataRecv(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                console_buffer.Append(e.Data);
                console_buffer.Append(Environment.NewLine);
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    this.CLIOutput.AppendText(e.Data);
                    this.CLIOutput.AppendText(Environment.NewLine);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
            }
            
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            if (!flagAbort)
            {
                try
                {
                    pHandle.CancelOutputRead();
                }
                catch (Exception)
                {
                    //No need to throw
                    //throw;
                }
                
            }
            
            pHandle.Close();
            Dispatcher.BeginInvoke(new Action(delegate
            {
                this.btnAbort.IsEnabled = false;
                this.btnRun.IsEnabled = true;
                //this.CLIOutput.Text = console_buffer.ToString();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle, null);
            flagAbort = false;
        }

        private void OnAbort(object sender, RoutedEventArgs e)
        {
            if (!pHandle.HasExited)
            {
                try
                {
                    pHandle.CancelOutputRead();
                }
                catch (Exception) { /*Nothing*/ }
                
                pHandle.Kill();
                flagAbort = true;
            }
        }

        private void OnRun(object sender, RoutedEventArgs e)
        {
            // Simple checks before further execution //
            if (!File.Exists("waifu2x-caffe-cui.exe"))
            {
                MessageBox.Show(@"waifu2x-caffe-cui.exe is missing!");
                return;
            }
            if (!Directory.Exists("models"))
            {
                MessageBox.Show("Training model folder is missing!");
                return;
            }
            // Sets Source
            // The source must be a file or folder that exists
            if (File.Exists(this.txtSrcPath.Text) || Directory.Exists(this.txtSrcPath.Text))
            {
                if (this.txtSrcPath.Text.Trim() == "") //When source path is empty, replace with current folder
                {
                    param_src.Clear();
                    param_src.Append("-i ");
                    param_src.Append("\"");
                    param_src.Append(App.directory);
                    param_src.Append("\"");
                }
                else
                {
                    param_src.Clear();
                    param_src.Append("-i ");
                    param_src.Append("\"");
                    param_src.Append(this.txtSrcPath.Text);
                    param_src.Append("\"");
                }
            }
            else
            {
                MessageBox.Show(@"The source folder or file does not exists!");
                return;
            }

            // Set Destination
            if (this.txtDstPath.Text.Trim() == "")
            {
                param_dst.Clear();
            }
            else
            {
                param_dst.Clear();
                param_dst.Append("-o ");
                param_dst.Append("\"");
                param_dst.Append(this.txtDstPath.Text);
                param_dst.Append("\"");
            }

            // Set input format
            param_informat.Clear();
            param_informat.Append("-l ");
            param_informat.Append(this.txtExt.Text);
            param_informat.Append(@":");
            param_informat.Append(this.txtExt.Text.ToUpper());

            // Set output format
            param_outformat.Clear();
            param_outformat.Append("-e ");
            param_outformat.Append(this.txtOExt.Text);

            // Set scale ratio
            if (this.slider_zoom.Value != (double) 1.0)
            {
                param_mag.Clear();
                param_mag.Append("-s ");
                param_mag.Append(this.slider_zoom.Value.ToString());
            }
            else
            {
                param_mag.Clear();
            }

            // Set mode
            bool doDenoise = (param_denoise.ToString().Trim() != "");
            bool doScale = (param_mag.ToString().Trim() != "");
            if (doDenoise && doScale)
            {
                param_mode.Clear();
                param_mode.Append("-m noise_scale");
            }
            else if (doDenoise)
            {
                param_mode.Clear();
                param_mode.Append("-m noise");
            }
            else if (doScale)
            {
                param_mode.Clear();
                param_mode.Append("-m scale");
            }
            else
            {
                MessageBox.Show("Nothing to do");
                return;
            }
            this.btnRun.IsEnabled = false;
            this.btnAbort.IsEnabled = true;
            // Assemble parameters
            string full_param = String.Join(" ", param_src.ToString(),
                param_dst.ToString(),
                param_informat.ToString(),
                param_outformat.ToString(),
                param_mode.ToString(),
                param_mag.ToString(),
                param_denoise.ToString(),
                param_color.ToString(),
                param_block.ToString(),
                param_device.ToString());
            // Setup ProcessStartInfo
            psinfo.FileName = "waifu2x-caffe-cui.exe";
            psinfo.Arguments = full_param;
            psinfo.RedirectStandardError = true;
            psinfo.RedirectStandardOutput = true;
            psinfo.UseShellExecute = false;
            psinfo.WorkingDirectory = App.directory;
            psinfo.CreateNoWindow = true;
            psinfo.WindowStyle= ProcessWindowStyle.Hidden;
            pHandle.StartInfo = psinfo;
            pHandle.EnableRaisingEvents = true;
            pHandle.OutputDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
            //pHandle.ErrorDataReceived += new DataReceivedEventHandler(OnConsoleDataRecv);
            pHandle.Exited += new EventHandler(OnProcessExit);

            // Starts working
            
            
            console_buffer.Clear();
            console_buffer.Append(full_param);
            console_buffer.Append("\n");
            try
            {
                bool pState = pHandle.Start();
                
            }
            catch (Exception)
            {
                pHandle.Kill();
                MessageBox.Show("Some parameters do not mix well and crashed...");
                //throw;
            }
            pHandle.BeginOutputReadLine();
            //pHandle.BeginErrorReadLine();

            //pHandle.WaitForExit();
            /*
            pHandle.CancelOutputRead();
            pHandle.Close();
            this.btnAbort.IsEnabled = false;
            this.btnRun.IsEnabled = true;
            this.CLIOutput.Text = console_buffer.ToString();
            */
        }
    }
}
