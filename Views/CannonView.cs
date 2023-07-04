using Kitchen;
using KitchenMods;
using LaunchIt.Components;
using MessagePack;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static LaunchIt.Components.CCannon;

namespace LaunchIt.Views
{
    public class CannonView : UpdatableObjectView<CannonView.ViewData>
    {
        // Set
        public Transform Frame;
        public Transform Barrel;
        public Transform HoldPoint;
        public ParticleSystem Particles;

        // Cached
        protected float FireSpeed;
        protected CannonState State;
        protected Vector3 TargetPosition;

        protected float FlightDelta;
        protected float RotationDelta;

        // Local
        protected Quaternion CurrentBarrelOrientation;
        protected Quaternion CurrentFrameOrientation;

        protected Quaternion TargetBarrelOrientation;
        protected Quaternion TargetFrameOrientation;

        protected Vector2 StartPoint;
        protected Vector2 ControlPoint;
        protected Vector2 EndPoint;

        protected Quaternion HorizontalEuler;

        protected Vector3 LocalHoldPoint;

        protected override void UpdateData(ViewData data)
        {
            if (LocalHoldPoint == null || LocalHoldPoint == default(Vector3))
            {
                LocalHoldPoint = HoldPoint.localPosition;
            }

            // Calculate Current/Target
            if (TargetPosition != data.TargetPosition)
            {
                // Horizontal
                var localPosition = data.TargetPosition - transform.position;
                var horizontalAngle = Mathf.Atan2(localPosition.x, localPosition.z) * Mathf.Rad2Deg;

                // Path
                var distance = Vector3.Distance(Frame.position, data.TargetPosition);

                // Update variables
                StartPoint = new(0, 0.5f);
                ControlPoint = new(distance / 2f, Mathf.Max(distance / 2f, distance / (math.pow(1.75f, distance) * 0.1f) + 0.5f));
                EndPoint = new(distance, 0.5f);

                CurrentFrameOrientation = Frame.transform.rotation;
                TargetFrameOrientation = Quaternion.Euler(-90, 0, horizontalAngle + 180);

                CurrentBarrelOrientation = Barrel.transform.localRotation;
                TargetBarrelOrientation = Quaternion.Euler(-90 + (Mathf.Atan2(ControlPoint.y - 0.5f, ControlPoint.x) * Mathf.Rad2Deg), 0, 0);

                HorizontalEuler = Quaternion.Euler(0, horizontalAngle, 0);
            }

            var newState = (CannonState)data.State;
            if (State != newState && newState == CannonState.Firing)
            {
                Particles.Play();
            }

            FireSpeed = data.FireSpeed;
            State = newState;
            TargetPosition = data.TargetPosition;
            FlightDelta = data.FlightDelta;
            RotationDelta = data.RotationDelta;

            if (State == CannonState.Idle || State == CannonState.Reloading)
            {
                HoldPoint.transform.localPosition = LocalHoldPoint;
            }
        }

        public void Update()
        {
            if (RotationDelta > 0 && State == CannonState.Turning)
            {
                RotationDelta -= Time.deltaTime * 0.5f;

                Frame.transform.rotation = Quaternion.Lerp(CurrentFrameOrientation, TargetFrameOrientation, 1.0f - RotationDelta);
                Barrel.transform.localRotation = Quaternion.Lerp(CurrentBarrelOrientation, TargetBarrelOrientation, 1.0f - RotationDelta);
            }

            if (FlightDelta > 0 && State == CannonState.Firing)
            {
                FlightDelta -= Time.deltaTime * FireSpeed;

                var point = GenericHelper.QuadraticBezier(StartPoint, ControlPoint, EndPoint, 1.0f - FlightDelta);
                HoldPoint.transform.position = (Vector3)math.mul(HorizontalEuler, new Vector3(0, point.y, point.x)) + transform.position;
            }
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery CannonQuery;
            protected override void Initialise()
            {
                CannonQuery = GetEntityQuery(new QueryHelper().All(typeof(CCannon), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                if (CannonQuery.IsEmpty)
                    return;

                using var cannons = CannonQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in cannons)
                {
                    var cCannon = GetComponent<CCannon>(entity);
                    var cView = GetComponent<CLinkedView>(entity);
                    SendUpdate(cView, new()
                    {
                        FireSpeed = cCannon.FireSpeed,
                        State = (int)cCannon.State,
                        TargetPosition = cCannon.Target == Entity.Null ? Vector3.zero : GetComponent<CPosition>(cCannon.Target).Position,
                        FlightDelta = cCannon.FlightDelta,
                        RotationDelta = cCannon.RotationDelta
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public int State;
            [Key(1)] public Vector3 TargetPosition;
            [Key(2)] public float FlightDelta;
            [Key(3)] public float RotationDelta;
            [Key(4)] public float FireSpeed;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<CannonView>();

            public bool IsChangedFrom(ViewData check) => 
                State != check.State || TargetPosition != check.TargetPosition || 
                FlightDelta != check.FlightDelta || RotationDelta != check.RotationDelta ||
                FireSpeed != check.FireSpeed;
        }
    }
}
