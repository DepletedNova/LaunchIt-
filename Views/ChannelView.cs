using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using MessagePack;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace LaunchIt.Views
{
    public class ChannelView : UpdatableObjectView<ChannelView.ViewData>
    {
        public Color InactiveColor = Color.black;
        public float Increment = 0.175f;
        public MeshRenderer Lights;
        public TextMeshPro ChannelText;

        protected override void UpdateData(ViewData data)
        {
            if (Lights != null)
            {
                Lights.material.color = data.ID == 0 ? InactiveColor : Color.HSVToRGB(Increment * (data.ID - 1) % 1.0f, 1.0f, 1.0f);
            }

            if (ChannelText != null)
            {
                ChannelText.gameObject.SetActive(true);
                ChannelText.text = data.ID == 0 ? "" : data.ID.ToString();
            }
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Users;
            protected override void Initialise()
            {
                Users = GetEntityQuery(new QueryHelper().All(typeof(CChannelUser), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                if (Users.IsEmpty)
                    return;

                using var entities = Users.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    var cUser = GetComponent<CChannelUser>(entity);
                    var view = GetComponent<CLinkedView>(entity);
                    SendUpdate(view, new()
                    {
                        ID = cUser.Channel
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public int ID;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<ChannelView>();

            public bool IsChangedFrom(ViewData check) => ID != check.ID;
        }
    }
}
