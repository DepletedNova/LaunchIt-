using KitchenData;
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
            (Locale.Default, CreateProcessInfo("Launcher Cooldown", "<sprite name=\"launcher_0\">"))
        };
        public override GameDataObject BasicEnablingAppliance => GetCustomGameDataObject<LaunchPlate>().GameDataObject;
        public override int EnablingApplianceCount => 1;
        public override bool CanObfuscateProgress => false;

        public override void OnRegister(Process gdo)
        {
            gdo.Icon = "<sprite name=\"launcher_0\">";
        }
    }
}
