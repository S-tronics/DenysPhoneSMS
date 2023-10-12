using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outgoing
{
    public class Logging
    {
        StreamReader file;
        private string filepath;
        private string filetext;
        public Logging(string filepath)
        {
            try
            {
                this.filepath = filepath;
                file = File.OpenText(this.filepath);
                file.Close();
            }
            catch(FileNotFoundException fex)
            {
                Console.WriteLine(fex.ToString());
                Console.WriteLine("DenysApp: Logging File Not Found, trying to create it.");
                File.CreateText("Logging.txt");
            }

            filetext = File.ReadAllText(this.filepath);
        }

        public void logappend(string txt_append)
        {
            DateTime today = DateTime.Now;

            filetext += today.ToString();
            filetext += " ";
            filetext += txt_append;
            filetext += "\r\n";
            File.AppendAllText(this.filepath, filetext);
        }

    }
}
