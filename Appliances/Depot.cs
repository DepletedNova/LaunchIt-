using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using System.Collections.Generic;
using UnityEngine;

namespace LaunchIt.Appliances
{
    public class Depot : CustomAppliance
    {
        public static int DepotID { get; private set; }

        public override string UniqueNameID => "depot";
        public override GameObject Prefab => GetPrefab("Depot");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Depot", "A location that can be targeted by any Launch Plate.", new()
            {
                new()
                {
                    Title = "Buffet",
                    Description = "Customers can grab food from it."
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.Medium;
        public override RarityTier RarityTier => RarityTier.Uncommon;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CApplianceGrabPoint()
        };

        public override void AttachDependentProperties(GameData gameData, GameDataObject GDO)
        {
            base.AttachDependentProperties(gameData, GDO);
            DepotID = GDO.ID;
        }

        public override void OnRegister(Appliance gdo)
        {
            Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Prefab.transform.Find("HoldPoint");

            Prefab.ApplyCounterMaterials();
            Prefab.ApplyMaterialToChild("Depot", "Metal- Shiny", "Wood 1", "Metal Very Dark");
            Prefab.ApplyMaterialToChild("Sigils", "Plastic - Yellow");
        }
    }
}
