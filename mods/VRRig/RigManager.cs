using Backtrace.Unity.Model.Breadcrumbs;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZenMenu.Menu;

namespace ZenMenu_.mods.VRRig
{
    [BepInPlugin("org.zen.vrrig","VrrigManager","0.0.0")]
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
            Dih
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
            }
        }

        static GameObject gameobject;
        void Awake()
        {
            if (gameobject == null)
            {
                gameobject = new GameObject("VrrigModManager(@Liex)");
                gameobject.AddComponent<RigManager>();
            }
        }
        public static bool Tpose,
            Griddy,
            GhostMonkey,
            InvisMonkey,
            GrabRig,
            Helecopter,
            Bees,
            Dih;
        bool ToggledGhostorInvis;
        void Update()
        {
            if (Tpose)
            {
                GorillaTagger.Instance.offlineVRRig.rightHandTransform.position = GorillaTagger.Instance.offlineVRRig.bodyTransform.right * 10;
                GorillaTagger.Instance.offlineVRRig.leftHandTransform.position = GorillaTagger.Instance.offlineVRRig.bodyTransform.right * -10;
            }
            if (Griddy)// make anim later
            {

            }
            if (GhostMonkey)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton)
                    ToggledGhostorInvis = true;
                if (ControllerInputPoller.instance.rightControllerSecondaryButton || ControllerInputPoller.instance.leftControllerSecondaryButton)
                    ToggledGhostorInvis = false;
                if (ToggledGhostorInvis)
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                else
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
            if (InvisMonkey)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton)
                    ToggledGhostorInvis = true;
                if (ControllerInputPoller.instance.rightControllerSecondaryButton || ControllerInputPoller.instance.leftControllerSecondaryButton)
                    ToggledGhostorInvis = false;
                if (ToggledGhostorInvis)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = Vector3.zero;
                }
                else
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
            if (GrabRig)
            {
                if (ControllerInputPoller.instance.rightControllerGripFloat > 0.5f || ControllerInputPoller.instance.leftControllerGripFloat > 0.5f)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = ControllerInputPoller.instance.rightControllerGripFloat > 0.5f? GorillaTagger.Instance.rightHandTransform.position : GorillaTagger.Instance.leftHandTransform.position;
                    GorillaTagger.Instance.offlineVRRig.headConstraint.gameObject.SetActive(false);
                }
                else if (ControllerInputPoller.instance.rightControllerGripFloat < 0.5f || ControllerInputPoller.instance.leftControllerGripFloat < 0.5f)
                {
                    GorillaTagger.Instance.offlineVRRig.headConstraint.gameObject.SetActive(false);
                }
            }
            if (Helecopter)
            {
                Tpose = true;
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = Vector3.up * Time.deltaTime * 6f;
                GorillaTagger.Instance.offlineVRRig.transform.rotation = Quaternion.RotateTowards(Quaternion.identity, Quaternion.identity, 360);
            }
            else if (!Main.GetModule("Vrrig","Tpose").Enabled)
                Tpose = false;
            if (Bees)
            {
                Tpose = true;
                var rigs = ZenMenu.patches.VrrigCache.Data.vrrigs.Where(x => !x.isLocal).Select(x => x.transform).ToList();
                if (rigs.Count > 0)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                    int randomIndex = UnityEngine.Random.Range(0, rigs.Count);
                    GorillaTagger.Instance.offlineVRRig.transform.position = rigs[randomIndex].position;
                }
            }
            else if (!Main.GetModule("Vrrig", "Tpose").Enabled)
                Tpose = false;
            if (Dih)
            {
                ZenMenu.AssetBundling.Data.Dih.transform.parent = GorillaTagger.Instance.offlineVRRig.bodyTransform;
                ZenMenu.AssetBundling.Data.Dih.transform.rotation = Quaternion.identity;
                ZenMenu.AssetBundling.Data.Dih.transform.position = GorillaTagger.Instance.offlineVRRig.bodyTransform.position - new Vector3(0, 0.1f, 0);
                ZenMenu.AssetBundling.Data.Dih.transform.forward = GorillaTagger.Instance.offlineVRRig.bodyTransform.forward;
            }
            else
                ZenMenu.AssetBundling.Data.Dih.transform.position = new Vector3(99999, 99999, 99999);
        }
    }
}
