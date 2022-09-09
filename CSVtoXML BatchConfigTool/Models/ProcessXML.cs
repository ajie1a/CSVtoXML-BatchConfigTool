using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace CSVtoXML_BatchConfigTool
{

    public class ProcessXML
    {
        public ProcessXML()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                MW_VM = ((MainWindow)Application.Current.MainWindow).DataContext as MainWindowViewModel;
            });
            SourceXMLReaded = ReadSourceXml() && ReadDestinationSampleXml();
        }
        private MainWindowViewModel MW_VM;
        // Key: Name, Value.Item1: VNumber, Value.Item2: XML Node
        private Dictionary<string, Tuple<string, string>> srcXmlNodes = new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase);
        private List<string> sourceDuplicateInformation = new List<string>();
        private string DstXmlFront1 = "";
        private string DstXmlFront2 = "";
        private string DstXmlEnd1 = "";
        private string DstXmlEnd2 = "";
        private string MasterFront = "";
        private string MasterEnd = "";
        public bool SourceXMLReaded = false;
        public List<string> NameOfNodeWithoutRefElement = new List<string>();
        private bool ReadSourceXml()
        {
            var path = "XML\\" + Settings.SourceXmlFileName;
            if (!File.Exists(path))
            {
                MW_VM.AddLogItem("Failed to load " + Settings.SourceXmlFileName);
                return false;
            }

            var rdr = XmlReader.Create(path);
            var duplicatedNodes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            NameOfNodeWithoutRefElement = new List<string>();
            while (rdr.Read())
            {
                if (rdr.Name == "ProcedureConfig")
                {
                    XElement ProcedureConfig = (XElement)XElement.ReadFrom(rdr);
                    var VNum = ProcedureConfig.Element("Name").Value;
                    var children = ProcedureConfig.Element("Children").Elements("UnitProcedureConfig").ToList();
                    foreach(var c in children)
                    {
                        //var name = c.Element("Name").Value;
                        var RefElement = c.Element("RefElements").Element("RefElement");
                        if (RefElement == null)
                        {
                            NameOfNodeWithoutRefElement.Add(VNum + " - " + c.Element("Name").Value);
                            continue;
                        }
                        var nameList = RefElement.Value.Split('\\');
                        var name = nameList[nameList.Length - 2] + "\\" + nameList[nameList.Length - 1];
                        
                        if (!srcXmlNodes.ContainsKey(name))
                        {
                            srcXmlNodes.Add(name, new Tuple<string, string>(VNum, c.ToString()));
                        }
                        else
                        {
                            if (duplicatedNodes.ContainsKey(name))
                            {
                                duplicatedNodes[name].Add(VNum);
                            }
                            else
                            {
                                duplicatedNodes.Add(name, new List<string>() { srcXmlNodes[name].Item1, VNum });
                            }
                        }
                    }
                }
            }
            foreach (var dn in duplicatedNodes)
            {
                var vnums = " (";
                foreach (var vn in dn.Value)
                    vnums += " " + vn + " ";
                vnums += ")";
                sourceDuplicateInformation.Add("Unit name duplicated in source file: " + dn.Key + vnums);
            }
            LogDuplicateInformation();
            MW_VM.AddLogItem("Loaded " + Settings.SourceXmlFileName);
            return true;
        }

        public void LogDuplicateInformation()
        {
            foreach (var d in sourceDuplicateInformation)
                MW_VM.AddLogItem(d);
        }

        private bool ReadDestinationSampleXml()
        {
            var path = "XML\\" + Settings.TemplateXmlFileName;
            if (!File.Exists(path))
            {
                MW_VM.AddLogItem("Failed to load " + Settings.TemplateXmlFileName);
                return false;
            }
            var lines = File.ReadAllLines(path);
            var TemplateNameStr = lines.ElementAt(Settings.TemplateNameLineNumber);
            var TNStartP = TemplateNameStr.IndexOf(">");
            var TNEndP = TemplateNameStr.LastIndexOf("<");
            var EFTemplatenameStr = lines.ElementAt(Settings.EFTemplatenameLineNumber);
            var EFTStartP = EFTemplatenameStr.IndexOf(">");
            var EFTEndP = EFTemplatenameStr.LastIndexOf("<");

            for (int i = 0; i < lines.Length; i++)
            {
                if (i < Settings.TemplateNameLineNumber)
                    DstXmlFront1 += lines[i] + "\r\n";
                if (i > Settings.TemplateNameLineNumber && i < Settings.DestinationXmlHeaderLineCount)
                    DstXmlFront2 += lines[i] + "\r\n";
                if (i >= Settings.DestinationXmlTailLineStart && i < Settings.EFTemplatenameLineNumber)
                    DstXmlEnd1 += lines[i] + "\r\n";
                if (i > Settings.EFTemplatenameLineNumber)
                    DstXmlEnd2 += lines[i] + "\r\n";
                if (i <= Settings.MasterXmlHeaderLineCount)
                    MasterFront += lines[i] + "\r\n";
                if (i >= Settings.MasterXmlTailLineCount)
                    MasterEnd += lines[i] + "\r\n";
            }
            DstXmlFront1 += TemplateNameStr.Substring(0, TNStartP + 1);
            DstXmlFront2 = TemplateNameStr.Substring(TNEndP, TemplateNameStr.Length - TNEndP) + DstXmlFront2;
            DstXmlEnd1 += EFTemplatenameStr.Substring(0, EFTStartP + 1);
            DstXmlEnd2 = EFTemplatenameStr.Substring(EFTEndP, EFTemplatenameStr.Length - EFTEndP) + DstXmlEnd2;
            MW_VM.AddLogItem("Loaded " + Settings.TemplateXmlFileName);
            return true;
        }

        public void CreateDestionationXmlFiles(KeyValuePair<string, Dictionary<string, List<string>>> InspectedCsvContent, string DestinationFolder = "")
        {
            //LogDuplicateInformation();
            var savedPathes = new List<Tuple<string, List<string>>>();
            var AllProcedureConfigs = new List<string>();
            var AllMissingContents = new List<Tuple<string, string>>();
            string content = "";
            foreach (var f in InspectedCsvContent.Value)
            {
                content = "";
                string AddContent = "";
                var MissingContents = new List<Tuple<string, string>>();

                foreach (var k in f.Value)
                {
                    if (srcXmlNodes.ContainsKey(k))
                    {
                        AddContent += "\r\n" + srcXmlNodes[k].Item2;
                    }
                    else
                    {
                        MissingContents.Add(new Tuple<string, string>(f.Key, k));
                        AllMissingContents.Add(new Tuple<string, string>(f.Key, k));
                    }
                }
                content = DstXmlFront1 + f.Key + DstXmlFront2 + AddContent + DstXmlEnd1 + f.Key + DstXmlEnd2;
                XDocument doc = XDocument.Parse(content);
                content = doc.ToString();
                AllProcedureConfigs.Add(GetProcedureConfig(content));
                var fp = ModelHelper.GetAvailableFilePath(DestinationFolder, InspectedCsvContent.Key + "_" + f.Key);
                File.WriteAllText(fp, content);
                MW_VM.AddLogItem("File " + fp + " is saved");
                foreach (var mc in MissingContents)
                    MW_VM.AddLogItem("\tKey \"" + mc.Item1 + "\" & \"" + mc.Item2 + "\" is not found in source " + Settings.SourceXmlFileName);
            }
            var masterXmlContent = CreateMasterXmlString(AllProcedureConfigs);
            var MasterSaveName = ModelHelper.GetAvailableFilePath(DestinationFolder, InspectedCsvContent.Key + "_Master");
            File.WriteAllText(MasterSaveName, masterXmlContent);
            MW_VM.AddLogItem("File " + MasterSaveName + " is saved");
            foreach (var mc in AllMissingContents)
                MW_VM.AddLogItem("\tKey \"" + mc.Item1 + "\" & \"" + mc.Item2 + "\" is not found in source " + Settings.SourceXmlFileName);
        }

        private string GetProcedureConfig(string XmlContent)
        {
            XmlReader reader = XmlReader.Create(new StringReader(XmlContent));
            while (reader.Read())
            {
                if (reader.Name == "Children")
                {
                    return reader.ReadInnerXml();
                }
            }
            return "";
        }

        private string CreateMasterXmlString(List<String> ProcedureConfigs)
        {
            var it = "";
            foreach (var pc in ProcedureConfigs)
                it += pc;
            var output = MasterFront + it + MasterEnd; 
            XDocument doc = XDocument.Parse(output);
            return doc.ToString();
        }
    }
}
