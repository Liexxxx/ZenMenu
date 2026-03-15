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
                "ZenMenu_.Resources.Notification_Prefab"
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
                    else
                    {
                        Data.Notification = UnityEngine.Object.Instantiate(prefab);
                        Data.Notification.hideFlags = HideFlags.HideAndDontSave;
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
                             && !c.gameObject.name.ToLower().Contains("mod"))
                    .ToList()
                    .ForEach(c => UnityEngine.GameObject.Destroy(c));

                Data.ZenMenu.GetComponentsInChildren<UnityEngine.Collider>(true)
                    .Where(c => !c.gameObject.name.ToLower().Contains("prev")
                             && !c.gameObject.name.ToLower().Contains("next")
                             && !c.gameObject.name.ToLower().Contains("mod"))
                    .ToList()
                    .ForEach(c => UnityEngine.GameObject.Destroy(c));
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