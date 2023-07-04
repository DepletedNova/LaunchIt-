using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace LaunchIt.Views
{
    public class CannonRangeView : UpdatableObjectView<CannonRangeView.ViewData>
    {
        public GameObject RadiusObject;

        protected bool Radius;
        protected override void UpdateData(ViewData data)
        {
            RadiusObject.SetActive(data.ShowRadius);
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Cannons;
            protected override void Initialise()
            {
                Cannons = GetEntityQuery(new QueryHelper().All(typeof(CCannon), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                if (Cannons.IsEmpty)
                    return;

                using var entities = Cannons.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    var view = GetComponent<CLinkedView>(entity);
                    SendUpdate(view, new()
                    {
                        ShowRadius = Has<CRangeMarker>(entity)
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool ShowRadius;
            [Key(1)] public FixedListInt128 TargetX;
            [Key(2)] public FixedListInt128 TargetY;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<CannonRangeView>();

            public bool IsChangedFrom(ViewData check) => ShowRadius != check.ShowRadius;
        }
    }
}
