using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace LaunchIt.Views
{
    public class TrampolineView : UpdatableObjectView<TrampolineView.ViewData>
    {
        public Transform HoldPoint;

        protected Vector3 StandardHoldPosition;

        protected float Delta;
        protected int Distance;
        protected Quaternion Direction;

        protected Vector2 StartPosition;
        protected Vector2 ControlPosition;
        protected Vector2 EndPosition;

        protected override void UpdateData(ViewData data)
        {
            if (StandardHoldPosition == null || StandardHoldPosition == default(Vector3))
            {
                StandardHoldPosition = HoldPoint.localPosition;
                StartPosition = new(0, HoldPoint.localPosition.y);
            }

            Delta = data.Delta;

            if (data.Distance != Distance || data.Rotation != Direction)
            {
                Distance = data.Distance;
                Direction = data.Rotation;

                EndPosition = new(Distance, 0.5f);
                ControlPosition = new(Distance / 2.0f, Distance / 1.5f + 0.5f);
            }
        }

        public void Update()
        {
            if (Delta > 0f)
            {
                Delta -= Time.deltaTime;

                var point = GenericHelper.QuadraticBezier(StartPosition, ControlPosition, EndPosition, 1.0f - Delta);
                HoldPoint.position = (Vector3)math.mul(Direction, new Vector3(0, point.y, -point.x)) + transform.position;
            }
            else
            {
                HoldPoint.localPosition = StandardHoldPosition;
            }
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Trampolines;
            protected override void Initialise()
            {
                Trampolines = GetEntityQuery(new QueryHelper().All(typeof(CTrampoline), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                if (Trampolines.IsEmpty)
                    return;

                using var entities = Trampolines.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    var trampoline = GetComponent<CTrampoline>(entity);
                    var view = GetComponent<CLinkedView>(entity);
                    SendUpdate(view, new()
                    {
                        Delta = trampoline.FlightDelta,
                        Distance = trampoline.Distance,
                        Rotation = trampoline.Orientation
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public float Delta;
            [Key(1)] public int Distance;
            [Key(2)] public Quaternion Rotation;
            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<TrampolineView>();

            public bool IsChangedFrom(ViewData check) => check.Delta != Delta || check.Distance != Distance || check.Rotation != Rotation;
        }
    }
}
