using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using LaunchIt.Components;
using LaunchIt.Views;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using TMPro;
using UnityEngine;

namespace LaunchIt.Appliances
{
    public class Depot : CustomAppliance
    {
        public static int StaticID { get; private set; }

        public override string UniqueNameID => "depot";
        public override GameObject Prefab => GetPrefab("Depot");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Depot", "A deposit for cannons! Customers will eat food off of it.", new()
            {
                new()
                {
                    Title = "Channel",
                    Description = "Interact to swap between channels during prep phase"
                }
            }, new()))
        };
        public override bool IsPurchasableAsUpgrade => true;
        public override PriceTier PriceTier => PriceTier.Medium;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation | ShoppingTags.Technology;

        public override List<Appliance> Upgrades => new()
        {
            GetExistingGDO(ApplianceReferences.Freezer) as Appliance,
            GetExistingGDO(ApplianceReferences.Workstation) as Appliance
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CApplianceGrabPoint(),
            new CFixedRotation(),
            new CForceFixedRotation(),
            new CChannelUser
            {
                Receiver = true
            }
        };

        public override void AttachDependentProperties(GameData gameData, GameDataObject GDO)
        {
            base.AttachDependentProperties(gameData, GDO);
            StaticID = GDO.ID;
        }

        public override void OnRegister(Appliance gdo)
        {
            Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Prefab.transform.Find("HoldPoint");

            Prefab.ApplyMaterialToChild("Depot", "Metal- Shiny", "Metal Very Dark", "Plastic - Red");
            Prefab.ApplyMaterialToChild("Name", "Metal Dark");
            Prefab.ApplyMaterialToChild("Wire", "Plastic - Blue");
            Prefab.ApplyMaterialToChild("Antenna", "Metal", "Plastic - Dark Grey", "Paper");
            Prefab.ApplyMaterialToChild("Tech", "Plastic - Dark Grey");
            Prefab.ApplyMaterialToChild("Stand", "Metal", "Metal Dark");
            Prefab.ApplyMaterialToChild("Light", "Indicator Light");

            var channel = Prefab.TryAddComponent<ChannelView>();
            channel.Lights = Prefab.GetChild("Light").GetComponent<MeshRenderer>();
            channel.ChannelText = Prefab.AddLabel(new(0, 0.46f, -0.35f), Quaternion.Euler(45, 0, 0)).GetComponent<TextMeshPro>();
        }
    }
}
