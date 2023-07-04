using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using static LaunchIt.Components.CCannon;

namespace LaunchIt.Systems.Cannon
{
    public class ResetCannonAtDay : StartOfDaySystem, IModSystem
    {
        private EntityQuery Cannons;
        protected override void Initialise()
        {
            base.Initialise();
            Cannons = GetEntityQuery(new QueryHelper().All(typeof(CCannon), typeof(CTakesDuration), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            if (Cannons.IsEmpty)
                return;

            using var entities = Cannons.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                Require(entity, out CCannon cCannon);
                Require(entity, out CTakesDuration duration);

                if (Has<CPreventItemTransfer>(entity))
                    EntityManager.RemoveComponent<CPreventItemTransfer>(entity);
                if (cCannon.Target != Entity.Null && Has<CPreventItemTransfer>(cCannon.Target))
                    EntityManager.RemoveComponent<CPreventItemTransfer>(cCannon.Target);

                cCannon.State = CannonState.Idle;
                cCannon.FlightDelta = 0;
                cCannon.RotationDelta = 0;
                cCannon.Target = Entity.Null;
                Set(entity, cCannon);

                duration.Total = 0;
                duration.Remaining = 0;
                Set(entity, duration);
            }
        }
    }
}
