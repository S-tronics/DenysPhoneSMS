using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Outgoing
{
    public class JsonDenysParam
    {
        public string type { get; set; }
        public string serial { get; set; }
        public string message { get; set; }
        public string from_nbr { get; set; }
        public IList<DenysContact> nbrs { get; set; }        
    }

    public class DenysContact
    {
        public string name { get; set; }
        public string nbr { get; set; }
    }

    public class JsonDenys
    {
        JsonDenysParam param = new JsonDenysParam();
        Logging log = new Logging("Logging.txt");
        string sfile = String.Empty;
        Int16 iserial = 0;
        public Int16 ISerial
        {
            get { return iserial; }
        }
        public JsonDenys()
        {
            if(File.Exists("Denys_Parameters.json"))
            {
                sfile = File.ReadAllText("Denys_Parameters.json");
                param = JsonConvert.DeserializeObject<JsonDenysParam>(sfile);
                iserial = Convert.ToInt16(param.serial);
            }
        }
        public bool IsValidJson(string strInput)
        {
            if (strInput.StartsWith("{") && strInput.EndsWith("}"))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<DenysContact>(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    Console.WriteLine(jex.Message);
                    log.logappend(jex.Message);
                }

            }
            return false;
        }
    }



}
