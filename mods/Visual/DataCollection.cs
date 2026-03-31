using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZenMenu_.mods.Visual
{
    internal class DataCollection : MonoBehaviourPunCallbacks
    {
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            Data.PlayersRegistered.Add(newPlayer);
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            Data.OldPlayers.Add(otherPlayer);
            Data.PlayersRegistered.Remove(otherPlayer);
            Data.ShouldRefresh = true;
        }
    }
}
