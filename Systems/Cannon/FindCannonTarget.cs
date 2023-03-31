using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static LaunchIt.Components.CCannon;

namespace LaunchIt.Systems.Cannon
{

    public class FindCannonTarget : GameSystemBase, IModSystem
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
                var cChannel = GetComponent<CChannelUser>(cEntity);
                var cPos = GetComponent<CPosition>(cEntity);

                // Ignore non-idle cannons & empty channels
                if (cCannon.State != CannonState.Idle || !CollectReceivers.Channels.ContainsKey(cChannel.Channel))
                    continue;

                // Reset target if it is unavailable
                if (cCannon.Target != Entity.Null && Require(cCannon.Target, out CItemHolder cTHolder))
                {
                    if (cTHolder.HeldItem != Entity.Null)
                    {
                        cCannon.Target = Entity.Null;
                    }
                    else continue; // don't calculate target if target is valid
                }

                // Cycle through potential targets
                foreach (var pTarget in CollectReceivers.Channels[cChannel.Channel])
                {
                    // Ignore non-empty targets
                    if (!Require(pTarget, out CItemHolder cPHolder) || cPHolder.HeldItem != Entity.Null)
                        continue;

                    // Ignore busy targets
                    if (Has<CPreventItemTransfer>(pTarget))
                        continue;

                    // Check if in range
                    var cPPos = GetComponent<CPosition>(pTarget);
                    var dist = Mathf.Round(Vector3.Distance(cPos.Position, cPPos.Position));
                    if (dist > cCannon.MaxRange || dist < cCannon.MinRange)
                        continue;

                    // Begin rotation
                    cCannon.State = CannonState.Turning;
                    cCannon.RotationDelta = 1f;
                    cCannon.Target = pTarget;
                    break;
                }

                Set(cEntity, cCannon);
            }
        }
    }
}
