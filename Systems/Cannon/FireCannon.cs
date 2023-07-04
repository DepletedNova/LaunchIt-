using Kitchen;
using KitchenMods;
using LaunchIt.Appliances;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using static LaunchIt.Components.CCannon;

namespace LaunchIt.Systems.Cannon
{
    [UpdateBefore(typeof(FindCannonTarget))]
    public class FireCannon : GameSystemBase, IModSystem
    {
        private EntityQuery CannonQuery;
        private EntityContext ctx;
        protected override void Initialise()
        {
            base.Initialise();
            CannonQuery = GetEntityQuery(new QueryHelper().All(typeof(CCannon), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            if (CannonQuery.IsEmpty)    
                return;

            ctx = new(EntityManager);

            using var cannons = CannonQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in cannons)
            {
                var cCannon = GetComponent<CCannon>(entity);
                var cHolder = GetComponent<CItemHolder>(entity);

                if (cHolder.HeldItem == Entity.Null || // Ignore itemless
                    cCannon.Target == Entity.Null || GetComponent<CItemHolder>(cCannon.Target).HeldItem != Entity.Null || // Ignore invalid target
                    (Has<CPreventItemTransfer>(cCannon.Target) && cCannon.State != CannonState.Firing)) // Ignore busy targets
                    continue;

                if (cCannon.State == CannonState.Turning || cCannon.State == CannonState.Reloading)
                    continue;

                // Fire if idle
                if (cCannon.State == CannonState.Idle)
                {
                    cCannon.FlightDelta = 1f;
                    cCannon.State = CannonState.Firing;
                    Set(entity, new CPreventItemTransfer());
                    Set(cCannon.Target, new CPreventItemTransfer());
                }

                cCannon.FlightDelta -= Time.DeltaTime * cCannon.FireSpeed;

                if (cCannon.FlightDelta <= 0f)
                {
                    EntityManager.RemoveComponent<CPreventItemTransfer>(cCannon.Target);
                    EntityManager.RemoveComponent<CPreventItemTransfer>(entity);

                    ctx.UpdateHolder(cHolder.HeldItem, cCannon.Target);

                    cCannon.FlightDelta = 0f;
                    cCannon.State = CannonState.Reloading;

                    Require<CTakesDuration>(entity, out var cDuration);
                    var cooldown = cCannon.Cooldown;
                    cDuration.Total = cooldown;
                    cDuration.Remaining = cooldown;
                    Set(entity, cDuration);
                }

                Set(entity, cCannon);
            }

            ctx.Dispose();
        }
    }
}
