using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Media;

namespace CSVtoXML_BatchConfigTool
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = VM;
            this.Closing += VM.Window_Closing;
        }
        private MainWindowViewModel VM = new MainWindowViewModel();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VM.Init();
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); 
                ((MainWindowViewModel)DataContext).StartProcessing(files[0]);
            }
        }

        private void LogTBX_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void LogTbxCopy_Click(object sender, RoutedEventArgs e)
        {
            LogTBX.Copy();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}
