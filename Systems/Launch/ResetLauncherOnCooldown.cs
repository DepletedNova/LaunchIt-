using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Systems
{
    [UpdateInGroup(typeof(HighPriorityInteractionGroup))]
    [UpdateAfter(typeof(UpdateTakesDuration))]
    public class ResetLauncherOnCooldown : GameSystemBase, IModSystem
    {
        private EntityQuery Launchers;
        protected override void Initialise()
        {
            base.Initialise();
            Launchers = GetEntityQuery(new QueryHelper()
                .All(typeof(CItemLauncher), typeof(CTakesDuration), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            if (Launchers.IsEmpty)
                return;

            using var entities = Launchers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                Require<CItemLauncher>(entity, out var cLauncher);
                if (cLauncher.State != LauncherState.Reloading)
                    continue;

                Require<CTakesDuration>(entity, out var cDuration);
                if (cDuration.Remaining > 0)
                    continue;

                cDuration.Total = 0;
                Set(entity, cDuration);
                
                cLauncher.State = LauncherState.Idle;
                Set(entity, cLauncher);
            }
        }
    }
}
