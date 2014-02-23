using System.Collections.Generic;
using vJoyInterfaceWrap;

namespace JoyInputer
{
    class VJoyController
    {
        private vJoy joystick;
        public uint id { get; private set; }

        public enum VJDSTATE
        {
            VJD_STAT_OWN,
            VJD_STAT_FREE,
            VJD_STAT_BUSY,
            VJD_STAT_MISS,
            VJD_STAT_UNKOWN
        }

        public VJoyController(uint id)
        {
            this.id = id;
            this.joystick = new vJoy();
        }

        public bool CheckDeviceID()
        {
            return this.id > 0 || this.id <= 16;
        }

        public bool CheckDriverEnabled()
        {
            return this.joystick.vJoyEnabled();
        }

        public string GetDriverAttributes()
        {
            if (joystick.vJoyEnabled())
            {
                return string.Format("Vendor: {0}, Product :{1}, Version Number:{2}", 
                    joystick.GetvJoyManufacturerString(), 
                    joystick.GetvJoyProductString(), 
                    joystick.GetvJoySerialNumberString());
            }
            return "vJoy driver not enabled: Failed Getting vJoy attributes.";
        }

        public VJDSTATE GetDeviceStatus()
        {
            VjdStat status = this.joystick.GetVJDStatus(this.id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN: return VJDSTATE.VJD_STAT_OWN;
                case VjdStat.VJD_STAT_FREE: return VJDSTATE.VJD_STAT_FREE;
                case VjdStat.VJD_STAT_BUSY: return VJDSTATE.VJD_STAT_BUSY;
                case VjdStat.VJD_STAT_MISS: return VJDSTATE.VJD_STAT_MISS;
                default: return VJDSTATE.VJD_STAT_UNKOWN;
            };
        }

        public string GetDeviceStatusMessage(VJDSTATE status)
        {
            switch (status)
            {
                case VJDSTATE.VJD_STAT_OWN: return string.Format("vJoy Device {0} is already owned by this", this.id);
                case VJDSTATE.VJD_STAT_FREE: return string.Format("vJoy Device {0} is free", this.id);
                case VJDSTATE.VJD_STAT_BUSY: return string.Format("vJoy Device {0} is already owned by another\nCannot continue", this.id);
                case VJDSTATE.VJD_STAT_MISS: return string.Format("vJoy Device {0} is not installed or disabled\nCannot continue", this.id);
                default: return string.Format("vJoy Device {0} general error\nCannot continue", this.id);
            };
        }

        public string GetDeviceStatusMessage()
        {
            return GetDeviceStatusMessage(GetDeviceStatus());
        }

        public bool Acquire()
        {
            VjdStat status = this.joystick.GetVJDStatus(this.id);
            return !((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))));
        }

        public void Relinquish()
        {
            this.joystick.RelinquishVJD(this.id);
        }

        public int GetButtonNumber()
        {
            return this.joystick.GetVJDButtonNumber(this.id);
        }

        public int GetContPovNumber()
        {
            return this.joystick.GetVJDContPovNumber(this.id);
        }

        public int GetDiscPovNumber()
        {
            return this.joystick.GetVJDDiscPovNumber(this.id);
        }

        public IEnumerable<bool> GetAxisExist()
        {
            return new bool[] 
            { 
                this.joystick.GetVJDAxisExist(this.id, HID_USAGES.HID_USAGE_X),
                this.joystick.GetVJDAxisExist(this.id, HID_USAGES.HID_USAGE_Y),
                this.joystick.GetVJDAxisExist(this.id, HID_USAGES.HID_USAGE_Z),
                this.joystick.GetVJDAxisExist(this.id, HID_USAGES.HID_USAGE_RZ)
            };
        }

        public void ResetInput()
        {
            this.joystick.ResetVJD(this.id);
        }

        public void InputButton(bool value, uint buttonID)
        {
            this.joystick.SetBtn(value, this.id, buttonID);
        }

        public void InputDiscPov(int value)
        {
            joystick.SetDiscPov(value, this.id, 1);
        }

        public void InputAxis1X(int value)
        {
            this.joystick.SetAxis(value, this.id, HID_USAGES.HID_USAGE_X);
        }

        public void InputAxis1Y(int value)
        {
            this.joystick.SetAxis(value, this.id, HID_USAGES.HID_USAGE_Y);
        }

        public void InputAxis2X(int value)
        {
            this.joystick.SetAxis(value, this.id, HID_USAGES.HID_USAGE_Z);
        }

        public void InputAxis2Y(int value)
        {
            this.joystick.SetAxis(value, this.id, HID_USAGES.HID_USAGE_RZ);
        }

        public void GetAxisMaxValue(ref long max)
        {
            this.joystick.GetVJDAxisMax(this.id, HID_USAGES.HID_USAGE_X, ref max);
        }
    }
}
