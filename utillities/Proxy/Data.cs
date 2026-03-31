using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TMPro;

namespace ZenMenu_.utillities.Proxy
{
    internal class Data
    {
        public static readonly byte[] EncKey = DeriveKey("Zen_18789497982_LIEX", "ProxySalt_Enc_v1");
        public  static readonly byte[] MacKey = DeriveKey("Zen_18789497982_LIEX_MAC", "ProxySalt_Mac_v1");
        private static byte[] DeriveKey(string pass, string salt)
        {
            using var k = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(pass), Encoding.UTF8.GetBytes(salt), 100000);
            return k.GetBytes(32);
        }

        public static TextMeshPro _tmp;
        public static SynchronizationContext _uiCtx;
        public static CancellationTokenSource _cts;
        public static TcpListener _listener;
        public static readonly System.Collections.Generic.HashSet<string> _seenUdpFlows = new System.Collections.Generic.HashSet<string>();
        public static readonly object _udpLock = new object();
        public static string _cachedLocalIP;
    }
}
