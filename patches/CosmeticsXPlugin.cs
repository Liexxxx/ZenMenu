using BepInEx;
using BepInEx.Bootstrap;
using GorillaNetworking;
using GorillaTag.CosmeticSystem;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace ZenMenu.Patches
{
    [BepInPlugin("vivet.cosmeticsx", "CosmeticsX", "1.0.0")]
    public class CosmeticsXPlugin : BaseUnityPlugin
    {
        private static Harmony _harmony;

        private void Awake()
        {
            _harmony = new Harmony("vivet.cosmeticsx");
            TryPatch(typeof(CosmeticsController), "GetCosmeticsPlayFabCatalogData", BindingFlags.Public | BindingFlags.Instance, null, nameof(CosmeticsX.GetCosmeticsPlayFabCatalogData_Postfix));
            TryPatch(typeof(CosmeticsController), "UnlockItem", BindingFlags.NonPublic | BindingFlags.Instance, null, nameof(CosmeticsX.UnlockItem_Postfix));
            TryPatch(typeof(CosmeticsV2Spawner_Dirty), "_Step5_InitializeVRRigsAndCosmeticsControllerFinalize", BindingFlags.NonPublic | BindingFlags.Static, null, nameof(CosmeticsX.Step5_Postfix));
            TryPatch(typeof(CosmeticsController), "PressWardrobeItemButton", BindingFlags.Public | BindingFlags.Instance, nameof(CosmeticsX.PressWardrobeItemButton_Prefix), null);
            TryPatch(typeof(CosmeticWardrobeProximityDetector), "IsUserNearWardrobe", BindingFlags.Public | BindingFlags.Static, null, nameof(CosmeticsX.IsUserNearWardrobe_Postfix));
            Debug.Log("[CosmeticsX] Awake complete, all patches applied");
            Chainloader.ManagerObject.AddComponent<CosmeticsXRunner>();
        }

        private static void TryPatch(Type targetType, string targetMethod, BindingFlags flags, string prefixName, string postfixName)
        {
            try
            {
                var original = targetType.GetMethod(targetMethod, flags);
                if (original == null) { Debug.LogWarning($"[CosmeticsX] Could not find {targetType.Name}.{targetMethod}"); return; }
                var prefix = prefixName != null ? new HarmonyMethod(typeof(CosmeticsX).GetMethod(prefixName, BindingFlags.Public | BindingFlags.Static)) : null;
                var postfix = postfixName != null ? new HarmonyMethod(typeof(CosmeticsX).GetMethod(postfixName, BindingFlags.Public | BindingFlags.Static)) : null;
                _harmony.Patch(original, prefix, postfix);
                Debug.Log($"[CosmeticsX] Patched {targetType.Name}.{targetMethod}");
            }
            catch (Exception ex) { Debug.LogWarning($"[CosmeticsX] Failed to patch {targetType.Name}.{targetMethod}: {ex.Message}"); }
        }
    }

    internal class CosmeticsXRunner : MonoBehaviour
    {
        private float _timer = 0f;
        private const float _interval = 2f;
        private bool _fullyUnlocked = false;

        private void Awake() { DontDestroyOnLoad(gameObject); CosmeticsX.ForceUnlockAll(); }

        private void Update()
        {
            if (_fullyUnlocked) return;
            _timer += Time.deltaTime;
            if (_timer < _interval) return;
            _timer = 0f;
            _fullyUnlocked = CosmeticsX.ForceUnlockAll();
        }
    }

    internal class CosmeticsX
    {
        private static bool _injectionPending = false;
        public static bool ForceUnlockAll()
        {
            try
            {
                if (!CosmeticsController.hasInstance) { Debug.Log("[CosmeticsX] ForceUnlockAll: no instance yet"); return false; }
                var ctrl = CosmeticsController.instance;
                if (ctrl.allCosmetics == null) { Debug.Log("[CosmeticsX] ForceUnlockAll: allCosmetics null"); return false; }
                if (ctrl.allCosmetics.Count == 0) { Debug.Log("[CosmeticsX] ForceUnlockAll: allCosmetics empty"); return false; }

                int added = 0;
                foreach (var cosmetic in ctrl.allCosmetics)
                {
                    if (cosmetic.isNullItem) continue;
                    if (!ctrl.unlockedCosmetics.Contains(cosmetic)) { ctrl.unlockedCosmetics.Add(cosmetic); added++; }
                    if (!ctrl.concatStringCosmeticsAllowed.Contains(cosmetic.itemName)) ctrl.concatStringCosmeticsAllowed += cosmetic.itemName;

                    switch (cosmetic.itemCategory)
                    {
                        case CosmeticsController.CosmeticCategory.Hat: if (!ctrl.unlockedHats.Contains(cosmetic)) ctrl.unlockedHats.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Face: if (!ctrl.unlockedFaces.Contains(cosmetic)) ctrl.unlockedFaces.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Badge: if (!ctrl.unlockedBadges.Contains(cosmetic)) ctrl.unlockedBadges.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Fur: if (!ctrl.unlockedFurs.Contains(cosmetic)) ctrl.unlockedFurs.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Shirt: if (!ctrl.unlockedShirts.Contains(cosmetic)) ctrl.unlockedShirts.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Pants: if (!ctrl.unlockedPants.Contains(cosmetic)) ctrl.unlockedPants.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Arms: if (!ctrl.unlockedArms.Contains(cosmetic)) ctrl.unlockedArms.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Back: if (!ctrl.unlockedBacks.Contains(cosmetic)) ctrl.unlockedBacks.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Chest: if (!ctrl.unlockedChests.Contains(cosmetic)) ctrl.unlockedChests.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.TagEffect: if (!ctrl.unlockedTagFX.Contains(cosmetic)) ctrl.unlockedTagFX.Add(cosmetic); break;
                        case CosmeticsController.CosmeticCategory.Paw:
                            if (cosmetic.isThrowable) { if (!ctrl.unlockedThrowables.Contains(cosmetic)) ctrl.unlockedThrowables.Add(cosmetic); }
                            else { if (!ctrl.unlockedPaws.Contains(cosmetic)) ctrl.unlockedPaws.Add(cosmetic); }
                            break;
                    }
                    PlayerPrefs.SetInt($"unlocked_{cosmetic.itemName}", 1);
                }
                PlayerPrefs.Save();
                ctrl.UpdateWardrobeModelsAndButtons();
                ctrl.UpdateWornCosmetics();
                _injectionPending = false;
                Debug.Log($"[CosmeticsX] ForceUnlockAll done — added {added}, total: {ctrl.unlockedCosmetics.Count}");
                return true;
            }
            catch (Exception ex) { Debug.LogError($"[CosmeticsX] ForceUnlockAll exception: {ex}"); return false; }
        }
        public static void GetCosmeticsPlayFabCatalogData_Postfix()
        {
            Debug.Log("[CosmeticsX] GetCosmeticsPlayFabCatalogData_Postfix — marking injection pending");
            _injectionPending = true;
        }
        public static void UnlockItem_Postfix(CosmeticsController __instance)
        {
            if (!_injectionPending) return;
            if (!__instance.allCosmeticsDict_isInitialized) return;
            Debug.Log($"[CosmeticsX] UnlockItem_Postfix triggered injection — unlocked so far: {__instance.unlockedCosmetics.Count}");
            _injectionPending = false;
            ForceUnlockAll();
        }
        public static void Step5_Postfix()
        {
            ForceUnlockAll();
        }
        public static bool PressWardrobeItemButton_Prefix(CosmeticsController __instance, CosmeticsController.CosmeticItem cosmeticItem, bool isLeftHand, bool isTempCosm)
        {
            if (cosmeticItem.isNullItem) return true;
            try
            {
                var targetSet = isTempCosm ? __instance.tempUnlockedSet : __instance.currentWornSet;
                __instance.ApplyCosmeticItemToSet(targetSet, cosmeticItem, isLeftHand, applyToPlayerPrefs: true);
                __instance.UpdateWornCosmetics(sync: true);
                __instance.ProcessExternalUnlock(cosmeticItem.itemName, autoEquip: true, isLeftHand: isLeftHand);
                Debug.Log($"[CosmeticsX] Equip complete: {cosmeticItem.itemName}");
            }
            catch (Exception ex)
            {
                return true;
            }
            return false; 
        }

        public static void IsUserNearWardrobe_Postfix(ref bool __result) { __result = true; }
    }
}