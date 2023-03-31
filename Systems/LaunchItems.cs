using Kitchen;
using KitchenData;
using KitchenMods;
using LaunchIt.Components;
using MessagePack.Formatters;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Systems
{
    [UpdateAfter(typeof(InteractionGroup))]
    public class LaunchItems : GameSystemBase, IModSystem
    {
        private EntityContext ctx;
        private EntityQuery Launchers;
        protected override void Initialise()
        {
            base.Initialise();
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
                if (cLauncher.CurrentTarget == Entity.Null || cLauncher.State == LauncherState.Reloading || cHolder.HeldItem == Entity.Null)
                    continue;

                if (cLauncher.State != LauncherState.Launching && Has<CPreventItemTransfer>(cLauncher.CurrentTarget))
                    continue;

                bool hasBin = Require<CApplianceBin>(cLauncher.CurrentTarget, out var cBin);
                if (hasBin && Require<CItem>(cHolder.HeldItem, out var cItem) && Data.TryGet<Item>(cItem.ID, out var ItemData) && ItemData.IsIndisposable)
                    continue;

                // Activate if idle
                if (cLauncher.State == LauncherState.Idle)
                {
                    cLauncher.FlightDelta = 1.0f;
                    cLauncher.State = LauncherState.Launching;
                    Set(entity, new CPreventItemTransfer());
                    Set(cLauncher.CurrentTarget, new CPreventItemTransfer());
                }

                // Tick flight
                cLauncher.FlightDelta -= Time.DeltaTime * cLauncher.LaunchSpeed;
                
                // Upon flight ending
                if (cLauncher.FlightDelta <= 0f)
                {
                    // Begin cooldown
                    Require<CTakesDuration>(entity, out var cDuration);
                    var cooldown = cLauncher.Cooldown * 1.75f;
                    cDuration.Total = cooldown;
                    cDuration.Remaining = cooldown;
                    Set(entity, cDuration);

                    // Set reloading state
                    cLauncher.State = LauncherState.Reloading;
                    cLauncher.FlightDelta = 0;
                    EntityManager.RemoveComponent<CPreventItemTransfer>(entity);
                    EntityManager.RemoveComponent<CPreventItemTransfer>(cLauncher.CurrentTarget);

                    // Transport item between locations
                    if (Require<CItemHolder>(cLauncher.CurrentTarget, out var _))
                    {
                        if (Require(cLauncher.CurrentTarget, out CTrampoline cTrampoline))
                        {
                            cTrampoline.LaunchedTo = true;
                            cTrampoline.Distance = cLauncher.LaunchDistance;
                            cTrampoline.Orientation = GetComponent<CPosition>(entity).Rotation;
                            Set(cLauncher.CurrentTarget, cTrampoline);
                        }
                        ctx.UpdateHolder(cHolder.HeldItem, cLauncher.CurrentTarget);
                    } else if (Require<CItemProvider>(cLauncher.CurrentTarget, out var cProvider))
                    {
                        if (Has<CDynamicItemProvider>(cLauncher.CurrentTarget) && Require(cHolder.HeldItem, out CItem cHeldItem))
                        {
                            cProvider.ProvidedItem = cHeldItem.ID;
                            cProvider.ProvidedComponents = cHeldItem.Items;
                        }

                        if (cProvider.Available < cProvider.Maximum)
                            cProvider.Available++;
                        ctx.Destroy(cHolder.HeldItem);
                        Set(cLauncher.CurrentTarget, cProvider);
                    } else if (hasBin)
                    {
                        cBin.CurrentAmount++;
                        ctx.Destroy(cHolder.HeldItem);
                        Set(cLauncher.CurrentTarget, cBin);
                    }
                }

                Set(entity, cLauncher);
            }
        }


    }
}
