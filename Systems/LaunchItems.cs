using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Systems
{
    [UpdateBefore(typeof(InteractionGroup))]
    public class LaunchItems : GameSystemBase, IModSystem
    {
        private EntityContext ctx;
        private EntityQuery Launchers;
        protected override void Initialise()
        {
            Launchers = GetEntityQuery(new QueryHelper()
                .All(typeof(CItemLauncher), typeof(CTakesDuration), typeof(CItemHolder)));
        }

        protected override void OnUpdate()
        {
            if (Launchers.IsEmpty || HasSingleton<SIsNightTime>())
                return;

            ctx = new EntityContext(EntityManager);

            using var entities = Launchers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                Require<CItemLauncher>(entity, out var cLauncher);
                Require<CItemHolder>(entity, out var cHolder);

                // Update 
                if (cHolder.HeldItem == default(Entity) || cLauncher.CurrentTarget == default(Entity) || cLauncher.State == LauncherState.Reloading)
                    continue;

                // Activate if idle
                if (cLauncher.State == LauncherState.Idle)
                {
                    cLauncher.FlightDelta = cLauncher.LaunchSpeed * cLauncher.LaunchDistance;
                    cLauncher.State = LauncherState.Launching;
                    Set(entity, new CPreventItemTransfer());
                    Set(cLauncher.CurrentTarget, new CPreventItemTransfer());
                }

                // Tick flight
                cLauncher.FlightDelta -= Time.DeltaTime;
                
                // Upon flight ending
                if (cLauncher.FlightDelta <= 0)
                {
                    // Begin cooldown
                    Require<CTakesDuration>(entity, out var cDuration);
                    var cooldown = cLauncher.Cooldown * 3f;
                    cDuration.Total = cooldown;
                    cDuration.Remaining = cooldown;
                    Set(entity, cDuration);

                    // Set reloading state
                    cLauncher.State = LauncherState.Reloading;
                    cLauncher.FlightDelta = 0;
                    EntityManager.RemoveComponent<CPreventItemTransfer>(entity);
                    EntityManager.RemoveComponent<CPreventItemTransfer>(cLauncher.CurrentTarget);

                    // Transport item between locations
                    ctx.UpdateHolder(cHolder.HeldItem, cLauncher.CurrentTarget);
                }

                Set(entity, cLauncher);
            }
        }
    }
}
