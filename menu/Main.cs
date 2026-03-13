using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using ZenMenu.AssetBundling;
using ZenMenu.Utillities;

namespace ZenMenu.Menu
{
    [HarmonyLib.HarmonyPatch(typeof(GorillaLocomotion.GTPlayer), "LateUpdate")]
    public class Main
    {
        public void Prefix()
        {
            try
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton || UnityEngine.Input.GetKeyDown(KeyCode.Q))
                {
                    InitMenu(false);
                    CreateRefrance(false);
                    RecenterMenu();
                }
                else if (!ControllerInputPoller.instance.leftControllerPrimaryButton)
                {
                    CloseMenu();
                }
                if (ControllerInputPoller.instance.leftControllerPrimaryButton)
                {
                    InitMenu(true);
                    CreateRefrance(true);
                    RecenterMenu();
                }
                else if (!ControllerInputPoller.instance.rightControllerPrimaryButton && !UnityEngine.Input.GetKeyDown(KeyCode.Q))
                {
                    CloseMenu();
                }
            }
            catch { }
            try
            {
                //Mod Runner
                if (ButtonSets.buttonSets_.TryGetValue(CurrentCategory, out var foundDict))
                {
                    foreach (ButtonModule Modules in foundDict.Values)
                    {
                        if (Modules.Enabled && Modules.Toggable)
                        {
                            try { Modules.Method?.Invoke(); } catch { }
                        }
                        if (Modules.EnableMethod != null)
                        {
                            if (Modules.Enabled)
                            {
                                try { Modules.EnableMethod.Invoke(); } catch { }
                            }
                        }
                    }
                }
            }catch { }
        }
        public static void InitMenu(bool L)
        {
            if (ButtonSets.buttonSets_.TryGetValue(CurrentCategory, out var foundDict))
            {
                var realModules = foundDict.Values.Take(5).ToArray();
                ButtonModule[] modules = new ButtonModule[6];
                modules[0] = deadModule;
                for (int i = 0; i < realModules.Length; i++)
                {
                    modules[i + 1] = realModules[i];
                }
                InitButtons(modules);
            }
        }
        public static void CloseMenu()
        {
            menu.SetActive(false);
            GameObject.Destroy(reference);
            buttonCollider = null;
            HasSetPostion = false;
        }
        public static void RecenterMenu()
        {
            Transform cam = Camera.main.transform;
            if (cam == null) return;
            if (!HasSetPostion)
            {
                menu.transform.position = cam.position + cam.forward * 1.0f + cam.up * 0.0f;
                menu.transform.rotation = Quaternion.LookRotation(menu.transform.position - cam.position, cam.up);
                HasSetPostion = true;
                lockedPosition = menu.transform.position;
            }
            else
            {
                menu.transform.position = lockedPosition;
                SetMenuRotationTowardCamera();
            }
        }
        static void SetMenuRotationTowardCamera()
        {
            if (menu == null || Camera.main == null) return;
            Vector3 camPos = Camera.main.transform.position;
            Vector3 toCam = camPos - menu.transform.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.0001f)
                menu.transform.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
        }
        static void SetupRef(GameObject r)
        {
            r.GetComponent<Renderer>().material.color = Data.ZenMenu.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color;
            r.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            r.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        }
        void CreateRefrance(bool L)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference.transform.parent = L ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            SetupRef(reference);
            buttonCollider = reference.GetComponent<SphereCollider>();
            buttonCollider.isTrigger = true;
            var rb1 = reference.AddComponent<Rigidbody>(); rb1.isKinematic = true; rb1.useGravity = false;
        }
        public static void InitButtons(ButtonModule[] Module)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject button = menu.GetNamedChild($"ModButton{i}");
                if (button == null) { }
                else
                {
                    button.GetNamedChild("ModName").GetComponent<TextMeshPro>().text = Module[i].ModName;
                    GameObject.Destroy(button.GetComponent<BoxCollider>());
                    GameObject.Destroy(button.GetComponent<Rigidbody>());
                    button.AddComponent<BoxCollider>();
                    var collider = button.GetComponent<BoxCollider>();
                    collider.isTrigger = true;
                    collider.size = button.transform.localScale;
                    button.AddComponent<ButtonCollider>().Button = Module[i];
                }
            }
            menu.GetNamedChild("NextPage").AddComponent<ButtonCollider>();
            menu.GetNamedChild("PrevPage").AddComponent<ButtonCollider>();
        }
        public static void ToggleMod(ButtonModule Mod)
        {
            if (Mod.Toggable && Mod.Method != null)
            {
                Mod.Enabled = !Mod.Enabled;
                try { Mod.Method.Invoke(); } catch { }
            }
            else if (!Mod.Toggable)
            {
                try { Mod.Method.Invoke(); } catch { }
            }
            if (Mod.EnableMethod != null && Mod.Enabled) try { Mod.EnableMethod.Invoke(); } catch { }
            if (Mod.DisableMethod != null && !Mod.Enabled) try { Mod.DisableMethod.Invoke(); } catch { }
        }
        public static void ChangePage(bool prev)
        {

        }
        public static GameObject reference;
        public static SphereCollider buttonCollider;
        public static GameObject menu = Data.ZenMenu;

        static bool HasSetPostion = false;
        static Vector3 lockedPosition;

        public static string CurrentCategory;
        static ButtonModule deadModule = new ButtonModule(
            "Empty",
            "Placeholder",
            false,
            false,
            Method_: null,
            Catagory_: "None"
        );
    }
}
