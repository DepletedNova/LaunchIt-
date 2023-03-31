using Kitchen;
using KitchenMods;
using LaunchIt.Appliances;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Systems
{
    [UpdateBefore(typeof(FindLauncherTarget))]
    [UpdateBefore(typeof(LaunchItems))]
    public class ResetLauncherAtNight : StartOfNightSystem, IModSystem
    {
        private EntityQuery Launchers;
        protected override void Initialise()
        {
            base.Initialise();
            Launchers = GetEntityQuery(new QueryHelper().All(typeof(CItemLauncher), typeof(CTakesDuration), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            if (Launchers.IsEmpty)
                return;

            using var entities = Launchers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                Require(entity, out CItemLauncher cLauncher);
                Require(entity, out CTakesDuration duration);
                if (cLauncher.State == LauncherState.Idle)
                    continue;

                if (Has<CPreventItemTransfer>(entity))
                    EntityManager.RemoveComponent<CPreventItemTransfer>(entity);
                if (cLauncher.CurrentTarget != Entity.Null && Has<CPreventItemTransfer>(cLauncher.CurrentTarget))
                    EntityManager.RemoveComponent<CPreventItemTransfer>(cLauncher.CurrentTarget);

                cLauncher.State = LauncherState.Idle;
                cLauncher.FlightDelta = 0;
                Set(entity, cLauncher);

                duration.Total = 0;
                duration.Remaining = 0;
                Set(entity, duration);
            }
        }
    }
}
