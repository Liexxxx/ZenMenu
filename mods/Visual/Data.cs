using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZenMenu_.mods.Visual
{
    internal class Data
    {
        public static List<Player> PlayersRegistered = new List<Player>();
        public static List<Player> OldPlayers = new List<Player>();
        public static Dictionary<VRRig, SkinnedMeshRenderer> originalRenderers = new Dictionary<VRRig, SkinnedMeshRenderer>();
        public static Dictionary<VRRig, SkinnedMeshRenderer> clonedRenderers = new Dictionary<VRRig, SkinnedMeshRenderer>();
        public static Dictionary<VRRig, LineRenderer> tracerLines = new Dictionary<VRRig, LineRenderer>();
        public static Dictionary<VRRig, TextMesh> distanceLabels = new Dictionary<VRRig, TextMesh>();
        public static Dictionary<VRRig, LineRenderer> boxRenderers = new Dictionary<VRRig, LineRenderer>();
        public static Dictionary<VRRig, TrailRenderer> trailRenderers = new Dictionary<VRRig, TrailRenderer>();
        public static bool ShouldRefresh;
        public static bool RestoredMaterials;
        public static void FlushPlayerCache() => PlayersRegistered.Clear();
    }
}
