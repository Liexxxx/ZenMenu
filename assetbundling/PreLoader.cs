using BepInEx;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace ZenMenu.AssetBundling
{
    [BepInPlugin("ZenMenu.Assets", "PreLoader", "0.0.0")]
    internal class PreLoader : BaseUnityPlugin
    {
        void Awake()
        {
            string[] resources = new string[]
            {
                "ZenMenu_.Resources.ZenMenu_Prefab",
                "ZenMenu_.Resources.Notification_Prefab",
                "ZenMenu_.Resources.Dih_Prefab",
                "ZenMenu_.Resources.Tih_Prefab",
                "ZenMenu_.Resources.Gun_Prefab",
            };

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            for (int i = 0; i < resources.Length; i++)
            {
                string resourceName = resources[i];
                System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;
                try
                {
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    byte[] bundleBytes = ms.ToArray();
                    ms.Dispose();
                    UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromMemory(bundleBytes);
                    if (bundle == null) continue;
                    UnityEngine.GameObject prefab = bundle.LoadAllAssets<UnityEngine.GameObject>()[0];
                    if (prefab == null) continue;
                    if (resourceName.Contains("ZenMenu_Prefab"))
                    {
                        Data.ZenMenu = UnityEngine.Object.Instantiate(prefab);
                        Data.ZenMenu.hideFlags = HideFlags.HideAndDontSave;
                    }
                    else if (resourceName.Contains("Notification"))
                    {
                        Data.Notification = UnityEngine.Object.Instantiate(prefab);
                        Data.Notification.hideFlags = HideFlags.HideAndDontSave;
                    }
                    else if (resourceName.Contains("Dih_Prefab"))
                    {
                        Data.Dih = UnityEngine.Object.Instantiate(prefab);
                        Data.Dih.hideFlags = HideFlags.HideAndDontSave;
                    }
                    else if (resourceName.Contains("Tih_Prefab"))
                    {
                        Data.Tih = UnityEngine.Object.Instantiate(prefab);
                        Data.Tih.hideFlags = HideFlags.HideAndDontSave;
                    }
                    else if (resourceName.Contains("Gun_Prefab"))
                    {
                        Data.Gun = UnityEngine.Object.Instantiate(prefab);
                        Data.Gun.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                catch { }
                finally { stream.Dispose(); }
            }

            try
            {
                var gtShader = UnityEngine.Shader.Find("GorillaTag/UberShader");
                foreach (UnityEngine.MeshRenderer mr in Data.ZenMenu.GetComponentsInChildren<UnityEngine.MeshRenderer>(true))
                {
                    if (mr.GetComponent<TMPro.TextMeshPro>() != null) continue;
                    if (gtShader != null) mr.material.shader = gtShader;
                }
                UnityEngine.GameObject.Destroy(Data.ZenMenu.GetComponent<UnityEngine.Collider>());
                Data.ZenMenu.GetComponentsInChildren<UnityEngine.BoxCollider>(true)
                    .Where(c => !c.gameObject.name.ToLower().Contains("prev")
                             && !c.gameObject.name.ToLower().Contains("next")
                             && !c.gameObject.name.ToLower().Contains("mod") && !c.gameObject.name.ToLower().Contains("home"))
                    .ToList()
                    .ForEach(c => UnityEngine.GameObject.Destroy(c));

                Data.ZenMenu.GetComponentsInChildren<UnityEngine.Collider>(true)
                    .Where(c => !c.gameObject.name.ToLower().Contains("prev")
                             && !c.gameObject.name.ToLower().Contains("next")
                             && !c.gameObject.name.ToLower().Contains("mod") && !c.gameObject.name.ToLower().Contains("home"))
                    .ToList()
                    .ForEach(c => UnityEngine.GameObject.Destroy(c));
                if (Data.Dih != null)
                {
                    Data.Dih.GetComponentsInChildren<UnityEngine.Collider>(true).ForEach(c => UnityEngine.GameObject.Destroy(c));
                    foreach (UnityEngine.MeshRenderer mr in Data.Dih.GetComponentsInChildren<UnityEngine.MeshRenderer>(true))
                    {
                        if (gtShader != null) mr.material.shader = gtShader;
                    }
                }
                if (Data.Gun != null)
                {
                    Data.Gun.GetComponentsInChildren<UnityEngine.Collider>(true).ForEach(c => UnityEngine.GameObject.Destroy(c));
                    foreach (UnityEngine.MeshRenderer mr in Data.Gun.GetComponentsInChildren<UnityEngine.MeshRenderer>(true))
                    {
                        if (gtShader != null) mr.material.shader = gtShader;
                    }
                }
                if (Data.Tih != null)
                {
                    Data.Tih.GetComponentsInChildren<UnityEngine.Collider>(true).ForEach(c => UnityEngine.GameObject.Destroy(c));
                    foreach (UnityEngine.MeshRenderer mr in Data.Tih.GetComponentsInChildren<UnityEngine.MeshRenderer>(true))
                    {
                        if (gtShader != null) mr.material.shader = gtShader;
                    }
                }
                foreach (UnityEngine.MeshRenderer mr in Data.Notification.GetComponentsInChildren<UnityEngine.MeshRenderer>(true))
                {
                    if (mr.GetComponent<TMPro.TextMeshPro>() != null) continue;
                    if (gtShader != null) mr.material.shader = gtShader;
                }
                FixTMPForBothEyes(Data.ZenMenu);
                FixTMPForBothEyes(Data.Notification);
            }
            catch { }
        }

        static void FixTMPForBothEyes(GameObject root)
        {
            if (root == null) return;
            foreach (var tm in root.GetComponentsInChildren<TMPro.TextMeshPro>(true))
            {
                var meshRenderer = tm.GetComponent<MeshRenderer>();
                if (meshRenderer == null) continue;
                meshRenderer.enabled = true;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    var m = meshRenderer.materials[i];
                    if (m != null)
                    {
                        m.enableInstancing = false;
                        m.SetOverrideTag("DisableBatching", "True");
                    }
                }
                if (tm.fontMaterial != null)
                {
                    tm.fontMaterial.enableInstancing = false;
                    tm.fontMaterial.SetOverrideTag("DisableBatching", "True");
                }
                tm.gameObject.layer = 0;
            }
        }
    }
}