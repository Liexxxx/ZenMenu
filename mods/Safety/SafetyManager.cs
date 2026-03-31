using BepInEx;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using GorillaNetworking;
using HarmonyLib;
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
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using ZenMenu.Menu;
using ZenMenu.Utillities.MotherShip.V1;

namespace ZenMenu.mods.Safety
{
    [BepInPlugin("org.zen.safety","safetymanager","0.0.0")]
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
            RpcProtection,
            Proxy
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
                    if (!PhotonNetwork.IsConnected) break;
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
                case Mods.RpcProtection:
                    RpcProtection = !RpcProtection;
                    break;
                case Mods.Proxy:
                    Proxy = !Proxy;
                    if (Proxy)
                    {
                        ZenProxy.SetLogTarget(GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData").GetComponent<TextMeshPro>());
                        _ = Task.Run(() => ZenProxy.InitProxy(8080));
                    }
                    else
                        ZenProxy.StopProxy();
                    break;
            }
        }
        public static void InitManager()
        {
            if (GameObject.Find("SafetyModManager(@Liex)") == null)
            {
                GameObject obj = new GameObject("SafetyModManager(@Liex)");
                obj.AddComponent<SafetyManager>();
                obj.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        public static GameObject zone;
        public static bool SentReport;
        public static bool zoneInitialized = false;

        static bool AntiBan_Mock;
        static bool AntiBan_Motheership;
        static bool AntiReport;
        static bool AntiModerator;
        static bool CleanTraces;
        static bool RpcProtection;
        static bool Proxy;
        float col = 0f;
        bool NeededManualRPCProt;
        void Update()
        {
            if (AntiBan_Mock)
            {
                if (PhotonNetwork.IsConnected)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
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
                foreach (var sb in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (sb.linePlayer != GorillaTagger.Instance.offlineVRRig.Creator)
                        continue;
                    if (!zoneInitialized)
                    {
                        zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        zone.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        UnityEngine.Object.Destroy(zone.GetComponent<Collider>());
                        zone.GetComponent<Renderer>().enabled = false;
                        zoneInitialized = true;
                    }
                    zone.transform.position = sb.reportButton.transform.position;
                    bool reportTriggered = false;
                    foreach (VRRig player in patches.VrrigCache.Data.vrrigs)
                    {
                        if (player == VRRig.LocalRig)
                            continue;
                        float distLeft = Vector3.Distance(player.leftHandTransform.position, zone.transform.position);
                        float distRight = Vector3.Distance(player.rightHandTransform.position, zone.transform.position);
                        if (distLeft < 0.45f || distRight < 0.45f)
                        {
                            if (!SentReport)
                            {
                                PhotonNetwork.Disconnect();
                                if (!Main.GetModule("Safety", "RPCProtection").Enabled)
                                {
                                    EnableMod(Mods.RpcProtection);
                                    NeededManualRPCProt = true;
                                }
                                player.transform.position = Vector3.negativeInfinity;
                                SentReport = true;
                                reportTriggered = true;
                            }
                            break;
                        }
                    }

                    if (SentReport && !PhotonNetwork.IsConnected)
                    {
                        SentReport = false;
                        if (zone != null)
                        {
                            UnityEngine.Object.Destroy(zone);
                            zone = null;
                            zoneInitialized = false;
                        }
                    }

                    if (reportTriggered)
                        break;
                }
            }
            else { zone = null; if (NeededManualRPCProt)EnableMod(Mods.RpcProtection); }
            if (RpcProtection)
            {
                MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                MonkeAgent.instance.rpcCallLimit = int.MaxValue;
                MonkeAgent.instance.logErrorMax = int.MaxValue;
                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;
                if (Time.time <= col) return;
                col = Time.time + 0.47f;
                try
                {
                    MonkeAgent.instance.OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
                    Traverse.Create(typeof(PhotonNetwork)).Property("ResentReliableCommands").SetValue(0);
                    PhotonNetwork.NetworkingClient.Service();
                    PhotonNetwork.NetworkingClient.OpChangeGroups(null, new byte[] { 1, 2, 3, 4 });
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsReset();
                    var sys = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Assembly-CSharp").GetType("RoomSystem")?.GetMethod("OnPlayerLeftRoom", BindingFlags.NonPublic | BindingFlags.Instance);
                    sys?.Invoke(null, new object[] { NetworkSystem.Instance.LocalPlayer });
                    new NetSystemState().Equals(NetSystemState.Connecting);
                    new PeerStateValue().Equals(PeerStateValue.Connected);
                    typeof(PhotonNetwork).GetMethod("RunViewUpdate", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
                    PhotonNetwork.SendAllOutgoingCommands();
                }
                catch { }
                typeof(MonkeAgent)
                    .GetMethod("RefreshRPCs", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(MonkeAgent.instance, null);
            }
            else
                col = 0f;
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
