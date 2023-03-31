using Kitchen;
using KitchenData;
using KitchenLib.References;
using KitchenMods;
using LaunchIt.Appliances;
using LaunchIt.Components;
using MessagePack;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Views
{
    public class LauncherView : UpdatableObjectView<LauncherView.ViewData>
    {
        protected float Delta;
        protected float Distance;
        protected float Speed;
        protected int Target = 0;
        protected LauncherState State = LauncherState.Idle;
        protected Vector3 HoldPointOffset;

        protected Vector2 StartPosition;
        protected Vector2 TargetPosition;
        protected Vector2 ControlPosition;

        public Transform HoldPoint;

        public Animator LaunchAnimator;
        public Transform SmartHoldPoint;
        public float HeightMulti = 0.25f;

        private static readonly int Launched = Animator.StringToHash("Launched");

        public void UpdateSmartTarget(int target)
        {
            if (SmartHoldPoint.childCount > 0)
            {
                Destroy(SmartHoldPoint.GetChild(0).gameObject);
            }
            if (GameData.Main.TryGet<Appliance>(target, out var appliance))
            {
                var SignalPrefab = Instantiate(appliance.Prefab, SmartHoldPoint);
                SignalPrefab.transform.SetParent(SmartHoldPoint, false);
                SignalPrefab.transform.localPosition = Vector3.zero;
                SignalPrefab.transform.localScale = Vector3.one * 0.25f;
            }
        }

        protected override void UpdateData(ViewData viewData)
        {
            if (SmartHoldPoint != null && viewData.TargetType != Target)
            {
                UpdateSmartTarget(viewData.TargetType);
            }

            if (viewData.Distance != Distance)
            {
                TargetPosition = new Vector2(viewData.Distance, 0.5f);
                ControlPosition = new Vector2(viewData.Distance / 2.0f, Mathf.Max(viewData.Distance / 1.75f, viewData.Distance / (math.pow(1.75f, viewData.Distance) * 0.15f)));
            }

            Delta = viewData.Delta;
            Distance = viewData.Distance;
            Target = viewData.TargetType;
            State = (LauncherState)viewData.State;
            Speed = viewData.Speed;

            if (LaunchAnimator != null)
            {
                LaunchAnimator.SetBool(Launched, State == LauncherState.Launching);
            }

            if (HoldPointOffset == null || HoldPointOffset == default(Vector3))
            {
                HoldPointOffset = HoldPoint.localPosition;
                StartPosition = new Vector2(-HoldPoint.localPosition.z, HoldPoint.localPosition.y);
            } else if (State != LauncherState.Launching)
            {
                HoldPoint.localPosition = HoldPointOffset;
            }
        }

        public void Update()
        {
            if (State == LauncherState.Launching)
            {
                Delta -= Time.deltaTime * Speed;

                var point = GenericHelper.QuadraticBezier(StartPosition, ControlPosition, TargetPosition, 1.0f - Delta);
                HoldPoint.localPosition = new Vector3(0, point.y, -point.x);
            }
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Launchers;
            protected override void Initialise()
            {
                Launchers = GetEntityQuery(new QueryHelper()
                    .All(typeof(CItemLauncher), typeof(CTakesDuration), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                if (Launchers.IsEmpty)
                    return;

                using var entities = Launchers.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    Require<CLinkedView>(entity, out var view);
                    Require<CItemLauncher>(entity, out var launcher);

                    SendUpdate(view, new()
                    {
                        Delta = launcher.FlightDelta,
                        Distance = launcher.LaunchDistance,
                        State = (int)launcher.State,
                        TargetType = launcher.SmartTargetID,
                        Speed = launcher.LaunchSpeed
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public int State;
            [Key(1)] public int TargetType;
            [Key(2)] public int Distance;
            [Key(3)] public float Delta;
            [Key(4)] public float Speed;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<LauncherView>();

            public bool IsChangedFrom(ViewData check) => State != check.State || TargetType != check.TargetType || Distance != check.Distance || Delta != check.Delta || Speed != check.Speed;
        }
    }
}
