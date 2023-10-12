using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Twilio;
using Twilio.Base;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Rest.Api.V2010.Account;

using LattePanda.Firmata;

namespace Outgoing
{
    public enum CALL_STATE
    {
        IDLE,
        EXECUTED,
        WAITING_FOR_RESPONSE,
        ANSWERED,
        NO_ANSWER,
        WAIT_FOR_RESET
    }
    public class DenysApp
    {
        private JsonDenysParam param;
        public JsonDenysParam Param {
            get
            {
                return param;
            }
            set
            {
                this.param = value;
            }
        }
        JsonDenys jdenys = new JsonDenys();
        LattePanda lp = new LattePanda();
        string readFile = String.Empty;
        CALL_STATE call_state = CALL_STATE.IDLE;
        CallResource call;
        MessageResource tx_message;
        MessageResource rx_message;
        Logging log = new Logging("Logging.txt");
        List<MessageResource> rec_sms = new List<MessageResource>();
        int callerindex = 0;
        int caller_wait_cntr = 0;

        public DenysApp()
        {
            lp.setIOMode(0, Arduino.INPUT);
        }

        public void SMSCheck()
        {
            while (true)
            {
                var messages = MessageResource.Read(limit: 20);
                Console.WriteLine("Check for new SMS");

                foreach (var record in messages)
                {
                    string sms_body = record.Body;
                    if (record.Status == MessageResource.StatusEnum.Received)
                    {
                        if (jdenys.IsValidJson(sms_body))
                        {
                            File.WriteAllText("Denys_Parameters.json", sms_body);
                            readFile = File.ReadAllText("Denys_Parameters.json");
                            param = JsonConvert.DeserializeObject<JsonDenysParam>(readFile);
                            log.logappend("SMS Received with params -> nbr: " + record.From.ToString() + " -> body: " + sms_body);
                        }
                        else
                        {
                            rec_sms.Add(record);
                            Console.WriteLine("SMS Received -> body:" + sms_body + " -> From:" + record.From.ToString());
                            log.logappend("SMS Received -> nbr: " + record.From.ToString() + " -> body: " + sms_body);
                        }
                        sms_body = String.Empty;
                        DeleteMessageOptions options = new DeleteMessageOptions(record.Sid);
                        MessageResource.Delete(options);
                    }
                }
                Thread.Sleep(10000);                //Every
            }
        }

        public void CallCheck()
        {
            switch (call_state)
            {
                case CALL_STATE.IDLE:
                    //if(lp.getIOstate(0))
                    if (true)
                    {
                        //Checkups
                        rec_sms.Clear();
                        call = CallResource.Create(
                            twiml: new Twilio.Types.Twiml("<Response><Say>" + "Denys Message Service :" + this.param.message + "Answer this message with a short sms" + "</Say></Response>"),
                            //twiml: new Twilio.Types.Twiml(File.ReadAllText("TwiML.txt")),
                            to: new Twilio.Types.PhoneNumber(param.nbrs[callerindex].nbr),
                            from: new Twilio.Types.PhoneNumber(this.param.from_nbr)
                            );
                        tx_message = MessageResource.Create(
                            body: "Denys Message Service :" + this.param.message + "Answer this message with a short sms",
                            to: new Twilio.Types.PhoneNumber(param.nbrs[callerindex].nbr),
                            from: new Twilio.Types.PhoneNumber(this.param.from_nbr)
                            );
                        call_state = CALL_STATE.EXECUTED;
                        log.logappend("Denys Message Service Activated -> " + "Denys Message Service :" + this.param.message + "Answer this message with an sms containing reset");
                    }
                    break;
                case CALL_STATE.EXECUTED:
                    if (rec_sms.Count > 0)
                    {
                        foreach(MessageResource message in rec_sms)
                        {
                            if(message.From.ToString() == param.nbrs[callerindex].nbr)
                            {
                                call_state = CALL_STATE.ANSWERED;
                                rx_message = message;
                                break;
                            }
                        }
                    }
                    caller_wait_cntr++;
                    if(caller_wait_cntr == 15)
                    {
                        caller_wait_cntr = 0;
                        call_state = CALL_STATE.NO_ANSWER;
                    }
                    break;
                case CALL_STATE.NO_ANSWER:
                    callerindex++;
                    if (callerindex > param.nbrs.Count)
                    {
                        callerindex = 0;
                        Console.WriteLine("Denys Message Service Activated ->  All numbers were called, restart");
                        log.logappend("Denys Message Service Activated ->  All numbers were called, restart");
                    }
                    call_state = CALL_STATE.IDLE;
                    break;
                case CALL_STATE.ANSWERED:
                    caller_wait_cntr = 0;
                    Console.WriteLine("Denys Message Service Activated -> Answered by: " + rx_message.From.ToString());
                    log.logappend("Denys Message Service Activated -> Answered by: " + rx_message.From.ToString());
                    for (int i = 0; i < param.nbrs.Count; i++)
                    {
                        tx_message = MessageResource.Create(
                            body: "Denys Message Service: Answered by" + rx_message.From.ToString(),
                            to: new Twilio.Types.PhoneNumber(param.nbrs[i].nbr),
                            from: new Twilio.Types.PhoneNumber(this.param.from_nbr)
                            );
                    }
                    call_state = CALL_STATE.WAIT_FOR_RESET;
                    break;
                case CALL_STATE.WAIT_FOR_RESET:
                    Console.WriteLine("Denys Message Service: Wait for Reset");
                    if (rec_sms.Count > 0)
                    {
                        foreach (MessageResource message in rec_sms)
                        {
                            if (message.Body.ToLower().Contains("reset"))
                            {
                                //call_state = CALL_STATE.IDLE;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        


    }
}
