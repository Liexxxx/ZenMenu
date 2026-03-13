using BepInEx;
using TMPro;
using UnityEngine;

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
                Data.ZenMenu.GetComponentInChildren<MeshRenderer>(true).material.shader = Shader.Find("GorillaTag/UberShader");
                GameObject.Destroy(Data.ZenMenu.GetComponentInChildren<BoxCollider>(true));
                Data.ZenMenu.GetComponentInChildren<TextMeshPro>(true).gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("TextMeshPro/Mobile/BitmapCustomSATOutline");
                Data.Notification.GetComponentInChildren<MeshRenderer>(true).material.shader = Shader.Find("GorillaTag/UberShader");
                Data.Notification.GetComponentInChildren<TextMeshPro>(true).gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("TextMeshPro/Mobile/BitmapCustomSATOutline");
            }
            catch { }
        }
    }
}