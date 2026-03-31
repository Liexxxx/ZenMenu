using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ZenMenu_.utillities.GunLib
{
    internal class GunData
    {
        public static VRRig LockedTarget;
        public static Photon.Realtime.Player LockedPlayer => PhotonNetwork.PlayerList.FirstOrDefault(x => x.ActorNumber == LockedTarget.Creator.ActorNumber);
        public static NetPlayer LockedNetPlayer => LockedTarget.Creator;

        public static Transform GunTransform;
    }
}
