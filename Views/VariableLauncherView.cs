using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static LaunchIt.Components.CVariableLauncher;

namespace LaunchIt.Views
{
    public class VariableLauncherView : UpdatableObjectView<VariableLauncherView.ViewData>
    {
        public Color InactiveColor = new(0.85f, 0.0f, 0.0f);
        public Color ActiveColor = new(0.0f, 0.85f, 0.0f);
        public MeshRenderer Light;

        protected override void UpdateData(ViewData data)
        {
            if (Light != null)
            {
                Light.material.color = data.Value ? ActiveColor : InactiveColor;
            }
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery VariableLaunchers;
            protected override void Initialise()
            {
                VariableLaunchers = GetEntityQuery(new QueryHelper().All(typeof(CItemLauncher), typeof(CVariableLauncher), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                if (VariableLaunchers.IsEmpty)
                    return;

                using var entities = VariableLaunchers.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    var cLauncher = GetComponent<CItemLauncher>(entity);
                    var cSwapper = GetComponent<CVariableLauncher>(entity);
                    var view = GetComponent<CLinkedView>(entity);
                    bool val;
                    switch(cSwapper.Switch)
                    {
                        case Variable.SingleMultiple:
                            val = cLauncher.SingleTarget; break;
                        case Variable.FarNear:
                            val = cLauncher.TargetFar; break;
                        default:
                            val = false; break;
                    }
                    SendUpdate(view, new()
                    {
                        Value = val
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool Value;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<VariableLauncherView>();

            public bool IsChangedFrom(ViewData check) => Value != check.Value;
        }
    }
}
