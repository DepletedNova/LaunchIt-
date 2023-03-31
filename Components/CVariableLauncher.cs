using KitchenData;
using KitchenMods;

namespace LaunchIt.Components
{
    public struct CVariableLauncher : IApplianceProperty, IModComponent
    {
        public Variable Switch;

        public enum Variable
        {
            FarNear = 0,
            SingleMultiple = 1,
        }
    }
}
