using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace LaunchIt.Systems
{
    public class ResetForcedFixedRotation : GameSystemBase, IModSystem
    {
        private EntityQuery FixedAppliances;
        protected override void Initialise()
        {
            base.Initialise();
            FixedAppliances = GetEntityQuery(new QueryHelper().All(typeof(CForceFixedRotation)));
        }

        protected override void OnUpdate()
        {
            if (FixedAppliances.IsEmpty)
                return;

            using var entities = FixedAppliances.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                if (Has<CForceFixedRotation>(entity) && Require(entity, out CPosition cPos))
                {
                    if (cPos.Rotation != Quaternion.identity)
                    {
                        cPos.Rotation = Quaternion.identity;
                        Set(entity, cPos);
                    }
                }
            }
        }
    }
}
