using BepInEx;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using GorillaNetworking;
using Modio.Mods;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using ZenMenu.Utillities.MotherShip.V1;

namespace ZenMenu.mods.Safety
{
    internal class SafetyManager : BaseUnityPlugin
    {

        static GameObject gameobject;
        void Awake()
        {
            InitManager();
        }
        public enum Mods
        {
            AnitBan_Mock,
            AntiBan_MotherShip,
            AntiReport,
            AntiModerator,
            PhotonSpoof,
            PlayerSpoof,
            AntiCrash,
            FlushPlayerCredentials,
            FlushCache,
            FlushRPCS,
            CleanTracesOnGameClose,
        }
        public static void EnableMod(Mods mod)
        {
            switch (mod)
            {
                case Mods.AnitBan_Mock:
                    AntiBan_Mock = !AntiBan_Mock;
                    break;
                case Mods.AntiBan_MotherShip:
                    AntiBan_Motheership = !AntiBan_Motheership;
                    break;
                case Mods.AntiReport:
                    AntiReport = !AntiReport;
                    break;
                case Mods.AntiModerator:
                    AntiModerator = !AntiModerator;
                    break;
                case Mods.PhotonSpoof:
                    if (!PhotonNetwork.IsConnected) return;
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
                    PhotonNetwork.LocalPlayer.CustomProperties.Clear();
                    PhotonNetwork.LoadOrCreateSettings(true);
                    PhotonNetwork.LocalCleanPhotonView(VRRig.LocalRig.gameObject.GetPhotonView());
                    PhotonNetwork.NetworkingClient.AuthMode = AuthModeOption.AuthOnceWss;
                    PhotonNetwork.NetworkingClient.AuthValues.SetAuthPostData("Ax-0^");
                    if (PhotonNetwork.NetworkClientState == ClientState.Authenticated)
                    {
                        PhotonNetwork.NetworkStatisticsReset();
                        PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                        PhotonNetwork.LogLevel = PunLogLevel.ErrorsOnly;
                        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = -1;
                    }
                    if (PhotonNetwork.IsMasterClient)
                    {
                        foreach (var p in PhotonNetwork.PlayerListOthers)
                        {
                            PhotonNetwork.CloseConnection(p);
                            PhotonNetwork.QuickResends = int.MinValue;
                            PhotonNetwork.RaiseEvent(200, null, new RaiseEventOptions
                            {
                                CachingOption = EventCaching.DoNotCache,
                                SequenceChannel = 0,
                                Flags = new WebFlags(0),
                                InterestGroup = (byte)PhotonNetwork.PlayerListOthers.Length,
                                Receivers = ReceiverGroup.Others,
                                TargetActors = PhotonNetwork.PlayerListOthers.Where(x => x.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber).Select(x => x.ActorNumber).ToArray()
                            }, new SendOptions
                            {
                                DeliveryMode = DeliveryMode.Unreliable,
                                Channel = 1,
                                Encrypt = true,
                                Reliability = false
                            });
                            PhotonNetworkController.Instance.ForceSave();
                        }
                    }
                    foreach (var s in NetworkRunner.Instances)
                    {
                        foreach (var p in s.ActivePlayers)
                        {
                            var p_ = PlayerRef.FromEncoded(p.RawEncoded);
                            typeof(PlayerRef).GetField("PlayerId").SetValue(p_, int.Parse(Guid.NewGuid().ToString()));
                        }
                        typeof(NetworkRunner).GetField("UserId", BindingFlags.Public | BindingFlags.Instance).SetValue(s, Guid.NewGuid().ToString());
                        int playeractor = (int)typeof(NetworkRunner).GetField("Simulation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s).GetType().GetMethod("GetPlayerActorId", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(typeof(NetworkRunner).GetField("Simulation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s), new object[] { s.LocalPlayer });
                        typeof(NetworkRunner).GetField("_cloudServices", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s).GetType().GetMethod("SendChangeMasterClient", BindingFlags.Public | BindingFlags.Instance).Invoke(typeof(NetworkRunner).GetField("_cloudServices", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s).GetType(), new object[] { playeractor });
                    }
                    break;
                case Mods.PlayerSpoof:
                    break;
                case Mods.AntiCrash:
                    CrashPatch.Apply = !CrashPatch.Apply;
                    break;
                case Mods.FlushPlayerCredentials:
                    MothershipAnchor.SupressCheck(PlayFabAuthenticator.instance.mothershipAuthenticator);
                    break;
                case Mods.FlushCache:
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                    break;
                case Mods.FlushRPCS:
                    PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
                    PhotonNetwork.OpCleanRpcBuffer(PhotonNetwork.LocalPlayer.Get<PhotonView>());
                    PhotonNetwork.OpCleanRpcBuffer(GorillaTagger.Instance.offlineVRRig.Get<VRRig>().Creator.Get<PhotonView>());
                    PhotonNetwork.RemoveBufferedRPCs(PhotonNetwork.GetPhotonView(1).ViewID, "SendRPC", new int[] { int.MinValue, int.MaxValue });
                    PhotonNetwork.OpRemoveCompleteCache();
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
                    PhotonNetwork.OpCleanRpcBuffer(PhotonNetwork.GetPhotonView(1));
                    PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                    PhotonNetwork.RemoveRPCsInGroup(int.MaxValue);
                    PhotonNetwork.SendAllOutgoingCommands();
                    break;
                case Mods.CleanTracesOnGameClose:
                    CleanTraces = !CleanTraces;
                    break;
            }
        }
        public static bool InitManager()
        {
            if (!GameObject.Find("SafetyModManager(@Liex)"))
                gameobject = new GameObject("SafetyModManager(@Liex)").AddComponent<SafetyManager>().gameObject;
            else
                return true;
            return false;
        }

        GameObject Zone;

        static bool AntiBan_Mock;
        static bool AntiBan_Motheership;
        static bool AntiReport;
        static bool AntiModerator;
        static bool CleanTraces;
        void Update()
        {
            if (AntiBan_Mock)
            {
                if (PhotonNetwork.IsConnected)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // 
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, "JOIN", new Photon.Realtime.RaiseEventOptions
                        {
                            SequenceChannel = 0,
                            CachingOption = Photon.Realtime.EventCaching.DoNotCache,
                            Flags = new Photon.Realtime.WebFlags(3),
                            InterestGroup = 1,
                            Receivers = Photon.Realtime.ReceiverGroup.All,
                            TargetActors = PhotonNetwork.PlayerListOthers.Select(x => x.ActorNumber).ToArray(),

                        }, new ExitGames.Client.Photon.SendOptions
                        {
                            Channel = 0,
                            DeliveryMode = ExitGames.Client.Photon.DeliveryMode.Reliable,
                            Reliability = true,
                            Encrypt = true,
                        });
                    }
                    PlayFabAuthenticator.instance.mothershipAuthenticator.MaxMetaLoginAttempts = 1;
                    PlayFabAuthenticator.instance.mothershipAuthenticator.UseConstantTestAccountId = true;
                    PlayFabAuthenticator.instance.mothershipAuthenticator.TestAccountId = Guid.NewGuid().ToString();
                    PlayFabAuthenticator.instance.mothershipAuthenticator.TestNickname = string.Empty;
                    PlayFabAuthenticator.instance.userID = Guid.NewGuid().ToString();
                    PlayFabAuthenticator.instance.BeginLoginFlow();
                    PlayFabAuthenticator.instance.mothershipAuthenticator.BeginLoginFlow();
                    PhotonNetworkController.Instance.ClearDeferredJoin();
                    Dictionary<string, object> Values = new Dictionary<string, object>()
                    {
                        {"Ax-0^","LIEX"},
                        {"UserID",Guid.NewGuid().ToString()},
                        {"NickName", Guid.NewGuid().ToString() }
                    };
                    typeof(PhotonAuthenticator).GetMethod("SetCustomAuthenticationParameters",BindingFlags.Instance | BindingFlags.Public).Invoke(null,new object[] {Values});
                    PlayFabClientAPI.ExecuteCloudScript(
                        new PlayFab.ClientModels.ExecuteCloudScriptRequest
                    {
                        SpecificRevision = -1,
                        GeneratePlayStreamEvent = true,
                        AuthenticationContext = new PlayFabAuthenticationContext
                        {
                            ClientSessionTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
                            EntityToken = Guid.NewGuid().ToString(),
                        },
                    },delegate {  }, delegate { });
                }
            }
            if (AntiBan_Motheership)
            {
                MothershipAnchor.SupressCheck(PlayFabAuthenticator.instance.mothershipAuthenticator);
            }
            if (AntiReport)
            {
                foreach (GorillaPlayerScoreboardLine lines in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (lines.linePlayer == VRRig.LocalRig.Creator)
                    {
                        if (Zone == null)
                        {
                            Zone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Zone.transform.position = lines.reportButton.transform.position;
                            Zone.transform.localScale = new Vector3(.5f, .5f, .5f);
                        }
                        else
                        {
                            if (!PhotonNetwork.IsConnected)
                            { GameObject.Destroy(Zone); Zone = null; }
                            foreach (var p in patches.VrrigCache.Data.vrrigs)
                                if (Vector3.Distance(p.rightHandTransform.position, Zone.transform.position) >= 0.45f || Vector3.Distance(p.leftHandTransform.position, Zone.transform.position) >= 0.45f)
                                    PhotonNetwork.Disconnect();
                        }
                    }
                }
            }
            else
                Zone = null;
            if (AntiModerator)
            {
                if (patches.VrrigCache.Data.Modertors.Count > 0)
                    PhotonNetwork.Disconnect();
            }
            if (CleanTraces)
            {
                if (!ApplicationQuittingState.IsQuitting) return;
                PhotonNetworkController.Instance?.OnApplicationQuit();
                PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
                PhotonNetwork.OpCleanRpcBuffer(PhotonNetwork.LocalPlayer.Get<PhotonView>());
                PhotonNetwork.OpCleanRpcBuffer(VRRig.LocalRig.gameObject.GetPhotonView());
                PhotonNetwork.OpRemoveCompleteCache();
                PhotonNetwork.OpRemoveCompleteCacheOfPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
                PhotonNetwork.NetworkingClient.RemoveCallbackTarget(PhotonNetworkController.Instance);
                PhotonNetwork.NetworkingClient.RemoveCallbackTarget(GorillaTagger.Instance);
                GorillaTagger.Instance?.Destroy();
                PhotonNetworkController.Instance?.Destroy();
                PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                PhotonNetwork.RemoveRPCsInGroup(int.MaxValue);
                PhotonNetwork.LocalPlayer.CustomProperties = null;
                PhotonNetwork.Disconnect();
                PhotonNetwork.SendAllOutgoingCommands();
                while (PhotonNetwork.IsConnected) { return; }
                PhotonNetwork.LocalCleanPhotonView(VRRig.LocalRig.GetComponent<PhotonView>());
                UnityEngine.Resources.UnloadUnusedAssets();
                PlayFabHttp.ClearAllEvents();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GorillaTagger.Instance.SetTag(UnityTag.Invalid);
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PhotonNetwork),"OnEvent")]
    public class CrashPatch
    {
        public static bool Apply = false;
        private bool Prefix(EventData photonEvent)
        {
            if (photonEvent.Code == 204)
                return Apply;
            if (photonEvent.Code == 51)
                return Apply;
            if (photonEvent.Code == 8)
                return Apply;
            if (photonEvent.Code == 67)
                return Apply;
            return true;
        }
    }
}
