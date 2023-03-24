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
    public class LaunchPlate : CustomAppliance
    {
        public static int LauncherID { get; private set; }

        public override string UniqueNameID => "launch_plate";
        public override GameObject Prefab => GetPrefab("Launch Plate");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Launch Plate", "Thwump and splat!", new()
            {
                new()
                {
                    Title = "Launching",
                    Description = "Launches items to any target location in front of itself.",
                    RangeDescription = "Can reach up to 8 tiles"
                },
                new()
                {
                    Title = "Varying",
                    Description = "Items can only be launched towards depots but any depots ahead can be targeted."
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Expensive;
        public override RarityTier RarityTier => RarityTier.Uncommon;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, TallLaunchPlate>()
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CItemLauncher
            {
                TileRange = 8,
                Cooldown = 1.0f,
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

        public override void AttachDependentProperties(GameData gameData, GameDataObject GDO)
        {
            base.AttachDependentProperties(gameData, GDO);
            LauncherID = GDO.ID;
        }

        public override void OnRegister(Appliance gdo)
        {
            Prefab.ApplyCounterMaterials();
            Prefab.ApplyMaterialToChild("Base", "Wood 1");
            var plate = Prefab.GetChild("Plate");
            plate.ApplyMaterial("Metal- Shiny", "Metal Very Dark", "Metal");
            plate.ApplyMaterialToChild("Sigils", "Plastic - Yellow");
            plate.ApplyMaterialToChild("Track", "Metal");

            var launchView = Prefab.TryAddComponent<LauncherView>();
            launchView.HoldPoint = Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Prefab.transform.Find("HoldPoint");
            launchView.LaunchAnimator = Prefab.GetComponent<Animator>();
        }
    }
}
