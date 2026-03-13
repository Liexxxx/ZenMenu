using BepInEx;
using GorillaLocomotion;
using System;
using System.Linq;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using ZenMenu.AssetBundling;
using ZenMenu.Utillities;

namespace ZenMenu.Menu
{
    [HarmonyLib.HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    public class Main : MonoBehaviour
    {
        public static void Prefix()
        {
            try
            {
                bool r = ControllerInputPoller.instance.rightControllerPrimaryButton;
                bool l = ControllerInputPoller.instance.leftControllerPrimaryButton;
                bool q = Keyboard.current[Key.Q].isPressed;

                if (r || q)
                {
                    menu.SetActive(true);
                    InitMenu(false);
                    if (!reference) CreateRefrance(false);
                    RecenterMenu();
                }
                else if (l)
                {
                    menu.SetActive(true);
                    InitMenu(true);
                    if (!reference) CreateRefrance(true);
                    RecenterMenu();
                }
                else CloseMenu();
            }
            catch { }

            try
            {
                if (ButtonSets.buttonSets_.TryGetValue(CurrentCategory, out var found))
                    foreach (ButtonModule m in found.Values)
                    {
                        if (m.Enabled && m.Toggable) try { m.Method?.Invoke(); } catch { }
                        if (m.EnableMethod != null && m.Enabled) try { m.EnableMethod.Invoke(); } catch { }
                    }
            }
            catch { }
        }

        public static void InitMenu(bool L)
        {
            if (!ButtonSets.buttonSets_.TryGetValue(CurrentCategory, out var found)) return;

            var mods = found.Values.Skip(page * 6).Take(6).ToArray();
            ButtonModule[] modules = new ButtonModule[6];

            for (int i = 0; i < 6; i++) modules[i] = i < mods.Length ? mods[i] : deadModule;

            InitButtons(modules);
            menu.SetActive(true);
        }

        public static void CloseMenu()
        {
            menu.SetActive(false);
            if (reference) GameObject.Destroy(reference);
            reference = null;
            buttonCollider = null;
            HasSetPostion = false;
        }

        public static void RecenterMenu()
        {
            Transform cam = Camera.main.transform;
            if (!HasSetPostion)
            {
                menu.transform.position = cam.position + cam.forward * 1f;
                menu.transform.rotation = Quaternion.LookRotation(cam.position - menu.transform.position, cam.up);
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
            Vector3 camPos = Camera.main.transform.position;
            Vector3 toCam = camPos - menu.transform.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.0001f) menu.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
        }

        static void SetupRef(GameObject r)
        {
            r.GetComponent<Renderer>().material.color = Data.ZenMenu.transform.GetChild(0).GetComponent<Renderer>().material.color;
            r.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            r.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        }

        static void CreateRefrance(bool L)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference.transform.parent = L ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            SetupRef(reference);
            buttonCollider = reference.GetComponent<SphereCollider>();
            buttonCollider.isTrigger = true;
            var rb = reference.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
        }

        public static void InitButtons(ButtonModule[] Module)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject button = menu.GetNamedChild($"ModButton{i}");
                if (!button) continue;

                button.GetNamedChild("ModName").GetComponent<TextMeshPro>().text = Module[i].ModName;

                var bc = button.GetComponent<BoxCollider>();
                if (!bc) bc = button.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.size = button.transform.localScale;

                var btn = button.GetComponent<ButtonCollider>();
                if (!btn) btn = button.AddComponent<ButtonCollider>();
                btn.Button = Module[i];
            }

            if (!menu.GetNamedChild("NextPage").GetComponent<ButtonCollider>()) menu.GetNamedChild("NextPage").AddComponent<ButtonCollider>();
            if (!menu.GetNamedChild("PrevPage").GetComponent<ButtonCollider>()) menu.GetNamedChild("PrevPage").AddComponent<ButtonCollider>();
        }

        public static void ToggleMod(ButtonModule Mod)
        {
            if (Mod.Toggable && Mod.Method != null) { Mod.Enabled = !Mod.Enabled; try { Mod.Method.Invoke(); } catch { } }
            else if (!Mod.Toggable) try { Mod.Method.Invoke(); } catch { }

            if (Mod.EnableMethod != null && Mod.Enabled) try { Mod.EnableMethod.Invoke(); } catch { }
            if (Mod.DisableMethod != null && !Mod.Enabled) try { Mod.DisableMethod.Invoke(); } catch { }
        }

        public static void ChangePage(bool prev)
        {
            if (!ButtonSets.buttonSets_.TryGetValue(CurrentCategory, out var found)) return;

            int maxPage = Mathf.CeilToInt(found.Count / 6f) - 1;

            if (prev) page--;
            else page++;

            if (page < 0) page = maxPage;
            if (page > maxPage) page = 0;

            InitMenu(false);
        }

        public static GameObject reference;
        public static SphereCollider buttonCollider;
        public static GameObject menu => Data.ZenMenu;

        static bool HasSetPostion = false;
        static Vector3 lockedPosition;

        public static string CurrentCategory= "Main";
        static int page;

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