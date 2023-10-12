using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LattePanda.Firmata;

namespace Outgoing
{
    public class LattePanda
    {
        
        Arduino ar; 

        public LattePanda()
        {
            //ar = new Arduino("COM1");
        }

        public void setIOMode(int pin, byte mode)
        {
            //ar.pinMode(pin, Arduino.INPUT);
        }

        public bool getIOstate(int pin)
        {
            //if (Convert.ToBoolean(ar.digitalRead(pin)))
            //    return true;
            //else
            //    return false;
            //return false;
            return true;
        }
        

    }
}
