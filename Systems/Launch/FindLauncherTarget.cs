using Kitchen;
using KitchenData;
using KitchenLib.References;
using KitchenMods;
using LaunchIt.Appliances;
using LaunchIt.Components;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Systems
{
    [UpdateBefore(typeof(LaunchItems))]
    public class FindLauncherTarget : GameSystemBase, IModSystem
    {
        private EntityQuery Launchers;
        protected override void Initialise()
        {
            base.Initialise();
            Launchers = GetEntityQuery(new QueryHelper()
                .All(typeof(CItemLauncher), typeof(CTakesDuration), typeof(CItemHolder))
                .None(typeof(CPreventItemTransfer)));
        }

        protected override void OnUpdate()
        {
            if (Launchers.IsEmpty)
                return;

            using var entities = Launchers.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                Require<CItemLauncher>(entity, out var cLauncher);
                if (cLauncher.State != LauncherState.Idle)
                    continue;

                Require<CPosition>(entity, out var cPos);

                cLauncher.LaunchDistance = 0;
                cLauncher.CurrentTarget = Entity.Null;

                Require<CItemHolder>(entity, out var cHolder);
                if (cHolder.HeldItem == Entity.Null || !Require(cHolder.HeldItem, out CItem cItem) || !Data.TryGet(cItem.ID, out Item ItemData))
                    continue;

                // Check tiles in range
                Vector3 prevPos = cPos.Position;
                Vector3 targetPos;
                bool inverse = cLauncher.TargetFar;
                for (int i = inverse ? cLauncher.MaxTileRange + 1 : cLauncher.MinTileRange - 1; inverse ? i >= cLauncher.MinTileRange: i <= cLauncher.MaxTileRange;)
                {
                    if (inverse) i--;
                    else i++;

                    targetPos = cPos.Forward(-i) + cPos.Position;

                    if (targetPos == prevPos) continue;

                    // Check if valid position
                    if (!inverse && !cLauncher.CrossesWalls && !CanReach(prevPos, targetPos))
                            break;

                    prevPos = targetPos;

                    // Check if ahead is a valid target
                    Entity occupant = GetOccupant(targetPos);
                    if (occupant == Entity.Null || !Require<CAppliance>(occupant, out var cAppliance) || Has<CPreventItemTransfer>(occupant))
                        continue;

                    // Check target type
                    if (!ValidSmartTarget(cLauncher, occupant, cAppliance) && !ValidGenericTarget(cLauncher, occupant, cAppliance))
                        continue;

                    // Check valid Target
                    bool hasHolder = Require<CItemHolder>(occupant, out var cTargetHolder);
                    bool hasGrabber = Require<CConveyPushItems>(occupant, out var cGrabber);
                    bool hasProvider = Require<CItemProvider>(occupant, out var cProvider);
                    bool hasBin = Require<CApplianceBin>(occupant, out var cBin);
                    if (hasHolder || hasProvider || hasBin)
                    {
                        var heldItem = GetComponent<CItem>(cHolder.HeldItem);
                        if (hasGrabber && cGrabber.GrabSpecificType && (heldItem.ID != cGrabber.SpecificType || !heldItem.Items.Equals(cGrabber.SpecificComponents)))
                            continue;

                        bool flag2 = (hasHolder && cTargetHolder.HeldItem == Entity.Null) ||
                            (hasProvider && !cProvider.PreventReturns && (cProvider.Available < cProvider.Maximum || cProvider.Maximum == 0) &&
                                ((Require(occupant, out CDynamicItemProvider cDynamicProvider) && cProvider.Available == 0 && ItemData.ItemStorageFlags == cDynamicProvider.StorageFlags) || 
                                cProvider.ProvidedItem == cItem.ID && cProvider.ProvidedComponents.Equals(cItem.Items))) ||
                            (hasBin && cBin.Capacity > cBin.CurrentAmount);

                        if (!flag2)
                        {
                            // Single Target / Multi-Target
                            if (cLauncher.SingleTarget)
                                break;
                            else continue;
                        }
                    }
                    else continue;

                    cLauncher.LaunchDistance = i;
                    cLauncher.CurrentTarget = occupant;
                    break;
                }

                Set(entity, cLauncher);
            }
        }

        private bool ValidSmartTarget(CItemLauncher cLauncher, Entity Target, CAppliance cTargetAppliance) => 
            cLauncher.TargetSmart && cLauncher.SmartTargetID == cTargetAppliance.ID;

        private bool ValidGenericTarget(CItemLauncher cLauncher, Entity Target, CAppliance cTargetAppliance) => 
            !cLauncher.TargetSmart && Has<CItemHolder>(Target) &&
            (Has<CItemLauncher>(Target) || Has<CChannelUser>(Target) || cTargetAppliance.ID == ApplianceReferences.Countertop || cTargetAppliance.ID == Trampoline.StaticID);
    }
}
