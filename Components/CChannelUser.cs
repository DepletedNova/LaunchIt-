using KitchenData;
using KitchenMods;

namespace LaunchIt.Components
{
    public struct CChannelUser : IApplianceProperty, IModComponent
    {
        public int Channel;
        public bool Receiver;
    }
}
