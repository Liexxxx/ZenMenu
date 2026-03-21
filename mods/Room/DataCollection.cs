using BepInEx;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZenMenu_.mods.Room
{
    internal class DataCollection : MonoBehaviourPunCallbacks
    {
        static GameObject GameObject;
        public static void InitCollection()
        {
            if (GameObject == null)
            {
                GameObject = new GameObject("RoomManagerDataCollection");
                GameObject.AddComponent<DataCollection>();
            }
        }
        public override void OnConnected()
        {
            base.OnConnected();
            Data.TimesConnected++;
            if (!string.IsNullOrEmpty(Data.CurrentRoom))
            {
                Data.LastRoom = Data.CurrentRoom;
            }
            Data.CurrentRoom = PhotonNetwork.CurrentRoom.Name;
        }
        public override void OnLeftRoom()
        {
            Data.TimesDisconnected++;
        }
    }
    [BepInPlugin("org.zen.room.data","datacollection","0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        void Awake() { DataCollection.InitCollection(); }
    }
}
