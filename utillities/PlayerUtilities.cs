using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZenMenu_.utillities
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
    }
}
