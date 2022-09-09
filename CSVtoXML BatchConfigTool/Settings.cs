namespace CSVtoXML_BatchConfigTool
{
    public static class Settings
    {
        public static string SourceXmlFileName = "PIEFGen8.XML";
        public static string TemplateXmlFileName = "Template_VXXX.XML";
        public static int TemplateNameLineNumber = 6;
        public static int DestinationXmlHeaderLineCount = 8;
        public static int DestinationXmlTailLineStart = 11;
        public static int EFTemplatenameLineNumber = 22;
        public static int MasterXmlHeaderLineCount = 3;
        public static int MasterXmlTailLineCount = 48;
        public static bool AutosaveLog = false;
    }
}
