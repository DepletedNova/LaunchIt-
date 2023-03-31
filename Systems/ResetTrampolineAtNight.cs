using Kitchen;
using KitchenMods;
using LaunchIt.Appliances;
using LaunchIt.Components;
using Unity.Collections;
using Unity.Entities;

namespace LaunchIt.Systems
{
    public class ResetTrampolineAtNight : StartOfNightSystem, IModSystem
    {
        private EntityQuery Trampolines;
        protected override void Initialise()
        {
            base.Initialise();
            Trampolines = GetEntityQuery(new QueryHelper().All(typeof(CTrampoline)));
        }

        protected override void OnUpdate()
        {
            if (Trampolines.IsEmpty)
                return;

            using var entities = Trampolines.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var trampoline = GetComponent<CTrampoline>(entity);
                if (Has<CPreventItemTransfer>(entity))
                    EntityManager.RemoveComponent<CPreventItemTransfer>(entity);
                if (trampoline.Target != Entity.Null && Has<CPreventItemTransfer>(trampoline.Target))
                    EntityManager.RemoveComponent<CPreventItemTransfer>(trampoline.Target);

                Set(entity, new CTrampoline());
            }
        }
    }
}
