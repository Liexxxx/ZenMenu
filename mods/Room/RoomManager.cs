using Backtrace.Unity.Model.Breadcrumbs;
using BepInEx;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZenMenu.mods.Safety;

namespace ZenMenu_.mods.Room
{
    [BepInEx.BepInPlugin("org.zen.room","roommanager","0.0.0")]
    internal class RoomManager : BaseUnityPlugin
    {
        void Awake() { InitManager(); }
        public enum Mods
        {
            Disconnect,
            Recconect,
            JoinRandom,
            JoinLastRoom,
            CreatePublic,
            CreateLocked,
            CreateModded,
            ClearRoomCache,
        }
        public static void InitManager()
        {
            if (GameObject.Find("RoomModManager(@Liex)") == null)
            {
                GameObject obj = new GameObject("RoomModManager(@Liex)");
                obj.AddComponent<SafetyManager>();
            }
        }
        public static void EnableMod(Mods Mod)
        {
            switch (Mod)
            {
                case Mods.Disconnect:
                    PhotonNetwork.Disconnect();
                    break;
                case Mods.Recconect:
                    if (PhotonNetwork.IsConnected)
                        PhotonNetwork.Disconnect();
                    else
                        if (!string.IsNullOrEmpty(Data.CurrentRoom))
                        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(Data.CurrentRoom,JoinType.Solo);
                    break;
                case Mods.JoinRandom:
                    if (PhotonNetwork.IsConnected)
                        PhotonNetwork.Disconnect();
                    else
                        PhotonNetworkController.Instance.AttemptToJoinPublicRoom(PhotonNetworkController.Instance.currentJoinTrigger);
                    break;
                case Mods.JoinLastRoom:
                    if (PhotonNetwork.IsConnected)
                        PhotonNetwork.Disconnect();
                    else
                        if (!string.IsNullOrEmpty(Data.LastRoom))
                            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(Data.LastRoom, JoinType.Solo);
                    break;
                case Mods.CreatePublic:
                    if (PhotonNetwork.IsConnected)
                        break;
                    else
                        NetworkSystem.Instance.ConnectToRoom(CreateRandomRoomName(),RoomConfig.AnyPublicConfig());
                   break;
                case Mods.CreateLocked:
                    if (PhotonNetwork.IsConnected)
                        break;
                    else
                        NetworkSystem.Instance.ConnectToRoom(CreateRandomRoomName(), new RoomConfig
                        {
                            createIfMissing = true,
                            isJoinable = false,
                            isPublic = true,
                            MaxPlayers = 1,
                        });
                    break;
                case Mods.CreateModded:
                    if (PhotonNetwork.IsConnected)
                        break;
                    else
                        NetworkSystem.Instance.ConnectToRoom(CreateRandomRoomName(), new RoomConfig
                        {
                            createIfMissing = true,
                            isJoinable = true,
                            isPublic = true,
                            MaxPlayers = 10,
                            CustomProps = new ExitGames.Client.Photon.Hashtable
                            {
                                { "gameMode", PhotonNetworkController.Instance.currentJoinTrigger.GetFullDesiredGameModeString() },
                                { "platform", (string)typeof(PhotonNetworkController).GetField("platformTag").GetValue(PhotonNetworkController.Instance)},
                                {
                                    "queueName",
                                    GorillaComputer.instance.currentQueue
                                },
                                {
                                    "language",
                                    LocalisationManager.CurrentLanguage.ToString()
                                },
                                {
                                    "fan_club",
                                    SubscriptionManager.IsLocalSubscribed() ? "true" : "false"
                                },
                                {
                                    "Modded",
                                    "true"
                                }
                            }
                        });
                    break;
                case Mods.ClearRoomCache:
                    PhotonNetwork.RaiseEvent(200, new object[] {  }, new Photon.Realtime.RaiseEventOptions
                    {
                        CachingOption = Photon.Realtime.EventCaching.RemoveFromRoomCache,
                        Flags = new Photon.Realtime.WebFlags(1),
                        Receivers = Photon.Realtime.ReceiverGroup.All,
                    }, new ExitGames.Client.Photon.SendOptions
                    {
                        DeliveryMode = ExitGames.Client.Photon.DeliveryMode.UnreliableUnsequenced,
                        Encrypt = false,
                        Reliability = false,
                    });
                    break;
            }
        }
        public static string CreateRandomRoomName()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char[] roomName = new char[4];
            for (int i = 0; i < roomName.Length; i++)
            {
                roomName[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
            }
            return new string(roomName);
        }
        void Update()
        {

        }
    }
}
