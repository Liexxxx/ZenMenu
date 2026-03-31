using Backtrace.Unity.Model.Breadcrumbs;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;
using ZenMenu.Menu;
using ZenMenu.mods.Safety;
using ZenMenu_.mods.Safety;
using ZenMenu_.utillities.GunLib;

namespace ZenMenu_.mods.VRRig_
{
    [BepInPlugin("org.zen.vrrig", "VrrigManager", "0.0.0")]
    internal class RigManager : BaseUnityPlugin
    {
        public enum Mods
        {
            Tpose,
            Griddy,
            GhostMonkey,
            InvisMonkey,
            GrabRig,
            Helecopter,
            Bees,
            Dih,
            Tih
        }
        public static void EnableMod(Mods Mod)
        {
            switch (Mod)
            {
                case Mods.Tpose:
                    Tpose = !Tpose;
                    break;
                case Mods.Griddy:
                    Griddy = !Griddy;
                    break;
                case Mods.GhostMonkey:
                    GhostMonkey = !GhostMonkey;
                    break;
                case Mods.InvisMonkey:
                    InvisMonkey = !InvisMonkey;
                    break;
                case Mods.GrabRig:
                    GrabRig = !GrabRig;
                    break;
                case Mods.Helecopter:
                    Helecopter = !Helecopter;
                    break;
                case Mods.Bees:
                    Bees = !Bees;
                    break;
                case Mods.Dih:
                    Dih = !Dih;
                    break;
                case Mods.Tih:
                    Tih = !Tih;
                    break;
            }
        }
        void Awake()
        {
            InitManager();
        }
        public static void InitManager()
        {
            if (GameObject.Find("VRRigModManager(@Liex)") == null)
            {
                GameObject obj = new GameObject("VRRigModManager(@Liex)");
                obj.AddComponent<VRRig_.RigManager>();
                obj.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        public static bool Tpose, Griddy, GhostMonkey, InvisMonkey, GrabRig, Helecopter, Bees, Dih,Tih;
        bool ToggledGhost, ToggledInvis;

        void Update()
        {

            if (Tpose)
            {
                if (GorillaTagger.Instance.offlineVRRig.rightHandTransform != null)
                    GorillaTagger.Instance.rightHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.right * 10;
                if (GorillaTagger.Instance.offlineVRRig.leftHandTransform != null)
                    GorillaTagger.Instance.leftHandTransform.position = GorillaTagger.Instance.bodyCollider.transform.right * -10;
            }

            if (GhostMonkey)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton) ToggledGhost = true;
                if (ControllerInputPoller.instance.rightControllerSecondaryButton || ControllerInputPoller.instance.leftControllerSecondaryButton) ToggledGhost = false;
                GorillaTagger.Instance.offlineVRRig.enabled = !ToggledGhost;
            }
            else
            {
                ToggledGhost = false;
                if (!InvisMonkey && !GrabRig) GorillaTagger.Instance.offlineVRRig.enabled = true;
            }

            if (InvisMonkey)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton) ToggledInvis = true;
                if (ControllerInputPoller.instance.rightControllerSecondaryButton || ControllerInputPoller.instance.leftControllerSecondaryButton) ToggledInvis = false;
                if (ToggledInvis)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = Vector3.zero;
                }
                else GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
            else
            {
                ToggledInvis = false;
                if (!GhostMonkey && !GrabRig) GorillaTagger.Instance.offlineVRRig.enabled = true;
            }

            if (GrabRig)
            {
                if (ControllerInputPoller.instance.rightControllerGripFloat > 0.5f || ControllerInputPoller.instance.leftControllerGripFloat > 0.5f)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    if (GorillaTagger.Instance.offlineVRRig.headConstraint != null) GorillaTagger.Instance.offlineVRRig.headConstraint.gameObject.SetActive(false);
                    if (ControllerInputPoller.instance.rightControllerGripFloat > 0.5f && GorillaTagger.Instance.rightHandTransform != null)
                        GorillaTagger.Instance.offlineVRRig.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    else if (GorillaTagger.Instance.leftHandTransform != null)
                        GorillaTagger.Instance.offlineVRRig.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                    if (GorillaTagger.Instance.offlineVRRig.headConstraint != null) GorillaTagger.Instance.offlineVRRig.headConstraint.gameObject.SetActive(true);
                }
            }
            else
            {
                if (!GhostMonkey && !InvisMonkey)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                    if (GorillaTagger.Instance.offlineVRRig.headConstraint != null) GorillaTagger.Instance.offlineVRRig.headConstraint.gameObject.SetActive(true);
                }
            }

            if (Helecopter)
            {
                Tpose = true;
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position += Vector3.up * Time.deltaTime * 6f;
                GorillaTagger.Instance.offlineVRRig.transform.Rotate(0, 360 * Time.deltaTime, 0);
            }
            else if (!Helecopter && !Bees)
            {
                if (Main.GetModule("Vrrig", "Tpose") != null && !Main.GetModule("Vrrig", "Tpose").Enabled) Tpose = false;
            }

            if (Bees)
            {
                Tpose = true;
                if (ZenMenu.patches.VrrigCache.Data.vrrigs.Where(x => x != null && !x.isLocal).Select(x => x.transform).ToList() is System.Collections.Generic.List<Transform> rigs && rigs.Count > 0)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = rigs[UnityEngine.Random.Range(0, rigs.Count)].position;
                }
            }

            if (Dih)
            {
                if (ZenMenu.AssetBundling.Data.Dih != null && GorillaTagger.Instance.offlineVRRig.bodyTransform != null)
                {
                    ZenMenu.AssetBundling.Data.Dih.transform.position = GorillaTagger.Instance.offlineVRRig.bodyTransform.position - new Vector3(0, 0.1f, 0) + GorillaTagger.Instance.offlineVRRig.bodyTransform.forward * 0.15f;
                    ZenMenu.AssetBundling.Data.Dih.transform.rotation = Quaternion.identity;
                    ZenMenu.AssetBundling.Data.Dih.transform.forward = GorillaTagger.Instance.offlineVRRig.bodyTransform.forward;
                    if (GorillaTagger.Instance.offlineVRRig.mainSkin != null && GorillaTagger.Instance.offlineVRRig.mainSkin.material != null)
                    {
                        foreach (Transform child in ZenMenu.AssetBundling.Data.Dih.transform)
                        {
                            if (child == null) continue;
                            if (child.gameObject.name != "TIP" && child.gameObject.GetComponent<MeshRenderer>() != null)
                                child.gameObject.GetComponent<MeshRenderer>().material = GorillaTagger.Instance.offlineVRRig.mainSkin.material;
                        }
                    }
                }
            }
            else if (ZenMenu.AssetBundling.Data.Dih != null)
                ZenMenu.AssetBundling.Data.Dih.transform.position = new Vector3(99999, 99999, 99999);
            if (Tih)
            {
                if (ZenMenu.AssetBundling.Data.Tih != null && GorillaTagger.Instance.offlineVRRig.bodyTransform != null)
                {
                    ZenMenu.AssetBundling.Data.Tih.transform.position = GorillaTagger.Instance.offlineVRRig.bodyTransform.position + new Vector3(0, 0.15f, 0) + GorillaTagger.Instance.offlineVRRig.bodyTransform.forward * 0.15f;
                    ZenMenu.AssetBundling.Data.Tih.transform.rotation = Quaternion.identity;
                    ZenMenu.AssetBundling.Data.Tih.transform.forward = GorillaTagger.Instance.offlineVRRig.bodyTransform.forward;
                    if (GorillaTagger.Instance.offlineVRRig.mainSkin != null && GorillaTagger.Instance.offlineVRRig.mainSkin.material != null)
                    {
                        foreach (Transform child in ZenMenu.AssetBundling.Data.Tih.transform)
                        {
                            if (child == null) continue;
                            if (!child.gameObject.name.Contains("UV") && child.gameObject.GetComponent<MeshRenderer>() != null)
                                child.gameObject.GetComponent<MeshRenderer>().material = GorillaTagger.Instance.offlineVRRig.mainSkin.material;
                        }
                    }
                }
            }
            else if (ZenMenu.AssetBundling.Data.Tih != null)
                ZenMenu.AssetBundling.Data.Tih.transform.position = new Vector3(99999, 99999, 99999);
        }
    }
}
