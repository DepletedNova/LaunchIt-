global using KitchenLib.Utils;
global using static KitchenLib.Utils.GDOUtils;
global using static KitchenLib.Utils.LocalisationUtils;
global using static KitchenLib.Utils.MaterialUtils;
global using static LaunchIt.Main;
using KitchenLib;
using KitchenLib.Customs;
using KitchenMods;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace LaunchIt
{
    public class Main : BaseMod
    {
        public const string GUID = "nova.launchit";
        public const string VERSION = "1.0.0";

        public Main() : base(GUID, "LaunchIt!", "Depleted Supernova#1957", VERSION, ">=1.0.0", Assembly.GetExecutingAssembly()) { }

        private static AssetBundle Bundle;

        protected override void OnPostActivate(Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).ToList()[0];
            Bundle.LoadAllAssets<Texture2D>();
            Bundle.LoadAllAssets<Sprite>();

            AddGameData();

            AddIcons();
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
    }
}
