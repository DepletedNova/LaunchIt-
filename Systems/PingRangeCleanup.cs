using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;

namespace LaunchIt.Systems
{
    public class PingRangeCleanup : GameSystemBase
    {
        private EntityQuery Dirty;
        protected override void Initialise()
        {
            base.Initialise();
            Dirty = GetEntityQuery(new QueryHelper().All(typeof(CRangeMarker)).None(typeof(CBeingLookedAt)));
        }

        protected override void OnUpdate()
        {
            if (Dirty.IsEmpty)
                return;

            using var entities = Dirty.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                EntityManager.RemoveComponent<CRangeMarker>(entity);
            }
        }
    }
}
