using Cosmetics;
using GorillaNetworking;
using GorillaNetworking.Store;
using GorillaTag.Cosmetics;
using GorillaTag.CosmeticSystem;
using HarmonyLib;
using UnityEngine;

namespace Vivet.Patches.Internal
{
    [HarmonyPatch(typeof(GameObject), "CreatePrimitive")]
    public class ShaderFix : MonoBehaviour
    {
        private static void Postfix(GameObject __result)
        {
            __result.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            __result.GetComponent<Renderer>().material.color = Color.black;
            
        }
    }
}