using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Entities;

namespace LaunchIt.Systems
{
    [UpdateBefore(typeof(ShowPingedApplianceInfo))]
    public class PingRange : ApplianceInteractionSystem
    {
        protected override InteractionType RequiredType => InteractionType.Notify;

        protected override bool IsPossible(ref InteractionData data)
        {
            if ((Has<CCannon>(data.Target) || Has<CItemLauncher>(data.Target))
                && Has<CAppliance>(data.Target))
                return true;
            return false;
        }

        protected override void Perform(ref InteractionData data)
        {
            Set(data.Target, new CRangeMarker());
        }
    }
}
