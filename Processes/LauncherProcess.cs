﻿using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Appliances;
using System.Collections.Generic;

namespace LaunchIt.Processes
{
    public class LauncherProcess : CustomProcess
    {
        public override string UniqueNameID => "launcher_cooldown_process";
        public override List<(Locale, ProcessInfo)> InfoList => new()
        {
            (Locale.English, CreateProcessInfo("Launcher Cooldown", "<sprite name=\"launcher_0\">"))
        };
        public override bool CanObfuscateProgress => false;
    }
}
