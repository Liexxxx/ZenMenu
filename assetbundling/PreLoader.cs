using BepInEx;
using GorillaExtensions;
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace ZenMenu.AssetBundling
{
    [BepInPlugin("ZenMenu.Assets", "PreLoader", "0.0.0")]
    internal class PreLoader : BaseUnityPlugin
    {
        void Awake()
        {
            StartCoroutine(LoadAssets());
        }

        public System.Collections.IEnumerator LoadAssets()
        {
            string[] resources = new string[]
            {
                "ZenMenu.Resources.ZenMenu_Prefab",
                "ZenMenu.Resources.Notification_Prefab"
            };
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            for (int i = 0; i < resources.Length; i++)
            {
                string resourceName = resources[i];
                System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                    continue;
                try
                {
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    byte[] bundleBytes = ms.ToArray();
                    ms.Dispose();
                    UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromMemory(bundleBytes);
                    UnityEngine.GameObject prefab = bundle.LoadAllAssets<UnityEngine.GameObject>()[0];
                    if (resourceName.Contains("ZenMenu_Prefab"))
                        Data.ZenMenu = UnityEngine.Object.Instantiate<UnityEngine.GameObject>(prefab);
                    else
                        Data.Notification = UnityEngine.Object.Instantiate<UnityEngine.GameObject>(prefab);
                }
                finally
                {
                    stream.Dispose();
                    if (Data.ZenMenu.IsNotNull())
                    {
                        Data.ZenMenu.GetComponentInChildren<MeshRenderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                        Data.ZenMenu.GetComponentInChildren<TextMeshPro>().gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("TextMeshPro/Mobile/BitmapCustomSATOutline");
                        Data.Notification.GetComponentInChildren<MeshRenderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                        Data.Notification.GetComponentInChildren<TextMeshPro>().gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("TextMeshPro/Mobile/BitmapCustomSATOutline");
                    }
                }
            }

            yield break;
        }
    }
}