using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Components;
using LaunchIt.Views;
using System.Collections.Generic;
using UnityEngine;

namespace LaunchIt.Appliances
{
    public class Trampoline : CustomAppliance
    {
        public static int StaticID { get; private set; }

        public override string UniqueNameID => "trampoline";
        public override GameObject Prefab => GetPrefab("Trampoline");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Item Trampoline", "Boing!!!", new()
            {
                new()
                {
                    Title = "Bouncy",
                    Description = "Landing items will have their original travel distance doubled, being launched again"
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Medium;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation | ShoppingTags.Technology;

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CForceFixedRotation(),
            new CTrampoline()
        };

        public override void AttachDependentProperties(GameData gameData, GameDataObject GDO)
        {
            base.AttachDependentProperties(gameData, GDO);
            StaticID = GDO.ID;
        }

        public override void OnRegister(Appliance gdo)
        {
            Prefab.TryAddComponent<TrampolineView>().HoldPoint = Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Prefab.transform.Find("HoldPoint");

            Prefab.ApplyMaterialToChild("Base", "Metal Dark");
            Prefab.ApplyMaterialToChild("Springs", "Metal");
            Prefab.ApplyMaterialToChild("Bounce", "Plastic - Black", "Plastic - Blue");
        }
    }
}
