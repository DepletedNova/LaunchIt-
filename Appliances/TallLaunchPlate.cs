using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Components;
using LaunchIt.Processes;
using LaunchIt.Views;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace LaunchIt.Appliances
{
    public class TallLaunchPlate : CustomAppliance
    {
        public override string UniqueNameID => "tall_launch_plate";
        public override GameObject Prefab => GetPrefab("Tall Launch Plate");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("High Launch Plate", "Take me to the moon!", new()
            {
                new()
                {
                    Title = "Launching",
                    Description = "Launches items to any target location in front of itself.",
                    RangeDescription = "Can reach up to 4 tiles"
                },
                new()
                {
                    Title = "Overarching",
                    Description = "Items can be launched over walls onto any depot."
                },
                new()
                {
                    Title = "Greased",
                    Description = "Performs <sprite name=\"launcher_0\"> 25% faster."
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override bool IsPurchasableAsUpgrade => true;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, SmartLaunchPlate>()
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CItemLauncher
            {
                TileRange = 4,
                Cooldown = 0.75f,
                CrossesWalls = true,
                LaunchSpeed = 1.5f
            },
            new CTakesDuration
            {
                IsInverse = true,
                Mode = InteractionMode.Items,
                Manual = false,
            },
            new CDisplayDuration
            {
                Process = GetCastedGDO<Process, LauncherProcess>().ID
            }
        };

        public override void OnRegister(Appliance gdo)
        {
            var counter = Prefab.GetChild("Counter");
            counter.ApplyMaterial("Wood - Default", "Wood 4 - Painted", "Wood 4 - Painted");
            counter.ApplyMaterialToChild("Handle", "Knob");

            var chassis = Prefab.GetChild("Chassis");
            chassis.ApplyMaterial("Wood - Default", "Wood 1");
            chassis.ApplyMaterialToChild("Pivot", "Metal");
            chassis.ApplyMaterialToChild("Arm", "Metal- Shiny", "Metal Dark", "Metal Very Dark", "Metal");
            chassis.ApplyMaterialToChild("Arrows", "Plastic - Blue");

            var launchView = Prefab.TryAddComponent<LauncherView>();
            launchView.HoldPoint = Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Prefab.transform.Find("HoldPoint");
            launchView.LaunchAnimator = Prefab.GetComponent<Animator>();
            launchView.HeightMulti = 10f;
        }
    }
}
