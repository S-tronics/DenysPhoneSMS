using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Newtonsoft.Json;

namespace Outgoing
{
    public enum APPFLOW
    {
        CHECK_NEW_SETTINGS,
        DENYS_CALL_CHECK,

    }
    internal class Program
    {
        static Timer timer = null;
        static void Main(string[] args)
        {
            string accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            TwilioClient.Init(accountSid, authToken);
            DenysApp app = new DenysApp();

            Thread smsthread = new Thread(new ThreadStart(app.SMSCheck));
            smsthread.Start(); 
            while(true)
            {
                DenysTimerCallback(app);
                Thread.Sleep(5000);
            }
            //timer = new Timer(e => DenysTimerCallback(app), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            Console.ReadKey();
        }
        private static void DenysTimerCallback(DenysApp app)
        {
            JsonDenysParam param = new JsonDenysParam();
            JsonDenys jdenys = new JsonDenys();
            string readFile = String.Empty;
            Logging log = new Logging("Logging.txt");

            try 
            { 
                readFile = File.ReadAllText("Denys_Parameters.json");
                param = JsonConvert.DeserializeObject<JsonDenysParam>(readFile);
                if (jdenys.ISerial < Convert.ToInt16(param.serial))
                {
                    app.Param = param;
                }
                app.Param = param;
                app.CallCheck();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

    }
}
