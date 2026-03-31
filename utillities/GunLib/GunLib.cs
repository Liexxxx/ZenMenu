using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZenMenu_.utillities.GunLib
{
    internal class GunLib
    {
        public static void Invoke(Action Method, bool LockOn)
        {
            if (ZenMenu.AssetBundling.Data.Gun == null) return;

            bool isHolding = ControllerInputPoller.instance.rightControllerGripFloat > 0.5f ||
                             (Mouse.current != null && Mouse.current.rightButton.isPressed);

            if (isHolding)
            {
                ZenMenu.AssetBundling.Data.Gun.SetActive(true);
                ZenMenu.AssetBundling.Data.Gun.transform.SetParent(GorillaTagger.Instance.rightHandTransform);
                ZenMenu.AssetBundling.Data.Gun.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                ZenMenu.AssetBundling.Data.Gun.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                GunData.GunTransform = ZenMenu.AssetBundling.Data.Gun.transform;
            }
            else
            {
                ZenMenu.AssetBundling.Data.Gun.SetActive(false);
                GunData.LockedTarget = null;
                return;
            }

            if (Mouse.current != null && Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    Debug.DrawRay(ray.origin, ray.direction * 100f,
                        hit.collider.GetComponentInParent<VRRig>() != null ? Color.yellow : Color.green, 0.5f);
                }

                if (LockOn)
                {
                    if (GunData.LockedTarget == null)
                    {
                        if (Physics.Raycast(ray, out RaycastHit lockHit, 100f) &&
                            lockHit.collider.GetComponentInParent<VRRig>() != null &&
                            lockHit.collider.GetComponentInParent<VRRig>() != GorillaTagger.Instance.offlineVRRig &&
                            Mouse.current.leftButton.wasPressedThisFrame)
                        {
                            GunData.LockedTarget = lockHit.collider.GetComponentInParent<VRRig>();
                        }
                    }

                    if (GunData.LockedTarget != null)
                    {
                        Debug.DrawLine(Camera.main.transform.position, GunData.LockedTarget.transform.position, Color.red, 0.5f);
                        if (Mouse.current.leftButton.isPressed) Method?.Invoke();
                    }
                }
                else
                {
                    GunData.LockedTarget = null;
                    if (Mouse.current.leftButton.isPressed) Method?.Invoke();
                }
            }
            else
            {
                Transform barrel = ZenMenu.AssetBundling.Data.Gun.transform.Find("Pistol_N_Barrel");
                if (barrel == null) return;

                if (Physics.Raycast(barrel.position, barrel.forward, out RaycastHit vrHit, 100f))
                {
                    Debug.DrawRay(barrel.position, barrel.forward * 100f,
                        vrHit.collider.GetComponentInParent<VRRig>() != null ? Color.yellow : Color.green, 0.5f);
                }

                if (LockOn)
                {
                    if (GunData.LockedTarget == null)
                    {
                        if (Physics.Raycast(barrel.position, barrel.forward, out RaycastHit vrLockHit, 100f) &&
                            vrLockHit.collider.GetComponentInParent<VRRig>() != null &&
                            vrLockHit.collider.GetComponentInParent<VRRig>() != GorillaTagger.Instance.offlineVRRig &&
                            ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
                        {
                            GunData.LockedTarget = vrLockHit.collider.GetComponentInParent<VRRig>();
                        }
                    }

                    if (GunData.LockedTarget != null)
                    {
                        Debug.DrawLine(barrel.position, GunData.LockedTarget.transform.position, Color.red, 0.5f);
                        if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f) Method?.Invoke();
                    }
                }
                else
                {
                    GunData.LockedTarget = null;
                    if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f) Method?.Invoke();
                }
            }
        }
    }
}