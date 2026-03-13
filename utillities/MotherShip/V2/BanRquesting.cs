using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ZenMenu.Utillities.MotherShip.V2
{
    internal class BanRquesting
    {
        private static string TitleId => typeof(MothershipTitleData).GetField("title_id")?.GetValue(null)?.ToString();
        private static string EnvId => typeof(MothershipTitleData).GetField("env_id")?.GetValue(null)?.ToString();
        private static string ResolveReason(int category) => category switch { 0 => "HateSpeech", 1 => "Cheating", 2 => "Toxicity", _ => "Other" };

        private static MothershipHTTPRequest ExtractHttpRequest(object httpRequestPtr)
        {
            var swigField = httpRequestPtr.GetType().GetField("swigCPtr", BindingFlags.NonPublic | BindingFlags.Instance);
            var handleRef = (HandleRef)swigField.GetValue(httpRequestPtr);
            return (MothershipHTTPRequest)Activator.CreateInstance(typeof(MothershipHTTPRequest), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { handleRef.Handle, true }, null);
        }

        public static void RequestBan(string playerId, int durationMinutes, bool orgWide = false, string metadata = "", int category = 1)
        {
            try
            {
                using CreateBanRequest request = new CreateBanRequest();
                request.player_id = playerId; request.title_id = TitleId; request.env_id = EnvId;
                request.category = category; request.reason = ResolveReason(category);
                request.duration_minutes = durationMinutes; request.org_wide = orgWide; request.metadata = metadata;
                MothershipHttpRunner.instance.SendRequest(null, ExtractHttpRequest(request.ToHttpRequest()), _ => { });
            }
            catch { }
        }
        public static void RequestCancleBan(string playerId, int category)
        {
            try
            {
                using CreateBanRequest request = new CreateBanRequest();
                request.player_id = playerId; request.title_id = TitleId; request.env_id = EnvId;
                request.category = category; request.reason = ResolveReason(category);
                MothershipHttpRunner.instance.SendRequest(null, ExtractHttpRequest(request.ToHttpRequest()), response =>
                {
                    try { string banId = FetchBanId(response.Body); if (!string.IsNullOrEmpty(banId)) CancelBan(banId); }
                    catch { }
                });
            }
            catch { }
        }
        public static void CancelBan(string banId)
        {
            try
            {
                using GetBanRequest request = new GetBanRequest();
                request.ban_id = banId; request.title_id = TitleId; request.env_id = EnvId;
                MothershipHttpRunner.instance.SendRequest(null, ExtractHttpRequest(request.ToHttpRequest()), _ => { });
            }
            catch { }
        }

        [Serializable]
        private class BanResponse { public string ban_id; }

        private static string FetchBanId(string json)
        {
            try { return JsonUtility.FromJson<BanResponse>(json)?.ban_id; }
            catch { return null; }
        }
    }
}