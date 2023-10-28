using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Components;
using LaunchIt.Processes;
using LaunchIt.Views;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace LaunchIt.Appliances
{
    public class Launcher : CustomAppliance
    {
        public static int LauncherID { get; private set; }

        public override string UniqueNameID => "launch_plate";
        public override GameObject Prefab => GetPrefab("Launch Plate");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Launcher", "Launches items to any counter, launcher, or trampoline!", new()
            {
                new()
                {
                    Title = "Launching",
                    RangeDescription = "16 Tiles"
                },
                new()
                {
                    Title = "Variable",
                    Description = "Interact to swap between single and multiple targetting types"
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override int PurchaseCostOverride => 300;
        public override RarityTier RarityTier => RarityTier.Uncommon;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, SmartLauncher>(),
            GetCastedGDO<Appliance, Cannon>(),
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CItemLauncher
            {
                MaxTileRange = 16,
                MinTileRange = 0,
                CrossesWalls = true,
                Cooldown = 1.0f,
                LaunchSpeed = 1.0f,
            },
            new CVariableLauncher
            {
                Switch = CVariableLauncher.Variable.SingleMultiple
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

        public override void AttachDependentProperties(GameData gameData, GameDataObject GDO)
        {
            base.AttachDependentProperties(gameData, GDO);
            LauncherID = GDO.ID;
        }

        public override void OnRegister(Appliance gdo)
        {
            Prefab.ApplyCounterMaterials();
            Prefab.ApplyMaterialToChild("Base", "Wood 1");
            Prefab.ApplyMaterialToChild("Antenna", "Metal Dark");
            Prefab.ApplyMaterialToChild("Light", "Indicator Light");
            var plate = Prefab.GetChild("Plate");
            plate.ApplyMaterial("Metal- Shiny", "Metal Very Dark", "Metal");
            plate.ApplyMaterialToChild("Sigils", "Plastic - Yellow");
            plate.ApplyMaterialToChild("Track", "Metal");

            var launchView = Prefab.TryAddComponent<LauncherView>();
            launchView.HoldPoint = Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Prefab.transform.Find("HoldPoint");
            launchView.LaunchAnimator = Prefab.GetComponent<Animator>();

            //Prefab.TryAddComponent<VariableLauncherView>().Light = Prefab.GetChild("Light").GetComponent<MeshRenderer>();
        }
    }
}
