using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZenMenu.Patches
{
    [HarmonyPatch(typeof(SIPlayer), nameof(SIPlayer.PlayerKnockback))]
    internal class NoKnockBack
    {
        private static bool Prefix(Vector3 directionAndMagnitude, bool forceOffGround = true, bool applyExclusionZone = true)
        {
            return false;
        }
    }
}
