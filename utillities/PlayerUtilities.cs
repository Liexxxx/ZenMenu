using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZenMenu.utillities
{
    internal class PlayerUtilities
    {
        public static int FetchPlayerFps(Player p)
        {
            return patches.VrrigCache.Data.vrrigs.Where(x=> x.Creator.UserId == p.UserId).Select(x=> (int)typeof(VRRig).GetField("fps").GetValue(x)).FirstOrDefault();
        }
        public static int GetPlayerPing(Player p)
        {
            return PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime;
        }
        public static bool ImInfected()
        {
            bool infected = GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("infected") ||
                GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("it") ||
                GorillaTagger.Instance.offlineVRRig.mainSkin.material.name.Contains("ice") ||
                GorillaGuardianZoneManager.zoneManagers.Any(x => x.CurrentGuardian == VRRig.LocalRig.OwningNetPlayer) || GorillaPropHuntGameManager.instance.IsInfected(VRRig.LocalRig.Creator);
            return infected;
        }
        public static bool RigInfected(VRRig target)
        {
            if (target.mainSkin.material.name.Contains("infected") || target.mainSkin.material.name.Contains("it") || target.mainSkin.material.name.Contains("ice") || GorillaGuardianZoneManager.zoneManagers.Any(x => x.CurrentGuardian == target.OwningNetPlayer) || GorillaPropHuntGameManager.instance.IsInfected(target.Creator))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
 