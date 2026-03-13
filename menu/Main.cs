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

                if (r || Keyboard.current[Key.Q].isPressed)
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
            if (reference) GameObject.Destroy(reference);
            reference = null;
            buttonCollider = null;
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
            buttonCollider.radius = 0.05f;
            var rb = reference.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
        }

        public static void InitButtons(ButtonModule[] modules)
        {
            for (int i = 0; i < 7; i++)
            {
                GameObject button = menu.GetNamedChild($"ModButton{i}");
                if (!button) continue;
                button.GetNamedChild("ModName").GetComponent<TextMeshPro>().text = modules[i].ModName;
                var bc = button.GetComponent<BoxCollider>() ?? button.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                Renderer r = button.GetComponent<Renderer>();
                if (r != null)
                {
                    Vector3 size = r.bounds.size;
                    size.x /= button.transform.lossyScale.x;
                    size.y /= button.transform.lossyScale.y;
                    size.z /= button.transform.lossyScale.z;
                    bc.size = new Vector3(size.x, size.y, Mathf.Max(size.z, 0.02f));
                    bc.center = button.transform.InverseTransformPoint(r.bounds.center);
                }
                var btn = button.GetComponent<ButtonCollider>() ?? button.AddComponent<ButtonCollider>();
                btn.Button = modules[i];
            }
            var next = menu.GetNamedChild("NextPage");
            var prev = menu.GetNamedChild("PrevPage");

            if (next)
            {
                var bc = next.GetComponent<BoxCollider>() ?? next.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                Renderer r = next.GetComponent<Renderer>();
                if (r != null)
                {
                    Vector3 size = r.bounds.size;
                    size.x /= next.transform.lossyScale.x;
                    size.y /= next.transform.lossyScale.y;
                    size.z /= next.transform.lossyScale.z;
                    bc.size = new Vector3(size.x, size.y, Mathf.Max(size.z, 0.02f));
                    bc.center = next.transform.InverseTransformPoint(r.bounds.center);
                }
                var btn = next.GetComponent<ButtonCollider>() ?? next.AddComponent<ButtonCollider>();
                btn.IsPageButton = true;
                btn.IsPrevPage = false;
            }

            if (prev)
            {
                var bc = prev.GetComponent<BoxCollider>() ?? prev.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                Renderer r = prev.GetComponent<Renderer>();
                if (r != null)
                {
                    Vector3 size = r.bounds.size;
                    size.x /= prev.transform.lossyScale.x;
                    size.y /= prev.transform.lossyScale.y;
                    size.z /= prev.transform.lossyScale.z;
                    bc.size = new Vector3(size.x, size.y, Mathf.Max(size.z, 0.02f));
                    bc.center = prev.transform.InverseTransformPoint(r.bounds.center);
                }
                var btn = prev.GetComponent<ButtonCollider>() ?? prev.AddComponent<ButtonCollider>();
                btn.IsPageButton = true;
                btn.IsPrevPage = true;
            }
        }

        public static void ToggleMod(ButtonModule mod)
        {
            if (mod.Toggable)
            {
                mod.Enabled = !mod.Enabled;

                if (mod.Enabled)
                    mod.EnableMethod?.Invoke();
                else
                    mod.DisableMethod?.Invoke();
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
        static bool OpenedWithLeft;

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