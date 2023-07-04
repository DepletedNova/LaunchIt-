using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using static LaunchIt.Components.CCannon;

namespace LaunchIt.Systems.Cannon
{
    [UpdateInGroup(typeof(HighPriorityInteractionGroup))]
    [UpdateAfter(typeof(UpdateTakesDuration))]
    public class ResetCannonOnCooldown : GameSystemBase, IModSystem
    {
        private EntityQuery Cannons;
        protected override void Initialise()
        {
            base.Initialise();
            Cannons = GetEntityQuery(new QueryHelper()
                .All(typeof(CCannon), typeof(CTakesDuration), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            if (Cannons.IsEmpty)
                return;

            using var entities = Cannons.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                Require<CCannon>(entity, out var cCannon);
                if (cCannon.State != CannonState.Reloading)
                    continue;

                Require<CTakesDuration>(entity, out var cDuration);
                if (cDuration.Remaining > 0)
                    continue;

                cDuration.Total = 0;
                Set(entity, cDuration);

                cCannon.State = CannonState.Idle;
                cCannon.FlightDelta = 0;
                cCannon.RotationDelta = 0;
                Set(entity, cCannon);
            }
        }
    }
}
