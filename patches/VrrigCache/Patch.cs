using BepInEx;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZenMenu.patches.VrrigCache
{
    internal class Patch : MonoBehaviourPunCallbacks
    {
        public static GameObject GameObject;

        public static void InitCache()
        {
            GameObject = new GameObject("VrrigCache_Zen").AddComponent<Patch>().gameObject;
        }
        private IEnumerator FetchRigsDelayed()
        {
            yield return new WaitForSeconds(2f);
            Data.vrrigs.Clear();

            HashSet<string> roomPlayerIds = new HashSet<string>();
            foreach (Player player in PhotonNetwork.PlayerList)roomPlayerIds.Add(player.UserId);
            foreach (VRRig rig in FindObjectsOfType<VRRig>())
            {
                if (!roomPlayerIds.Contains(rig.Creator.UserId)) continue;
                if (Data.vrrigs.Contains(rig)) continue;
                Data.vrrigs.Add(rig);
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            StartCoroutine(FetchRigsDelayed());
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            StartCoroutine(FetchRigsDelayed());
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            StartCoroutine(FetchRigForNewPlayer(newPlayer));
        }

        private IEnumerator FetchRigForNewPlayer(Player newPlayer)
        {
            yield return new WaitForSeconds(2f);
            foreach (VRRig rig in FindObjectsOfType<VRRig>())
            {
                if (Data.vrrigs.Contains(rig)) continue;
                if (rig.Creator.UserId == newPlayer.UserId)
                {
                    Data.vrrigs.Add(rig);
                    break;
                }
            }
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            VRRig toRemove = null;
            foreach (VRRig rig in Data.vrrigs)
            {
                if (rig.Creator.UserId == otherPlayer.UserId)
                {
                    toRemove = rig;
                    break;
                }
            }
            if (toRemove != null)
            {
                Data.vrrigs.Remove(toRemove);
            }
        }
        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Data.vrrigs.Clear();
        }

        public override void OnLeftLobby()
        {
            base.OnLeftLobby();
            Data.vrrigs.Clear();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            Data.vrrigs.Clear();
        }
    }
    [BepInEx.BepInPlugin("org.zen.VrrigCache","RigCaching","1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        void Awake() => Patch.InitCache();
    }
}
