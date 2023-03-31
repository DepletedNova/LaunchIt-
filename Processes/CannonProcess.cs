using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Appliances;
using System.Collections.Generic;

namespace LaunchIt.Processes
{
    public class CannonProcess : CustomProcess
    {
        public override string UniqueNameID => "cannon_cooldown_process";
        public override List<(Locale, ProcessInfo)> InfoList => new()
        {
            (Locale.English, CreateProcessInfo("Cannon Cooldown", "<sprite name=\"cannon_0\">"))
        };
        public override bool CanObfuscateProgress => false;
    }
}
