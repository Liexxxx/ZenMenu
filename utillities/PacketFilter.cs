using BepInEx;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ZenMenu_.utillities
{
    internal class PacketFilter
    {
        public static event Action<string, string> OnValidPacket;
        public static event Action<string, string> OnMalformedPacket;
        private static int maxPayloadSize = 10000;
        private static bool requireJsonFormat = true;
        public static void Configure(int maxSize, bool requireJson)
        {
            maxPayloadSize = maxSize;
            requireJsonFormat = requireJson;
        }
        public static void GetUserData(string playFabId, Action<GetUserDataResult> callback = null)
        {
            var request = new PlayFab.ClientModels.GetUserDataRequest
            {
                PlayFabId = playFabId
            };

            PlayFabClientAPI.GetUserData(request,
                result =>
                {
                    Process(result);
                    callback?.Invoke(result);
                },
                error =>
                {
                    OnMalformedPacket?.Invoke("REQUEST_ERROR", error.GenerateErrorReport());
                });
        }

        private static void Process(GetUserDataResult result)
        {
            if (result?.Data == null)
                return;

            foreach (KeyValuePair<string, UserDataRecord> pair in result.Data)
            {
                string key = pair.Key;
                string value = pair.Value?.Value;

                if (Validate(value))
                {
                    Cache(key, value);
                    OnValidPacket?.Invoke(key, value);
                }
                else
                {
                    OnMalformedPacket?.Invoke(key, value);
                }
            }
        }

        private static bool Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (value.Length > maxPayloadSize)
                return false;

            if (requireJsonFormat && !IsJson(value))
                return false;

            return true;
        }

        private static bool IsJson(string str)
        {
            str = str.Trim();
            return (str.StartsWith("{") && str.EndsWith("}")) ||
                   (str.StartsWith("[") && str.EndsWith("]"));
        }

        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>();

        private static void Cache(string key, string value)
        {
            cache[key] = value;
        }

        public static string GetCached(string key)
        {
            return cache.TryGetValue(key, out var val) ? val : null;
        }
    }
    [BepInPlugin("org.zen.packetfilter", "Packet Filter Plugin", "1.0.0")]
    public class PacketFilterPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            PacketFilter.Configure(10000, true);
            PacketFilter.OnValidPacket += OnValidPacket;
            PacketFilter.OnMalformedPacket += OnMalformedPacket;
        }
        private void OnDestroy()
        {
            PacketFilter.OnValidPacket -= OnValidPacket;
            PacketFilter.OnMalformedPacket -= OnMalformedPacket;
        }
        private void OnValidPacket(string key, string value)
        {

        }
        private void OnMalformedPacket(string key, string value)
        {
            Logger.LogWarning($"[BLOCKED] {key}");
        }
        public void FetchData(string playFabId)
        {
            PacketFilter.GetUserData(playFabId);
        }
    }
}
