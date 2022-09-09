using System;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;

namespace CSVtoXML_BatchConfigTool
{
    [Serializable]
	public class SerializableSettings
	{
		private string FileName => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Assembly.GetExecutingAssembly().GetName().Name + ".Settings.xml");
		public SerializableSettings()
		{
			var x = Assembly.GetCallingAssembly().GetName().Name;
			if (!File.Exists(FileName))
			{
				SerializeInfos();
				return;
			}
			DeserializeInfos();
		}
		public void SerializeInfos()
		{
			FileStream fs = null;
			try
			{
				XmlSerializer ser = new XmlSerializer(typeof(SerializableSettings));
				fs = File.Create(FileName);
				ser.Serialize(fs, this);
			}
			catch { }
			finally
			{
				if (fs != null)
					fs.Close();
			}
		}
		private void DeserializeInfos()
		{
			FileStream fs = null;
			try
			{
				XmlSerializer ser = new XmlSerializer(typeof(SerializableSettings));
				fs = new FileStream(FileName, FileMode.Open);
				XmlReader reader = XmlReader.Create(fs);
				if (File.Exists(FileName))
				{
					_ = (SerializableSettings)ser.Deserialize(reader);
				}
			}
			catch{ }
			finally
			{
				fs?.Close();
			}
		}



		[XmlElement]
		public string SourceXmlFileName
		{
			get => Settings.SourceXmlFileName;
			set => Settings.SourceXmlFileName = value;
		}
		[XmlElement]
		public string TemplateXmlFileName
		{
			get => Settings.TemplateXmlFileName;
			set => Settings.TemplateXmlFileName = value;
		}
		[XmlElement]
		public int TemplateNameLineNumber
		{
			get => Settings.TemplateNameLineNumber;
			set => Settings.TemplateNameLineNumber = value;
		}
		[XmlElement]
		public int DestinationXmlHeaderLineCount
		{
			get => Settings.DestinationXmlHeaderLineCount;
			set => Settings.DestinationXmlHeaderLineCount = value;
		}
		[XmlElement]
		public int DestinationXmlTailLineStart
		{
			get => Settings.DestinationXmlTailLineStart;
			set => Settings.DestinationXmlTailLineStart = value;
		}
		[XmlElement]
		public int EFTemplatenameLineNumber
		{
			get => Settings.EFTemplatenameLineNumber;
			set => Settings.EFTemplatenameLineNumber = value;
		}
		[XmlElement]
		public int MasterXmlHeaderLineCount
		{
			get => Settings.MasterXmlHeaderLineCount;
			set => Settings.MasterXmlHeaderLineCount = value;
		}
		[XmlElement]
		public int MasterXmlTailLineCount
		{
			get => Settings.MasterXmlTailLineCount;
			set => Settings.MasterXmlTailLineCount = value;
		}
		[XmlElement]
		public bool AutosaveLog
		{
			get => Settings.AutosaveLog;
			set => Settings.AutosaveLog = value;
		}
	}
}
