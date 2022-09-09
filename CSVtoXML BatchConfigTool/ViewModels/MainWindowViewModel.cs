using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CSVtoXML_BatchConfigTool
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
        #endregion INotifyPropertyChanged
        public bool allwaysCan(object param) { return true; }
        public SerializableSettings serializableSettings = new SerializableSettings();

        public MainWindowViewModel()
        {
            LoadBtnClickedCommand = new DelegateCommand(MLoadBtnClicked, allwaysCan);
            LogTbxSaveToCommand = new DelegateCommand(LogTbxSaveTo, allwaysCan);
        }

        public void Init()
        {
            //new SerializableSettings();
            CsvProc = new ProcessCsv();
        }
        public void Window_Closing(object sender, CancelEventArgs e)
        {
            serializableSettings.SerializeInfos();
        }

        private ProcessCsv CsvProc;
        //private string FolderPath = "";

        public bool AutosaveLog
        {
            get { return Settings.AutosaveLog; }
            set
            {
                Settings.AutosaveLog = value;
                OnPropertyChanged("AutosaveLog");
            }
        }

        private string _PathText = "";
        public string PathText
        {
            get { return _PathText; }
            set
            {
                _PathText = value;
                OnPropertyChanged("PathText");
            }
        }
        public string _LogText = "";
        public string LogText
        {
            get { return _LogText; }
            set
            {
                _LogText = value;
                OnPropertyChanged("LogText");
            }
        }
        public void AddLogItem(string item)
        {
            if (string.IsNullOrEmpty(LogText))
                LogText = item;
            else
                LogText += "\r\n" + item;
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ((MainWindow)Application.Current.MainWindow).LogTBX.ScrollToEnd();
            });
        }

        public string GetAllLogText()
        {
            return ((MainWindow)Application.Current.MainWindow).LogTBX.Text;
        }

        public void ClearLog()
        {
            LogText = "";
        }

        public void AddEmptyLineToLog()
        {
            if (!string.IsNullOrEmpty(LogText))
                AddLogItem("");
        }

        public ICommand LoadBtnClickedCommand { get; set; }
        public void MLoadBtnClicked(object param)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.Multiselect = false;
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                StartProcessing(dlg.FileName);
            }
        }
        public ICommand LogTbxSaveToCommand { get; set; }
        public void LogTbxSaveTo(object param)
        {
            var dlg = new CommonSaveFileDialog();
            dlg.Filters.Add(new CommonFileDialogFilter("log file", "log,txt"));
            dlg.DefaultFileName = "CSVtoXmlLog." + DateTime.Now.ToString("yyyy.MM.dd.HH.mm") + ".log";
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                System.IO.File.WriteAllText(dlg.FileName, LogText);
            }
        }

        public void StartProcessing(string FolderPath)
        {
            PathText = FolderPath;
            Task.Factory.StartNew(() => CsvProc.LoadCsvPath(FolderPath));
        }
    }
}
