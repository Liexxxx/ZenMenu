using BepInEx;
using GorillaLocomotion;
using GT_CustomMapSupportRuntime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ZenMenu_.mods.Movement.Data;

namespace ZenMenu_.mods.Movement
{
    [BepInPlugin("zen.mods.movement","MovementManager","0.0.0")]
    internal class MovementManager :BaseUnityPlugin
    {
        void Awake()
        {
            if (GameObject.Find("MovementManager(@Liex)") == null)
            {
                GameObject obj = new GameObject("MovementManager(@Liex)");
                obj.AddComponent<MovementManager>();
                obj.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        public enum Mods
        {
            Longarms,
            Speedboost,
            Noclip,
            Platforms,
            Dash,
            Pull,
            Casting,
            Slidecontrol,
            SlipperySurfaces,
            Fly,
            Slingshotfly,
            GrappleHook
        }
        public static void EnableMod(Mods Mod)
        {
            switch (Mod)
            {
                case Mods.Longarms:
                    if (OriginalArmsScale == null)
                        OriginalArmsScale = GorillaLocomotion.GTPlayer.Instance.transform.localScale;
                    else
                    {
                        if (!LongArms)
                        {
                            GorillaLocomotion.GTPlayer.Instance.transform.localScale = GorillaLocomotion.GTPlayer.Instance.transform.localScale + LongArmsAddition;
                            LongArms = true;
                        }
                        else if (LongArms)
                        {
                            GorillaLocomotion.GTPlayer.Instance.transform.localScale = OriginalArmsScale;
                            LongArms = false;
                        }
                    }
                    break;
                case Mods.Speedboost:
                    if (!Speed)
                    {
                        GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed = GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed + SpeedAddition;
                        GorillaLocomotion.GTPlayer.Instance.jumpMultiplier = GorillaLocomotion.GTPlayer.Instance.jumpMultiplier + SpeedAddition;
                        Speed = true;
                    }
                    else
                    {
                        GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed = GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed - SpeedAddition;
                        GorillaLocomotion.GTPlayer.Instance.jumpMultiplier = GorillaLocomotion.GTPlayer.Instance.jumpMultiplier-  SpeedAddition;
                        Speed = false;
                    }
                    break;
                case Mods.Noclip:
                    noclip = !noclip;
                    break;
                case Mods.Platforms:
                    platforms = !platforms;
                    break;
                case Mods.Dash:
                    dash = !dash;
                    break;
                case Mods.Pull:
                    pull = !pull;
                    break;
                case Mods.Casting:
                    casting = !casting;
                    break;
                case Mods.Slidecontrol:
                    if (orignalControll == null)
                        orignalControll = GorillaLocomotion.GTPlayer.Instance.slideControl;
                    else
                    {
                        GorillaLocomotion.GTPlayer.Instance.slideControl = Slide? orignalControll / 3 : orignalControll * 3;
                    }
                    break;
                case Mods.Fly:
                    fly = !fly;
                    break;
                case Mods.Slingshotfly:
                    slingshotfly = !slingshotfly;
                    break;
                case Mods.GrappleHook:
                    break;
                case Mods.SlipperySurfaces:
                    foreach (var e in Resources.FindObjectsOfTypeAll<SurfaceOverrideSettings>())
                        e.slidePercentage = e.slidePercentage == float.MaxValue? 0 : float.MaxValue;
                    break;
            }
        }

        private static bool noclip, platforms, dash, pull, casting, grapplehook, fly, slingshotfly;
        bool LeftNoclipOn;
        void Update()//width hight depth
        {
            if (noclip)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton)
                {
                    foreach (var Col in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    {
                        Col.enabled = false;
                        LeftNoclipOn = true;
                    }
                }
                else
                {
                    foreach (var Col in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    {
                        Col.enabled = true;
                        LeftNoclipOn = false;
                    }
                }
            }
            else if (LeftNoclipOn)
            {
                foreach (var Col in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    Col.enabled = true;
                    LeftNoclipOn = false;
                }
            }
            if (platforms)
            {
                if (ControllerInputPoller.instance.rightControllerGripFloat > 0.5f)
                {
                    RightPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    RightPlat.transform.localScale = new Vector3(0.6f, 0.1f, 0.9f);
                    if (!RightPlaced)
                    {
                        RightPlat.transform.position = GorillaTagger.Instance.rightHandTransform.position - new Vector3(0, 0.23f, 0);
                        RightPlaced = true;
                    }
                }
                else
                {
                    GameObject.Destroy(RightPlat);
                    RightPlaced = false;
                }
                if (ControllerInputPoller.instance.leftControllerGripFloat > 0.5f)
                {
                    LeftPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    LeftPlat.transform.localScale = new Vector3(0.6f, 0.1f, 0.9f);
                    if (!LeftPlaced)
                    {
                        LeftPlat.transform.position = GorillaTagger.Instance.leftHandTransform.position - new Vector3(0, 0.23f, 0);
                        LeftPlaced = true;
                    }
                }
                else
                {
                    GameObject.Destroy(LeftPlat);
                    LeftPlaced = false;
                }
            }
            if (dash)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton)
                {
                    if (!Dashed)
                        GorillaLocomotion.GTPlayer.Instance.SetPlayerVelocity(GorillaLocomotion.GTPlayer.Instance.bodyVelocityTracker.GetAverageVelocity() + GorillaLocomotion.GTPlayer.Instance.transform.forward * Time.deltaTime * 10f);
                    Dashed = true;
                }
                else if (!ControllerInputPoller.instance.rightControllerPrimaryButton && !ControllerInputPoller.instance.leftControllerPrimaryButton)
                {
                    Dashed = false;
                }
            }
            if (pull)
            {
                if ((!GTPlayer.Instance.IsHandTouching(true) && TouchLeft || !GTPlayer.Instance.IsHandTouching(false) && TouchRight) && (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f || ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f))
                {
                    Vector3 vel = GorillaTagger.Instance.rigidbody.linearVelocity;
                    GTPlayer.Instance.transform.position += new Vector3(vel.x * 0.05f, 0f, vel.z * 0.05f);
                }
                TouchLeft = GTPlayer.Instance.IsHandTouching(true);
                TouchRight = GTPlayer.Instance.IsHandTouching(false);
            }
            if (casting)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton)
                {
                    if (!Casted)
                        GorillaLocomotion.GTPlayer.Instance.SetPlayerVelocity(GorillaLocomotion.GTPlayer.Instance.bodyVelocityTracker.GetAverageVelocity() + GorillaLocomotion.GTPlayer.Instance.transform.up * Time.deltaTime * 10f);
                    Casted = true;
                }
                else if (!ControllerInputPoller.instance.rightControllerPrimaryButton && !ControllerInputPoller.instance.leftControllerPrimaryButton)
                {
                    Casted = false;
                }
            }
            if (fly)
            {
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f || ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f)
                {
                    GorillaLocomotion.GTPlayer.Instance.SetVelocity(Vector3.zero);
                    GorillaLocomotion.GTPlayer.Instance.transform.position += ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f? GorillaTagger.Instance.rightHandTransform.forward * Time.deltaTime * 18 : GorillaTagger.Instance.leftHandTransform.forward * Time.deltaTime * 18;
                }
            }
            if (slingshotfly)
            {
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f || ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f)
                {
                    GorillaLocomotion.GTPlayer.Instance.gameObject.GetComponent<Rigidbody>().AddForce(ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f ? GorillaTagger.Instance.rightHandTransform.forward : GorillaTagger.Instance.leftHandTransform.forward, ForceMode.VelocityChange);
                }
            }
            if (grapplehook)
            {

            }
        }
    }
}
