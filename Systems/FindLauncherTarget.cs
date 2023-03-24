using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Systems
{
    [UpdateAfter(typeof(LaunchItems))]
    public class FindLauncherTarget : GameSystemBase, IModSystem
    {
        private EntityQuery Launchers;
        protected override void Initialise()
        {
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
                cLauncher.CurrentTarget = default(Entity);

                // Check tiles in range
                Vector3 prevPos = cPos.Position;
                Vector3 targetPos;
                for (int i = 1; i <= cLauncher.TileRange; i++)
                {
                    targetPos = cPos.Forward(-i) + cPos.Position;

                    // Check if valid position
                    if (!cLauncher.CrossesWalls && !CanReach(prevPos, targetPos))
                        break;

                    prevPos = targetPos;

                    // Check if ahead is a valid target
                    Entity occupant = GetOccupant(targetPos);
                    if (occupant == default(Entity) ||
                        !Require<CAppliance>(occupant, out var cAppliance) || !Require<CItemHolder>(occupant, out var cHolder) ||
                        !(cLauncher.TargetSmart && cLauncher.SmartTargetID == cAppliance.ID) ||
                        Has<CPreventItemTransfer>(occupant))
                        continue;

                    // Disallow multi-targetting for Smart Launchers
                    if (cHolder.HeldItem != default(Entity))
                    {
                        if (cLauncher.TargetSmart || cLauncher.CrossesWalls)
                            break;
                        else continue;
                    }

                    cLauncher.LaunchDistance = i;
                    cLauncher.CurrentTarget = occupant;
                    break;
                }

                Set(entity, cLauncher);
            }
        }

    }
}
