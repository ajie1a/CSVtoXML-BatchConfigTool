using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVtoXML_BatchConfigTool
{
    public static class ModelHelper
    {
        public static string GetAvailableFilePath(string path, string name, string extension = ".xml")
        {
            var fn = name + extension;
            int idx = 1;
            while (File.Exists(Path.Combine(path, fn)))
            {
                fn = name + " (" + idx++ + ")" + extension;
                if (idx > 100)
                    fn = name + "." + DateTime.Now.ToString(".yyyy.MM.dd.HH.mm.ss.fff") + extension;
            }
            return Path.Combine(path, fn);
        }
    }
}
