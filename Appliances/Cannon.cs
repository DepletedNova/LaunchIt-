using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using LaunchIt.Components;
using LaunchIt.Processes;
using LaunchIt.Views;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LaunchIt.Appliances
{
    public class Cannon : CustomAppliance
    {
        public override string UniqueNameID => "tall_launch_plate";
        public override GameObject Prefab => GetPrefab("Cannon");
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, CreateApplianceInfo("Cannon", "Fires items to any depot in the same channel!", new()
            {
                new()
                {
                    Title = "Gunpowder",
                    RangeDescription = "12 Tile Radius"
                },
                new()
                {
                    Title = "Channel",
                    Description = "Interact to swap between channels during prep phase"
                }
            }, new()))
        };
        public override bool IsPurchasable => true;
        public override PriceTier PriceTier => PriceTier.VeryExpensive;
        public override int PurchaseCostOverride => 650;
        public override RarityTier RarityTier => RarityTier.Rare;
        public override ShoppingTags ShoppingTags => ShoppingTags.Automation;

        public override List<Appliance> Upgrades => new()
        {
            GetCastedGDO<Appliance, SmartLauncher>()
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CForceFixedRotation(),
            new CFixedRotation(),
            new CCannon
            {
                MaxRange = 12f,
                MinRange = 0f,
                Cooldown = 1.25f,
                FireSpeed = 1,
            },
            new CChannelUser(),
            new CTakesDuration
            {
                IsInverse = true,
                Mode = InteractionMode.Items,
            },
            new CDisplayDuration
            {
                Process = GetCastedGDO<Process, CannonProcess>().ID
            }
        };

        public override void OnRegister(Appliance gdo)
        {
            Prefab.ApplyMaterialToChild("Rail", "Metal Very Dark", "Metal Dark", "Metal");

            var Frame = Prefab.GetChild("Frame");
            Frame.ApplyMaterial("Plastic - Dark Grey", "Metal Very Dark", "Metal - Brass");
            Frame.ApplyMaterialToChild("Cannon/Barrel", "Metal Black");
            Frame.ApplyMaterialToChild("Pivot", "Metal Black");
            Frame.ApplyMaterialToChild("Antenna", "Metal", "Plastic - Dark Grey", "Paper", "Metal Very Dark");
            Frame.ApplyMaterialToChild("Bulb", "Indicator Light");
            Frame.ApplyMaterialToChild("Red Wire", "Plastic - Red");
            Frame.ApplyMaterialToChild("Blue Wire", "Plastic - Blue");

            var Barrel = Frame.GetChild("Cannon");

            var channel = Prefab.TryAddComponent<ChannelView>();
            channel.Lights = Frame.GetChild("Bulb").GetComponent<MeshRenderer>();
            channel.ChannelText = Prefab.AddLabel(new(0, 0.3f, -0.45f), Quaternion.Euler(45, 0, 0)).GetComponent<TextMeshPro>();

            var cannon = Prefab.TryAddComponent<CannonView>();
            cannon.Frame = Frame.transform;
            cannon.Barrel = Frame.transform.Find("Cannon");
            cannon.Particles = Barrel.GetChild("Particle").GetComponent<ParticleSystem>();
            cannon.HoldPoint = Prefab.TryAddComponent<HoldPointContainer>().HoldPoint = Barrel.GetChild("HoldPoint").transform;

            //Prefab.TryAddComponent<CannonRangeView>().RadiusObject = Prefab.GetChild("Range").ApplyMaterial("Neon Light - Red");
        }
    }
}