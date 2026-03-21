using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ZenMenu.Patches
{
    [System.ComponentModel.Description(PluginInfo.Description)]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    internal class PatchPlugin : BaseUnityPlugin
    {
        private void Awake() => GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        public static bool IsPatched { get; private set; }
        public static int PatchErrors { get; private set; }
        private static Harmony instance;
        public const string InstanceId = PluginInfo.GUID;


        public void OnPlayerSpawned()
        {
            if (IsPatched) return;
            instance??= new Harmony(PluginInfo.GUID);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.GetCustomAttribute<HarmonyPatch>() != null))
            {
                try
                {
                    instance.CreateClassProcessor(type).Patch();
                }
                catch
                {
                    PatchErrors++;
                }
            }
            IsPatched = true;
            if (VRRig.LocalRig.mainSkin.material != null)
            {
                foreach (Transform Children in AssetBundling.Data.Dih.transform)
                {
                    if (Children.gameObject.name != "TIP")
                    {
                        Children.gameObject.GetComponent<MeshRenderer>().material = VRRig.LocalRig.mainSkin.material;
                    }
                }
            }
        }
    }
    public class PluginInfo
    {
        public const string GUID = "org.zen.gorillatag.menu";
        public const string Name = "Zen";
        public const string Description = "Created by Liex";
        public const string Version = "1.0.0";
    }
}
