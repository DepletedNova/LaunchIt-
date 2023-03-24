using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Components;
using LaunchIt.Processes;
using LaunchIt.Views;
using System.Collections.Generic;
using UnityEngine;
using static LaunchIt.Components.CItemLauncher;

namespace LaunchIt.Appliances
{
    public class SmartLaunchPlate : CustomAppliance
    {
        public override string UniqueNameID => "smart_launch_plate";
        public override GameObject Prefab => GetPrefab("Smart Launch Plate");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Smart Launch Plate", "It knows where to aim!", new()
            {
                new()
                {
                    Title = "Launching",
                    Description = "Launches items to any target location in front of itself.",
                    RangeDescription = "Can reach up to 4 tiles"
                },
                new()
                {
                    Title = "Smart",
                    Description = "Items will only launch to the first target of the set type.",
                    RangeDescription = "Set in Prep Phase"
                },
                new()
                {
                    Title = "Slow",
                    Description = "Performs <sprite name=\"launcher_0\"> 50% slower."
                }
            }, new()))
        };

        public override bool IsPurchasable => true;
        public override bool IsPurchasableAsUpgrade => true;
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override int PurchaseCostOverride => 350;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, LaunchPlate>()
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CItemLauncher
            {
                TileRange = 4,
                Cooldown = 1.50f,
                TargetSmart = true,
                LaunchSpeed = 0.35f
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
            Prefab.ApplyCounterMaterials();
            Prefab.ApplyMaterialToChild("Base", "Wood 1");
            Prefab.ApplyMaterialToChild("Signal", "Metal Very Dark");
            var plate = Prefab.GetChild("Plate");
            plate.ApplyMaterial("Metal- Shiny", "Metal Very Dark", "Metal");
            plate.ApplyMaterialToChild("Sigils", "Plastic - Red");
            plate.ApplyMaterialToChild("Track", "Metal");

            var launchView = Prefab.TryAddComponent<LauncherView>();
            launchView.HoldPoint = Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Prefab.transform.Find("HoldPoint");
            launchView.LaunchAnimator = Prefab.GetComponent<Animator>();
            launchView.SmartHoldPoint = Prefab.transform.Find("SmartHold");
        }
    }
}
