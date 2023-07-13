global using KitchenLib.Utils;
global using static KitchenLib.Utils.GDOUtils;
global using static KitchenLib.Utils.LocalisationUtils;
global using static KitchenLib.Utils.MaterialUtils;
global using static LaunchIt.Main;
using KitchenData;
using KitchenLib;
using KitchenLib.Customs;
using KitchenLib.Event;
using KitchenLib.References;
using KitchenMods;
using LaunchIt.Appliances;
using LaunchIt.Processes;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace LaunchIt
{
    public class Main : BaseMod
    {
        public const string GUID = "nova.launchit";
        public const string VERSION = "2.0.7";

        public Main() : base(GUID, "LaunchIt!", "Depleted Supernova#1957", VERSION, ">=1.0.0", Assembly.GetExecutingAssembly()) { }

        private static AssetBundle Bundle;

        protected override void OnPostActivate(Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).ToList()[0];
            Bundle.LoadAllAssets<Texture2D>();
            Bundle.LoadAllAssets<Sprite>();

            AddGameData();

            AddIcons();

            Events.BuildGameDataEvent += (_, args) =>
            {
                if (!args.firstBuild)
                    return;
                //AddUpgrades();
            };
        }

        internal void AddGameData()
        {
            MethodInfo AddGDOMethod = typeof(BaseMod).GetMethod(nameof(BaseMod.AddGameDataObject));
            int counter = 0;
            Log("Registering GameDataObjects.");
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsAbstract || typeof(IWontRegister).IsAssignableFrom(type))
                    continue;

                if (typeof(CustomGameDataObject).IsAssignableFrom(type))
                {
                    MethodInfo generic = AddGDOMethod.MakeGenericMethod(type);
                    generic.Invoke(this, null);
                    counter++;
                }
            }
            Log($"Registered {counter} GameDataObjects.");
        }

        private void AddIcons()
        {
            var icons = Bundle.LoadAsset<TMP_SpriteAsset>("LaunchIcons");
            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(icons);
            icons.material = Object.Instantiate(TMP_Settings.defaultSpriteAsset.material);
            icons.material.mainTexture = Bundle.LoadAsset<Texture2D>("LaunchTex");
        }

        private void AddUpgrades()
        {
            (GetExistingGDO(ApplianceReferences.Countertop) as Appliance).Upgrades.Add(GetCastedGDO<Appliance, Depot>());
            (GetExistingGDO(ApplianceReferences.Dumbwaiter) as Appliance).Upgrades.Add(GetCastedGDO<Appliance, Launcher>());
        }

        public static GameObject GetPrefab(string name) => Bundle.LoadAsset<GameObject>(name);
    }

    internal interface IWontRegister { }

    internal static class GenericHelper
    {
        public static void ApplyCounterMaterials(this GameObject prefab)
        {
            var counter = prefab.GetChild("Counter");
            counter.ApplyMaterial("Wood - Default", "Wood 4 - Painted", "Wood 4 - Painted");
            counter.ApplyMaterialToChild("Handle", "Knob");
            counter.ApplyMaterialToChild("Countertop", "Wood - Default");
        }

        public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float m = 1.0f - t;
            float a = m * m;
            float b = 2.0f * m * t;
            float c = t * t;
            float x = a * p0.x + b * p1.x + c * p2.x;
            float y = a * p0.y + b * p1.y + c * p2.y;
            return new(x, y);
        }

        private static GameObject TeleporterLabel;
        public static GameObject AddLabel(this GameObject parent, Vector3 Position, Quaternion Rotation)
        {
            // Initialize label
            if (TeleporterLabel == null)
            {
                TeleporterLabel = (GetExistingGDO(ApplianceReferences.Teleporter) as Appliance).Prefab.GetChild("Label 1");
            }

            var label = Object.Instantiate(TeleporterLabel);
            label.transform.parent = parent.transform;
            label.transform.localPosition = Position;
            label.transform.localRotation = Rotation;
            label.transform.localScale *= 0.8f;
            label.name = "Label";
            return label;
        }
    }
}
