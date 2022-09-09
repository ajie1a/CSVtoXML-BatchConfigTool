using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSVtoXML_BatchConfigTool
{
    public class ProcessCsv
    {
        public ProcessCsv()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                MW_VM = ((MainWindow)Application.Current.MainWindow).DataContext as MainWindowViewModel;
            });
            XmlProc = new ProcessXML();
            if (!XmlProc.SourceXMLReaded)
                MW_VM.AddLogItem("Failed to read xml source files");
            else
                MW_VM.AddLogItem("Readed xml source files");
            LogIniteialInformation = MW_VM.GetAllLogText();
        }
        private string LogIniteialInformation = "";
        private MainWindowViewModel MW_VM;
        private ProcessXML XmlProc;
        private List<string> csvFiles = new List<string>();
        private Dictionary<string, string> CsvContent = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string FolderPath;
        public void LoadCsvPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;
            MW_VM.ClearLog();
            MW_VM.AddLogItem(LogIniteialInformation);
            MW_VM.AddLogItem("");
            FolderPath = path;
            FileAttributes attr = File.GetAttributes(path);
            csvFiles.Clear();
            if (attr.HasFlag(FileAttributes.Directory))
            {
                var files = Directory.GetFiles(path, "*.csv", SearchOption.AllDirectories).ToList();
                foreach (var f in files)
                {
                    if (Path.GetFileName(f).ToLower().StartsWith("batchviewdisplay_"))
                    {
                        csvFiles.Add(f);
                    }
                }
                if (csvFiles.Count == 0)
                {
                    MW_VM.AddLogItem("There is no csv file which start with \"batchviewdisplay_\" in folder " + path);
                    return;
                }
                MW_VM.AddLogItem("There are " + csvFiles.Count.ToString() + " matched csv files form all " + files.Count + " .csv files in folder " + path);
            }
            else
            {
                if (Path.GetExtension(path).ToLower() != ".csv" || !Path.GetFileName(path).ToLower().StartsWith("batchviewdisplay_"))
                {
                    MW_VM.AddLogItem("There is no matched file (target file type .csv and start with \"BatchviewDisplay_\"");
                    return;
                }
                csvFiles = new List<string>() { path };
                MW_VM.AddLogItem("One csv file: " + path);
            }
            if (csvFiles.Count > 0)
                LoadCSVFiles();
        }

        private void LoadCSVFiles()
        {
            if (csvFiles.Count == 0)
                return;
            CsvContent = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", MissingFieldFound = null };
            foreach (var f in csvFiles)
            {
                int added = 0, duplicated = 0;
                try
                {
                    using (var reader = new StreamReader(f))
                    using (var csv = new CsvHelper.CsvReader(reader, config))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        var x = csv.HeaderRecord;
                        if (!x.Contains("Batch Unit Path"))
                        {
                            MW_VM.AddLogItem(Path.GetFileName(f) + "\tdoes not contains the column \"Batch Unit Path\"");
                            continue;
                        }
                        if (!x.Contains("Batch Unit Name"))
                        {
                            MW_VM.AddLogItem(Path.GetFileName(f) + "\tdoes not contains the column \"Batch Unit Name\"");
                            continue;
                        }
                        while (csv.Read())
                        {
                            var p = csv.GetField("Batch Unit Path");
                            //var n = csv.GetField("Batch Unit Name");
                            var list = p.Split('\\');
                            if (list.Length < 2)
                                continue;
                            var n = list[list.Length - 2] + "\\" + list.Last();
                            if (p != null && n != null)
                            {
                                if (CsvContent.ContainsKey(p))
                                {
                                    duplicated++;
                                }
                                else
                                {
                                    CsvContent.Add(p, n);
                                    added++;
                                }
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    MessageBox.Show("Datei \r\n\"" + f + "\"\r\n ist in anderem Programm geöffnet, Vorgang wird abgebrochen");
                    MW_VM.AddLogItem("File \"" + f + "\" is open in another program, transformation canceled");
                    return;
                }
                MW_VM.AddLogItem(Path.GetFileName(f) + "\tcontent readed, added " + added.ToString() + " rows, " + duplicated.ToString() + " rows duplicated");
            }
            InspectReaded();
        }

        private void InspectReaded()
        {
            if (CsvContent.Count == 0)
            {
                MW_VM.AddLogItem("There is no valid record in csv files");
                return;
            }
            MW_VM.AddLogItem("There are " + CsvContent.Count + " records in csv files");
            var contentList = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in CsvContent)
            {
                var splited = r.Key.Split(new char[] { '\\' }).ToList();
                splited.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                if (splited.Count < 3)
                    continue;
                var server = splited[0].ToUpper();
                var VNum = splited[splited.Count - 2];
                if (contentList.ContainsKey(server))
                {
                    if (contentList[server].ContainsKey(VNum))
                        contentList[server][VNum].Add(r.Value);
                    else
                        contentList[server].Add(VNum, new List<string>() { r.Value });
                }
                else
                {
                    contentList.Add(server, new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase) { { VNum, new List<string>() { r.Value } } });
                }
            }
            if (contentList.Count < 0)
            {
                MW_VM.AddLogItem("There is no valid record in all csv files");
                return;
            }

            MW_VM.AddEmptyLineToLog();
            XmlProc.LogDuplicateInformation();
            var f = "_CSVtoXMLBatchConfig";
            int idx = 1;
            while (Directory.Exists(Path.Combine(FolderPath, f)))
            {
                f = "_CSVtoXMLBatchConfig (" + idx++ + ")";
                if (idx > 100)
                    f = "_CSVtoXMLBatchConfig " + DateTime.Now.ToString("yyyyMMdd HHmmssfff");
            }
            foreach (var s in contentList)
            {
                var fp = Path.Combine(Path.Combine(FolderPath, f), s.Key);
                if (!Directory.Exists(fp))
                    Directory.CreateDirectory(fp);
                XmlProc.CreateDestionationXmlFiles(s, fp);
            }

            MW_VM.AddLogItem("");
            MW_VM.AddLogItem("Reference elements not found for items:");
            foreach (var n in XmlProc.NameOfNodeWithoutRefElement)
            {
                MW_VM.AddLogItem("\t" + n);
            }

            if (Settings.AutosaveLog)
            {
                var logpath = ModelHelper.GetAvailableFilePath(Path.Combine(FolderPath, f), "report", ".log");
                MW_VM.AddLogItem("Log file be saved at " + logpath);
                File.WriteAllText(logpath, MW_VM.LogText);
            }
        }
    }
}
