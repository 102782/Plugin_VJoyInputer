using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vJoyInterfaceWrap;

namespace JoyInputer
{
    class VJoyController
    {
        private vJoy joystick;
        private vJoy.JoystickState iReport;
        private uint id = 1;

        public VJoyController(int id = 1)
        {
            this.joystick = new vJoy();
            this.iReport = new vJoy.JoystickState();

            if (id <= 0 || id > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!", id);
            }

            if (!this.joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
            {
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
                    this.joystick.GetvJoyManufacturerString(),
                    this.joystick.GetvJoyProductString(),
                    this.joystick.GetvJoySerialNumberString());
            }
        }
    }
}
