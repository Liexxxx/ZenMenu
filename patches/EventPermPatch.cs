using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZenMenu.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(LoadBalancingPeer), "CheckIfOpCanBeSent")]
    internal class EventPermPatch
    {
        private bool Prefix(byte opCode, ServerConnection serverConnection, string opName) => true;
    }
}
