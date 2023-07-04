using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Entities;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Systems
{
    [UpdateBefore(typeof(PickUpAndDropAppliance))]
    public class SwapSmartLauncherTarget : ApplianceInteractionSystem, IModSystem
    {
        private CItemHolder cHolder;
        private CItemLauncher cLauncher;
        private CAppliance cAppliance;
        protected override InteractionType RequiredType => InteractionType.Grab;
        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require(data.Interactor, out cHolder) || !Require(data.Target, out cLauncher) || cHolder.HeldItem == Entity.Null ||!Require(cHolder.HeldItem, out cAppliance) ||
                !cLauncher.TargetSmart || cLauncher.SmartTargetID == cAppliance.ID || cLauncher.State != LauncherState.Idle ||
                Has<CApplianceBlueprint>(cHolder.HeldItem) || Has<CApplyDecor>(cHolder.HeldItem))
                return false;

            if (!(Has<CItemHolder>(cHolder.HeldItem) || Has<CItemProvider>(cHolder.HeldItem) || Has<CApplianceBin>(cHolder.HeldItem)))
                return false;

            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            cLauncher.SmartTargetID = cAppliance.ID;
            Set(data.Target, cLauncher);
        }
    }
}
