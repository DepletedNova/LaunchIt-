using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using static LaunchIt.Components.CCannon;

namespace LaunchIt.Systems.Cannon
{
    [UpdateAfter(typeof(FindCannonTarget))]
    public class RotateCannon : GameSystemBase, IModSystem
    {
        private EntityQuery CannonQuery;
        protected override void Initialise()
        {
            base.Initialise();

            CannonQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CChannelUser), typeof(CCannon), typeof(CPosition)));
        }

        protected override void OnUpdate()
        {
            if (CannonQuery.IsEmpty)
                return;

            using var cannons = CannonQuery.ToEntityArray(Allocator.Temp);
            foreach (var cEntity in cannons)
            {
                var cCannon = GetComponent<CCannon>(cEntity);

                // Ignore non-rotating cannons
                if (cCannon.State != CannonState.Turning)
                    continue;

                // Tick rotation
                cCannon.RotationDelta -= Time.DeltaTime * 0.5f;

                // Reset to idle if done rotating
                if (cCannon.RotationDelta <= 0)
                {
                    cCannon.RotationDelta = 0;
                    cCannon.State = CannonState.Idle;
                }

                Set(cEntity, cCannon);
            }
        }
    }
}
