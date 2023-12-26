using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVR_ExConfig;

internal interface IVRSetting
{
    public string ReadableName { get; }
    public bool Enabled { get; }
    public void SetEnabled( bool enabled );
}
