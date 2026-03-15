using BepInEx;
using GorillaLocomotion;
using System;
using System.Collections.Generic;
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

                if (r || Keyboard.current[Key.Q].isPressed)
                {
                    menu.SetActive(true);
                    InitMenu(false);
                    if (!reference) CreateReference(false);
                    RecenterMenu();
                }
                else if (l)
                {
                    menu.SetActive(true);
                    InitMenu(true);
                    if (!reference) CreateReference(true);
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
            ButtonModule[] modules = new ButtonModule[7];
            modules[0] = deadModule;
            for (int i = 0; i < 6; i++)
                modules[i + 1] = i < mods.Length ? mods[i] : deadModule;
            InitButtons(modules);
            menu.SetActive(true);
            OpenedWithLeft = L;
        }

        public static void CloseMenu()
        {
            menu.SetActive(false);
            if (reference) { GameObject.Destroy(reference); reference = null; }
            if (reference2) { GameObject.Destroy(reference2); reference2 = null; }
            buttonCollider = null;
            buttonCollider2 = null;
            HasSetPostion = false;
            OpenedWithLeft = false;
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

        public static void ChangeCataory(string Catagory)
        {
            CurrentCategory = Catagory;
            page = 0;
            InitMenu(OpenedWithLeft);
        }

        static void SetMenuRotationTowardCamera()
        {
            Vector3 camPos = Camera.main.transform.position;
            Vector3 toCam = camPos - menu.transform.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.0001f) menu.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
        }

        static Color MenuColor
        {
            get
            {
                if (Data.ZenMenu == null || Data.ZenMenu.transform.childCount == 0) return Color.gray;
                var rend = Data.ZenMenu.transform.GetChild(0).GetComponent<Renderer>();
                return rend != null ? rend.material.color : Color.gray;
            }
        }

        static void SetupRef(GameObject r)
        {
            r.name = "ButtoonRefrance";
            var rend = r.GetComponent<Renderer>();
            if (rend != null) rend.material.color = MenuColor;
            r.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            r.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        }

        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference.transform.parent = isRightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            reference2.transform.parent = isRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
            SetupRef(reference);
            SetupRef(reference2);
            buttonCollider = reference.GetComponent<SphereCollider>();
            buttonCollider2 = reference2.GetComponent<SphereCollider>();
            buttonCollider.isTrigger = true;
            buttonCollider2.isTrigger = true;
            buttonCollider.radius = 0.05f;
            buttonCollider2.radius = 0.05f;
            var rb1 = reference.AddComponent<Rigidbody>();
            rb1.isKinematic = true;
            rb1.useGravity = false;
            var rb2 = reference2.AddComponent<Rigidbody>();
            rb2.isKinematic = true;
            rb2.useGravity = false;
        }

        static readonly Dictionary<int, Color> _originalRendererColors = new Dictionary<int, Color>();
        static readonly Dictionary<int, Color> _originalTMPColors = new Dictionary<int, Color>();

        const float EnabledBrightenAmount = 0.4f;

        static Color GetEnabledColor(Color baseColor) => Color.Lerp(baseColor, Color.white, EnabledBrightenAmount);

        static void ApplyButtonVisualState(GameObject button, ButtonModule module)
        {
            if (button == null || module == null) return;
            bool isEnabled = module.Toggable && module.Enabled && module.ModName != "Empty";

            var renderers = new List<Renderer>();
            button.GetComponentsInChildren(true, renderers);
            foreach (Renderer rend in renderers)
            {
                if (rend == null) continue;
                if (!_originalRendererColors.ContainsKey(rend.GetInstanceID()))
                    _originalRendererColors[rend.GetInstanceID()] = rend.material.color;
                rend.material.color = isEnabled ? GetEnabledColor(_originalRendererColors[rend.GetInstanceID()]) : _originalRendererColors[rend.GetInstanceID()];
            }

            var tmps = new List<TextMeshPro>();
            button.GetComponentsInChildren(true, tmps);
            foreach (TextMeshPro tmp in tmps)
            {
                if (tmp == null) continue;
                int id = tmp.GetInstanceID();
                if (!_originalTMPColors.ContainsKey(id))
                    _originalTMPColors[id] = tmp.color;
                tmp.color = isEnabled ? GetEnabledColor(_originalTMPColors[id]) : _originalTMPColors[id];
            }
        }


        public static void InitButtons(ButtonModule[] modules)
        {
            if (modules == null) return;

            for (int i = 0; i < 7; i++)
            {
                GameObject button = menu.GetNamedChild($"ModButton{i}");
                if (!button) continue;
                ButtonModule mod = modules[i];
                button.GetNamedChild("ModName").GetComponent<TextMeshPro>().text = mod.ModName;
                ApplyButtonVisualState(button, mod);
                var btn = button.GetComponent<ButtonCollider>() ?? button.AddComponent<ButtonCollider>();
                btn.Button = mod ?? deadModule;
            }

            GameObject next = menu.GetNamedChild("NextPage");
            GameObject prev = menu.GetNamedChild("PrevPage");
            GameObject home = menu.GetNamedChild("HomeButton");

            if (next)
            {
                var btn = next.GetComponent<ButtonCollider>() ?? next.AddComponent<ButtonCollider>();
                btn.IsPageButton = true;
                btn.IsPrevPage = false;
            }

            if (prev)
            {
                var btn = prev.GetComponent<ButtonCollider>() ?? prev.AddComponent<ButtonCollider>();
                btn.IsPageButton = true;
                btn.IsPrevPage = true;
            }

            if (home)
            {
                var btn = home.GetComponent<ButtonCollider>() ?? home.AddComponent<ButtonCollider>();
                btn.IsHome = true;
            }
        }

        public static void ToggleMod(ButtonModule mod)
        {
            if (mod.Toggable)
            {
                mod.Enabled = !mod.Enabled;
                if (mod.Enabled) mod.EnableMethod?.Invoke();
                else mod.DisableMethod?.Invoke();
            }
            else
            {
                mod.Method?.Invoke();
            }
            InitMenu(OpenedWithLeft);
        }

        public static void ChangePage(bool prev)
        {
            if (!ButtonSets.buttonSets_.TryGetValue(CurrentCategory, out var found)) return;
            int maxPage = Mathf.CeilToInt(found.Count / 6f) - 1;
            if (prev) page--;
            else page++;
            if (page < 0) page = maxPage;
            if (page > maxPage) page = 0;
            InitMenu(OpenedWithLeft);
        }

        public static void ReturnHome()
        {
            CurrentCategory = "Main";
            InitMenu(OpenedWithLeft);
        }

        public static bool OpenedWithLeft;

        public static GameObject reference;
        public static GameObject reference2;
        public static SphereCollider buttonCollider;
        public static SphereCollider buttonCollider2;
        public static GameObject menu => Data.ZenMenu;

        static bool HasSetPostion = false;
        static Vector3 lockedPosition;

        public static string CurrentCategory = "Main";
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