using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Components;
using LaunchIt.Processes;
using LaunchIt.Views;
using System.Collections.Generic;
using UnityEngine;

namespace LaunchIt.Appliances
{
    public class SmartLauncher : CustomAppliance
    {
        public override string UniqueNameID => "smart_launch_plate";
        public override GameObject Prefab => GetPrefab("Smart Launch Plate");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Smart Launcher", "Place an appliance on it to set its target! Does not launch over walls.", new()
            {
                new()
                {
                    Title = "Smart",
                    Description = "Items will only launch to the first target of the set type",
                    RangeDescription = "3 - 8 Tiles"
                },
                new()
                {
                    Title = "Sticky",
                    Description = "<color=#ff1111>50%</color> slower <sprite name=\"launcher_0\">"
                }
            }, new()))
        };

        public override bool IsPurchasableAsUpgrade => true;
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override int PurchaseCostOverride => 550;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, Cannon>()
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CItemLauncher
            {
                MaxTileRange = 8,
                MinTileRange = 3,
                TargetSmart = true,
                SingleTarget = true,
                Cooldown = 1.50f,
                LaunchSpeed = 1.0f
            },
            new CTakesDuration
            {
                IsInverse = true,
                Mode = InteractionMode.Items,
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
