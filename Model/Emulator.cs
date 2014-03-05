using Microsoft.SmartDevice.MultiTargeting.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPScreenShoter.Model
{
    public class Emulator
    {
            public ConnectableDevice Device { get; set; }
            public override string ToString()
            {
                return Device.Name;
            }
    }
}
