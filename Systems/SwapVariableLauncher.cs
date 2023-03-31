using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using static LaunchIt.Components.CVariableLauncher;

namespace LaunchIt.Systems
{
    public class SwapVariableLauncher : ItemInteractionSystem, IModSystem
    {
        private CItemLauncher cLauncher;
        private CVariableLauncher cSwapper;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require(data.Target, out cLauncher) || !Require(data.Target, out cSwapper))
                return false;
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            switch (cSwapper.Switch)
            {
                case Variable.SingleMultiple:
                    cLauncher.SingleTarget = !cLauncher.SingleTarget; break;
                case Variable.FarNear:
                    cLauncher.TargetFar = !cLauncher.TargetFar; break;
            }
            Set(data.Target, cLauncher);
        }
    }
}
