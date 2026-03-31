using Constants;
using GorillaNetworking;
using GorillaTagScripts;
using MonoMod.RuntimeDetour;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace ZenMenu.Patches
{
    internal class _20PlayerLobbies
    {
        [HarmonyLib.HarmonyPatch(typeof(PhotonNetworkController), "AttemptToJoinPublicRoomAsync")]
        public class SubuscriptionVerificationBypass
        {
            public static bool Enabled = false;

            public static bool Prefix(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType, List<(string, string)> additionalCustomProperties, bool filterSubscribed)
            {
                if (!Enabled) return true;

                AttemptToJoinPublicRoomAsync(triggeredTrigger, roomJoinType, additionalCustomProperties, filterSubscribed);
                return false;
            }

            private static async void AttemptToJoinPublicRoomAsync(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType, List<(string, string)> additionalCustomProperties, bool filterSubscribed)
            {
                string desiredGameMode = triggeredTrigger.GetFullDesiredGameModeString();
                PhotonNetworkController.Instance.currentJoinTrigger = triggeredTrigger;

                if (PlayFabClientAPI.IsClientLoggedIn())
                {
                    PhotonNetworkController.Instance.playFabAuthenticator.SetDisplayName(NetworkSystem.Instance.GetMyNickName());
                }

                RoomConfig roomConfig = RoomConfig.AnyPublicConfig();

                string platformTag = typeof(PhotonNetworkController)
                    .GetField("platformTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(PhotonNetworkController.Instance) as string;

                ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable
                {
                    { "gameMode", desiredGameMode },
                    { "platform", platformTag },
                    { "queueName", GorillaComputer.instance.currentQueue },
                    { "language", LocalisationManager.CurrentLanguage.ToString() },
                    { "fan_club", "true" }
                };

                if (additionalCustomProperties != null)
                {
                    foreach (var (key, value) in additionalCustomProperties)
                    {
                        if (!hashtable.ContainsKey(key))
                            hashtable.Add(key, value);
                    }
                }

                roomConfig.CustomProps = hashtable;
                roomConfig.MaxPlayers = PhotonNetworkController.Instance.currentJoinTrigger.GetRoomSize(true);

                Debug.Log($"AttemptToJoinPublicRoom: MaxPlayers: {roomConfig.MaxPlayers} | FanClub bypass active");

                await NetworkSystem.Instance.ConnectToRoom(null, roomConfig);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(SubscriptionManager), nameof(SubscriptionManager.IsLocalSubscribed))]
        public class ForceSubscription
        {
            public static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }
    }
}