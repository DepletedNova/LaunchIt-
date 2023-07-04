using Kitchen;
using KitchenMods;
using LaunchIt.Appliances;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LaunchIt.Systems
{
    [UpdateAfter(typeof(LaunchItems))]
    public class BounceItems : GameSystemBase, IModSystem
    {
        private EntityContext ctx;
        private EntityQuery Trampolines;
        protected override void Initialise()
        {
            base.Initialise();
            Trampolines = GetEntityQuery(new QueryHelper().All(typeof(CTrampoline), typeof(CItemHolder), typeof(CPosition)));
        }

        protected override void OnUpdate()
        {
            if (Trampolines.IsEmpty)
                return;

            ctx = new EntityContext(EntityManager);

            using var entities = Trampolines.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                Require(entity, out CItemHolder cHolder);
                Require(entity, out CTrampoline cTrampoline);
                if (cHolder.HeldItem == Entity.Null || !cTrampoline.LaunchedTo)
                    continue;

                Require(entity, out CPosition cPos);

                // Check occupant
                var forward = (Vector3)math.mul(cTrampoline.Orientation, new float3(0, 0, -cTrampoline.Distance));
                var occupant = GetOccupant(forward + cPos.Position);
                if (occupant == Entity.Null || !Require(occupant, out CItemHolder cOldHolder) || cOldHolder.HeldItem != Entity.Null)
                {
                    Set(entity, new CTrampoline());
                    continue;
                }

                if (!Has<CPreventItemTransfer>(entity) && Has<CPreventItemTransfer>(occupant))
                {
                    Set(entity, new CTrampoline());
                    continue;
                }

                // Begin flight
                if (!Has<CPreventItemTransfer>(entity))
                {
                    Set(entity, new CPreventItemTransfer());
                    Set(occupant, new CPreventItemTransfer());
                    cTrampoline.FlightDelta = 1f;
                    cTrampoline.Target = occupant;
                }

                // Tick flight
                cTrampoline.FlightDelta -= Time.DeltaTime * 1.15f;

                if (cTrampoline.FlightDelta <= 0f)
                {
                    ctx.Remove<CPreventItemTransfer>(entity);
                    ctx.Remove<CPreventItemTransfer>(occupant);

                    if (Require(occupant, out CTrampoline chainedTrampoline))
                    {
                        chainedTrampoline.LaunchedTo = true;
                        chainedTrampoline.Distance = cTrampoline.Distance;
                        chainedTrampoline.Orientation = cTrampoline.Orientation;
                        Set(occupant, chainedTrampoline);
                    }

                    Set(entity, new CTrampoline());

                    ctx.UpdateHolder(cHolder.HeldItem, occupant);
                    continue;
                }

                Set(entity, cTrampoline);
            }

            ctx.Dispose();
        }
    }
}
