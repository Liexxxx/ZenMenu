using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static ZenMenu_.utillities.Proxy.Data;

public static class ZenProxy
{
    public static void SetLogTarget(TextMeshPro tmp)
    {
        _tmp = tmp;
        _uiCtx = SynchronizationContext.Current;
        _tmp.text = "";
        _tmp.richText = true;
    }

    private const int MAX_LINES = 5;

    private static void Log(string msg, string hex)
    {
        if (_tmp == null) return;
        string line = $"<color=#{hex}>[{DateTime.Now:HH:mm:ss.fff}] {msg}</color>\n";
        if (_uiCtx != null) _uiCtx.Post(_ => AppendTrimmed(line), null);
        else AppendTrimmed(line);
    }

    private static void AppendTrimmed(string line)
    {
        _tmp.text += line;
        string[] lines = _tmp.text.Split('\n');
        if (lines.Length > MAX_LINES + 1)
            _tmp.text = string.Join("\n", lines, lines.Length - MAX_LINES - 1, MAX_LINES + 1);
    }

    private static void LogInfo(string m) => Log(m, "00FF00");
    private static void LogSpoof(string m) => Log(m, "FFFF00");
    private static void LogData(string m) => Log(m, "1E90FF");
    private static void LogError(string m) => Log(m, "FF4500");
    private static void LogUdp(string m) => Log(m, "FFD700");

    public static Task InitProxy() => InitProxy(8080);

    public static async Task InitProxy(int port)
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            LogInfo("Stopping previous instance...");
            _cts.Cancel();
            try { _listener?.Stop(); } catch { }
            await Task.Delay(600);
        }
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        try { _listener.Start(); }
        catch (SocketException ex) { LogError($"Port {port} in use: {ex.Message}"); return; }
        LogInfo($"Zen_ Proxy started on {GetLocalIP()}:{port}");
        LogInfo("Cipher : AES-256-CBC + HMAC-SHA256");
        LogInfo("Spoof  : Gateway / Router / STUN / Speedtest / IP leak");
        LogInfo($"Set device proxy -> {GetLocalIP()}:{port}");
        _ = Task.Run(() => StartRawUdpSniffer(GetLocalIP(), _cts.Token));
        _ = Task.Run(() => StartUdpPortListeners(_cts.Token));
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();
                if (_cts.Token.IsCancellationRequested) break;
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        catch { }
        finally { _listener.Stop(); LogInfo("Proxy stopped."); }
    }

    public static void StopProxy()
    {
        _cts?.Cancel();
        try { _listener?.Stop(); } catch { }
    }

    private static void StartRawUdpSniffer(string localIP, CancellationToken token)
    {
        Socket raw = null;
        try
        {
            raw = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            raw.Bind(new IPEndPoint(IPAddress.Parse(localIP), 0));
            raw.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            raw.IOControl(unchecked((int)0x98000001), new byte[] { 1, 0, 0, 0 }, new byte[4]);
            LogUdp($"UDP raw sniffer active on {localIP}");
            var buf = new byte[65535];
            raw.ReceiveTimeout = 1000;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (raw.Receive(buf) < 20 || buf[9] != 17) continue;
                    if (raw.Receive(buf) < (buf[0] & 0x0F) * 4 + 8) continue;
                    bool isNew;
                    lock (_udpLock)
                    {
                        isNew = _seenUdpFlows.Add($"{$"{buf[12]}.{buf[13]}.{buf[14]}.{buf[15]}"}:{(buf[(buf[0] & 0x0F) * 4] << 8) | buf[(buf[0] & 0x0F) * 4 + 1]}->{$"{buf[16]}.{buf[17]}.{buf[18]}.{buf[19]}"}:{(buf[(buf[0] & 0x0F) * 4 + 2] << 8) | buf[(buf[0] & 0x0F) * 4 + 3]}");
                        if (_seenUdpFlows.Count > 500) _seenUdpFlows.Clear();
                    }
                    if (!isNew || ($"{buf[12]}.{buf[13]}.{buf[14]}.{buf[15]}" != localIP && $"{buf[16]}.{buf[17]}.{buf[18]}.{buf[19]}" != localIP)) continue;
                    string label = ClassifyUdpFlow(localIP, $"{buf[12]}.{buf[13]}.{buf[14]}.{buf[15]}", $"{buf[16]}.{buf[17]}.{buf[18]}.{buf[19]}", (buf[(buf[0] & 0x0F) * 4] << 8) | buf[(buf[0] & 0x0F) * 4 + 1], (buf[(buf[0] & 0x0F) * 4 + 2] << 8) | buf[(buf[0] & 0x0F) * 4 + 3]);
                    string dir = $"{buf[12]}.{buf[13]}.{buf[14]}.{buf[15]}" == localIP ? "^ OUT" : "v IN ";
                    LogUdp($"UDP {dir}  {label}  {$"{buf[12]}.{buf[13]}.{buf[14]}.{buf[15]}"}:{(buf[(buf[0] & 0x0F) * 4] << 8) | buf[(buf[0] & 0x0F) * 4 + 1]} -> {$"{buf[16]}.{buf[17]}.{buf[18]}.{buf[19]}"}:{(buf[(buf[0] & 0x0F) * 4 + 2] << 8) | buf[(buf[0] & 0x0F) * 4 + 3]}  {(buf[(buf[0] & 0x0F) * 4 + 4] << 8) | buf[(buf[0] & 0x0F) * 4 + 5]}B");
                }
                catch (SocketException) { }
            }
        }
        catch (SocketException ex)
        {
            LogError($"Raw UDP sniffer failed (needs Admin): {ex.Message}");
            LogUdp("Run as Administrator to enable UDP sniffing.");
        }
        catch (Exception ex) { LogError($"UDP sniffer: {ex.Message}"); }
        finally { try { raw?.Close(); } catch { } }
    }

    private static void StartUdpPortListeners(CancellationToken token)
    {
        foreach (int p in new[] { 50000, 50001, 50002, 50003, 50004, 50005 })
            _ = Task.Run(() => ListenUdpPort(p, "DISCORD VOICE UDP", "9370DB", token));
        foreach (int p in new[] { 3478, 3479, 5349 })
            _ = Task.Run(() => ListenUdpPort(p, "STUN/TURN UDP", "FFFF00", token));
    }

    private static async Task ListenUdpPort(int port, string label, string hex, CancellationToken token)
    {
        UdpClient udp = null;
        try
        {
            udp = new UdpClient(port);
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    udp.Client.ReceiveTimeout = 1000;
                    var remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udp.Receive(ref remote);
                    bool isNew;
                    lock (_udpLock) isNew = _seenUdpFlows.Add($"udp:{remote.Address}:{port}");
                    if (isNew) Log($"{label}  from={remote.Address}  port={port}  {data.Length}B", hex);
                }
                catch (SocketException) { }
            }
        }
        catch { }
        finally { try { udp?.Close(); } catch { } }
    }

    private static string ClassifyUdpFlow(string localIP, string srcIp, string dstIp, int srcPort, int dstPort)
    {
        string remoteIp = srcIp == localIP ? dstIp : srcIp;
        int remotePort = srcIp == localIP ? dstPort : srcPort;
        switch (remotePort)
        {
            case 50000: case 50001: case 50002: return "DISCORD VOICE";
            case 3478: return "STUN/TURN";
            case 3479: return "STUN/TURN + ZOOM";
            case 5349: return "STUN/TURN TLS";
            case 8801: case 8802: return "ZOOM MEDIA";
            case 19302: case 19303: case 19304: return "GOOGLE STUN";
            case 9000: case 9001: case 9002: return "DISCORD RTP";
            case 53: return "DNS QUERY";
            case 123: return "NTP SYNC";
            case 4500: return "IPSEC NAT-T";
            case 500: return "IPSEC IKE";
            case 1194: return "OPENVPN UDP";
            case 51820: return "WIREGUARD";
            case 5353: return "mDNS";
            case 1900: return "SSDP/UPNP";
        }
        if (srcPort == 50000 || srcPort == 50001 || srcPort == 50002) return "DISCORD VOICE";
        if (remoteIp.StartsWith("162.159.")) return "DISCORD (Cloudflare)";
        if (remoteIp.StartsWith("66.22.")) return "DISCORD";
        if (remoteIp.StartsWith("185.157.")) return "DISCORD";
        if (remoteIp.StartsWith("64.125.")) return "ZOOM";
        if (remoteIp.StartsWith("209.9.")) return "ZOOM";
        if (remoteIp.StartsWith("8.8.")) return "GOOGLE DNS";
        if (remoteIp.StartsWith("1.1.1.")) return "CLOUDFLARE DNS";
        if (remoteIp.StartsWith("152.195.")) return "NETFLIX CDN";
        if (remoteIp.StartsWith("104.16.")) return "CLOUDFLARE CDN";
        if (remotePort >= 50000 && remotePort <= 50099) return "DISCORD VOICE";
        if (remotePort >= 8801 && remotePort <= 8802) return "ZOOM";
        if (remotePort >= 49152 && remotePort <= 65535) return "UDP HIGH PORT";
        return "UDP";
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        try
        {
            client.NoDelay = true;
            var clientStream = client.GetStream();
            var headerBuf = new byte[8192];
            int headerRead = await clientStream.ReadAsync(headerBuf, 0, headerBuf.Length);
            if (headerRead == 0) { client.Close(); return; }
            string rawHeader = Encoding.ASCII.GetString(headerBuf, 0, headerRead);
            string[] lines = rawHeader.Split('\n');
            if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])) { client.Close(); return; }
            string[] parts = lines[0].Trim().Split(' ');
            if (parts.Length < 2) { client.Close(); return; }
            string method = parts[0], target = parts[1];
            string hostHeader = "", contentType = "", userAgent = "", referer = "";
            long contentLength = 0;
            foreach (string line in lines)
            {
                if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase)) hostHeader = line[5..].Trim().Split(':')[0].ToLower();
                else if (line.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase)) contentType = line[13..].Trim().ToLower();
                else if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) long.TryParse(line[15..].Trim(), out contentLength);
                else if (line.StartsWith("User-Agent:", StringComparison.OrdinalIgnoreCase)) userAgent = line[11..].Trim();
                else if (line.StartsWith("Referer:", StringComparison.OrdinalIgnoreCase)) referer = line[8..].Trim();
            }
            string path = target.StartsWith("http") ? new Uri(target).PathAndQuery : target;
            if (IsSpoofTarget(hostHeader, target, rawHeader))
            {
                LogSpoof($"SPOOFED  host={hostHeader}  from={clientIP}");
                await SendSpoofResponse(clientStream, hostHeader);
                client.Close(); return;
            }
            (string actLabel, string actColor, bool isUpload, bool isDownload, bool anyUrl) = DetectActivity(hostHeader, target);
            bool hasActivity = actLabel != null;
            if (hasActivity)
            {
                string sizeStr = contentLength > 0 ? $"  size={FormatBytes(contentLength)}" : "";
                if (isUpload && (contentLength > 0 || contentType.Contains("multipart") || contentType.Contains("octet")))
                    Log($"UPLOAD  {actLabel}  {clientIP} -> {hostHeader}{sizeStr}", actColor);
                else
                    Log($"{actLabel}  {clientIP} <-> {hostHeader}{path}", actColor);
            }
            else
            {
                string reqLabel = ClassifyWebRequest(method, hostHeader, path, contentType, contentLength, out string reqColor);
                Log($"{reqLabel}  [{clientIP}]", reqColor);
                if (userAgent != "" && !userAgent.Contains("Mozilla"))
                    Log($"  UA: {userAgent[..Math.Min(80, userAgent.Length)]}", "696969");
                if (referer != "")
                    Log($"  REF: {referer[..Math.Min(80, referer.Length)]}", "696969");
            }
            if (method == "CONNECT")
            {
                await clientStream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n"));
                await HandleTunnelAsync(clientStream, target, clientIP, hostHeader, hasActivity, actLabel, actColor);
            }
            else
            {
                await HandleHttpAsync(clientStream, headerBuf, headerRead, target, clientIP, hostHeader, hasActivity, actLabel, actColor);
            }
        }
        catch (SocketException) { }
        catch (IOException) { }
        catch (Exception ex) { LogError($"Client [{clientIP}] {ex.Message}"); }
        finally { try { client.Close(); } catch { } }
    }

    private static bool IsSpoofTarget(string host, string url, string rawHeader)
    {
        string h = (host ?? "").ToLower();
        foreach (string blocked in new[]
        {
            "stun.l.google.com","stun1.l.google.com","stun2.l.google.com","stun3.l.google.com","stun4.l.google.com",
            "stun.services.mozilla.com","stun.stunprotocol.org","global.stun.twilio.com","stun.cloudflare.com",
            "api.ipify.org","ifconfig.me","ipinfo.io","checkip.amazonaws.com","icanhazip.com",
            "wtfismyip.com","myexternalip.com","ip-api.com","ipecho.net","httpbin.org",
            "routerlogin.net","routerlogin.com","myrouter.local","router.local","gateway.local",
            "192.168.0.1","192.168.1.1","10.0.0.1","10.0.0.2","10.0.1.1",
            "xfinity.com","comcast.com","comcast.net","xfinityspeed.net","speedtest.xfinity.com",
            "login.xfinity.com","device.xfinity.com","geoip.maxmind.com","db-ip.com","ipregistry.co",
            "speedtest.net","fast.com","speed.cloudflare.com","nperf.com","testmy.net","ookla.com",
            "browserleaks.com","ipleak.net","dnsleaktest.com","whoer.net","vpncheck.org",
        })
            if (h == blocked || h.EndsWith("." + blocked) || h.Contains(blocked)) return true;
        string combined = ((url ?? "") + rawHeader).ToLower();
        foreach (string pattern in new[]
        {
            "/turn","/stun","/ice","/webrtc","/rtc","/gateway","/router","/network-info","/localip",
            "/wan-info","/status.cgi","/api/network","/currentsetting.htm","/setup.cgi",
            "networkdiagnostic","_ah/api/stun","networktraversal.googleapis.com",
            "xfinity","comcast","/isp","/asn","/whoami","/myip","geoip","ipgeo","macaddress","whois",
            "/speedtest","/latency","/upload-test","/download-test","bandwidth-test",
        })
            if (combined.Contains(pattern)) return true;
        if (Regex.IsMatch(h, @"^(192\.168\.|10\.|172\.(1[6-9]|2\d|3[01])\.)")) return true;
        if (Regex.IsMatch(combined, @"(stun|turn|ice.server|webrtc|icecandidate|srflx|relay|gateway|routerinfo)", RegexOptions.IgnoreCase)) return true;
        return false;
    }

    private static async Task SendSpoofResponse(NetworkStream clientStream, string host)
    {
        string fakeJson = $"{{\"status\":\"Zen_\",\"gateway\":\"0.0.0.0\",\"router_ip\":\"zen_.invalid\",\"mac\":\"00:00:ZE:N_:00:00\",\"ssid\":\"Zen_NET_{RandomHex(6)}\",\"error\":\"Zen_ Network Error: Access Denied.\"}}";
        string payload = "Zen_\nZen_ Network Error: Access Denied. Device identifier unavailable. Router information encrypted. Gateway address invalid. Local network topology hidden. System protection activated. User data protected. Connection obfuscated. Unable to retrieve network credentials. Zen_ Security Proxy v3.7.1.\n" + GenerateNoise(512) + "\n" + fakeJson;
        byte[] body = Encoding.UTF8.GetBytes(payload);
        try
        {
            await clientStream.WriteAsync(Encoding.UTF8.GetBytes(
                $"HTTP/1.1 200 OK\r\nContent-Type: application/json; charset=utf-8\r\nContent-Length: {body.Length}\r\nServer: Zen_-Proxy\r\nConnection: close\r\n\r\n"));
            await clientStream.WriteAsync(body);
            await clientStream.FlushAsync();
            LogSpoof($"SPOOF SENT  {host}  {FormatBytes(body.Length)}");
        }
        catch (Exception ex) { LogError($"SendSpoof: {ex.Message}"); }
    }

    private static async Task HandleTunnelAsync(NetworkStream clientStream, string hostPort, string clientIP, string host,
        bool hasActivity, string actLabel, string actColor)
    {
        string[] p = hostPort.Split(':');
        int port = p.Length > 1 ? int.Parse(p[1]) : 443;
        var remote = new TcpClient();
        try
        {
            await remote.ConnectAsync(p[0], port);
            remote.NoDelay = true;
            var remoteStream = remote.GetStream();
            await Task.WhenAny(
                EncryptedRelayAsync(clientStream, remoteStream, $"{clientIP} ^ {p[0]}", true, hasActivity, actLabel, actColor),
                EncryptedRelayAsync(remoteStream, clientStream, $"{p[0]} v {clientIP}", false, hasActivity, actLabel, actColor)
            );
        }
        catch (SocketException ex) { LogError($"Tunnel {p[0]}:{port} -- {ex.Message}"); }
        catch (IOException) { }
        catch (Exception ex) { LogError($"Tunnel: {ex.Message}"); }
        finally { try { remote.Close(); } catch { } }
    }

    private static async Task HandleHttpAsync(NetworkStream clientStream, byte[] buf, int bufLen, string url, string clientIP, string host,
        bool hasActivity, string actLabel, string actColor)
    {
        Uri uri;
        try { uri = new Uri(url.StartsWith("http") ? url : "http://" + url); } catch { return; }
        var remote = new TcpClient();
        try
        {
            await remote.ConnectAsync(uri.Host, uri.Port < 0 ? 80 : uri.Port);
            var remoteStream = remote.GetStream();
            await remoteStream.WriteAsync(buf, 0, bufLen);
            await remoteStream.FlushAsync();
            await EncryptedRelayAsync(remoteStream, clientStream, $"{uri.Host} v {clientIP}", false, hasActivity, actLabel, actColor);
        }
        catch (SocketException ex) { LogError($"HTTP {uri.Host} -- {ex.Message}"); }
        catch (IOException) { }
        catch (Exception ex) { LogError($"HTTP: {ex.Message}"); }
        finally { try { remote.Close(); } catch { } }
    }

    private static async Task EncryptedRelayAsync(Stream src, Stream dst, string label, bool isUpstream,
        bool hasActivity, string actLabel, string actColor, int bufSize = 65536)
    {
        var buf = new byte[bufSize];
        long total = 0;
        bool announcedStart = false;
        try
        {
            while (true)
            {
                int read = await src.ReadAsync(buf, 0, buf.Length);
                if (read == 0) break;
                byte[] pkt = Encrypt(buf, 0, read);
                byte[] lenBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(pkt.Length));
                await dst.WriteAsync(lenBytes, 0, 4);
                await dst.WriteAsync(pkt, 0, pkt.Length);
                await dst.FlushAsync();
                total += read;
                if (!announcedStart && total >= 8192)
                {
                    announcedStart = true;
                    if (hasActivity)
                        Log((isUpstream ? "UPLOAD IN PROGRESS" : "DOWNLOAD IN PROGRESS") + $"  {label}", actColor);
                }
                if (total % (512 * 1024) < (long)read)
                    LogData((isUpstream ? "UP" : "DN") + $" {label}  {FormatBytes(total)}");
            }
            if (total >= 1024)
                Log((isUpstream ? "UPLOAD DONE" : "DOWNLOAD DONE") + $"  {label}  {FormatBytes(total)}",
                    hasActivity ? actColor : (isUpstream ? "FF69B4" : "32CD32"));
        }
        catch (IOException) { }
        catch { }
    }

    private static (string label, string color, bool isUpload, bool isDownload, bool anyUrl)
        DetectActivity(string host, string url)
    {
        string h = (host ?? "").ToLower();
        string u = (url ?? "").ToLower();
        (string[] hp, string[] up, string lbl, string col, bool anyUrl, bool upload, bool download)[] rules =
        {
            (new[]{"discord.com","discord.gg","discordapp.com","discord.media","gateway.discord.gg","cdn.discordapp.com","media.discordapp.net"},
             new[]{"gateway","voice","rtc","api/v"}, "DISCORD", "9370DB", true, false, false),
            (new[]{"cdn.discordapp.com","media.discordapp.net"}, new[]{"/attachments/"},
             "DISCORD DOWNLOAD", "DDA0DD", false, false, true),
            (new[]{"meet.google.com","hangouts.google.com","calls.google.com","meet.jit.si"},
             new[]{"meet","call","join","video"}, "GOOGLE MEET", "00BFFF", true, false, false),
            (new[]{"zoom.us","zoom.com","zoomgov.com"}, new[]{"meeting","join","webinar"},
             "ZOOM", "00BFFF", true, false, false),
            (new[]{"teams.microsoft.com","teams.live.com","skype.com"}, new[]{"meeting","call","voice"},
             "TEAMS/SKYPE", "00BFFF", true, false, false),
            (new[]{"steamcdn-a.akamaihd.net","content.steampowered.com","cdn.cloudflare.steamstatic.com"},
             new[]{"/depot/","/download",".pak",".bin"}, "STEAM DOWNLOAD", "32CD32", false, false, true),
            (new[]{"epicgames.com","dl.epicgames.com"}, new[]{"/download",".zip",".exe"},
             "EPIC DOWNLOAD", "32CD32", false, false, true),
            (new[]{"objects.githubusercontent.com","releases.githubusercontent.com","codeload.github.com"},
             new[]{"/releases/","/archive/",".zip",".exe"}, "GITHUB DOWNLOAD", "32CD32", false, false, true),
            (new[]{"dl.google.com","redirector.gvt1.com","gvt1.com"}, new[]{"/download",".exe",".apk",".zip"},
             "GOOGLE DOWNLOAD", "32CD32", false, false, true),
            (new[]{"software-download.microsoft.com","download.microsoft.com","windowsupdate.com"},
             new[]{"/download",".exe",".msi",".cab"}, "MICROSOFT DOWNLOAD", "32CD32", false, false, true),
            (new[]{"youtube.com","googlevideo.com"}, new[]{"/videoplayback","itag=","range="},
             "YOUTUBE STREAM", "FF0000", false, false, false),
            (new[]{"twitch.tv","live-video.net","twitchsvc.net"}, new[]{".m3u8",".ts","chunked","/live"},
             "TWITCH STREAM", "9370DB", false, false, false),
            (new[]{"spotify.com","scdn.co","audio-ak.spotify.com"}, new[]{"/audio","/media",".ogg",".mp3"},
             "SPOTIFY STREAM", "00FF7F", false, false, false),
            (new[]{"wetransfer.com","transfer.sh","file.io","up.imgur.com"}, new[]{"/upload","/file","/send"},
             "FILE UPLOAD", "FF69B4", false, true, false),
            (new[]{"drive.google.com","www.googleapis.com","graph.microsoft.com","content.dropboxapi.com","onedrive.live.com"},
             new[]{"/upload","?upload"}, "CLOUD UPLOAD", "FF69B4", false, true, false),
            (new[]{"drive.google.com","www.googleapis.com","graph.microsoft.com","content.dropboxapi.com","onedrive.live.com"},
             new[]{"/download","?export"}, "CLOUD DOWNLOAD", "32CD32", false, false, true),
        };
        foreach (var r in rules)
        {
            bool hostMatch = false;
            foreach (string p in r.hp) if (h == p || h.EndsWith("." + p) || h.Contains(p)) { hostMatch = true; break; }
            if (!hostMatch) continue;
            if (r.anyUrl) return (r.lbl, r.col, r.upload, r.download, true);
            foreach (string p in r.up) if (u.Contains(p)) return (r.lbl, r.col, r.upload, r.download, false);
        }
        return (null, null, false, false, false);
    }

    private static string ClassifyWebRequest(string method, string host, string path, string contentType, long contentLength, out string color)
    {
        if (method == "CONNECT")
        { color = "00FFFF"; return "HTTPS  " + host; }
        if (path.ToLower().Contains("/api/") || path.ToLower().Contains("/v1/") || path.ToLower().Contains("/v2/") || path.ToLower().Contains("/v3/") ||
            path.ToLower().Contains("/graphql") || path.ToLower().Contains("/rest/") || path.ToLower().Contains(".json") || path.ToLower().Contains("/ajax"))
        { color = "EE82EE"; return $"API    {method}  {host}{path}"; }
        if (method == "POST" && (contentType.Contains("json") || contentType.Contains("form") || contentType.Contains("xml")))
        { color = "FF69B4"; return $"POST   {host}{path}" + (contentLength > 0 ? $"  {FormatBytes(contentLength)}" : ""); }
        string lowerPath = path.ToLower();
        string ext = lowerPath.LastIndexOf('.') > lowerPath.LastIndexOf('/') && lowerPath.LastIndexOf('.') > 0
            ? lowerPath[lowerPath.LastIndexOf('.')..]
            : "";
        bool isPageExt = ext == ".html" || ext == ".htm" || ext == ".php" || ext == ".asp" || ext == ".aspx" || ext == ".jsp" || ext == "";
        if (isPageExt && !lowerPath.Contains("/static/") && !lowerPath.Contains("/assets/") && !lowerPath.Contains("/cdn/") && (method == "GET" || method == "HEAD"))
        { color = "FFFFFF"; return "WEB    " + host + (path == "/" ? "" : path); }
        if (ext == ".mp4" || ext == ".webm" || ext == ".mp3" || ext == ".ogg" || ext == ".m3u8" || ext == ".ts" || ext == ".aac")
        { color = "FF0000"; return $"MEDIA  {host}{path}"; }
        if (ext == ".js" || ext == ".css" || ext == ".woff" || ext == ".woff2" || ext == ".ttf" || ext == ".svg" || ext == ".ico")
        { color = "646464"; return $"ASSET  {host}{path}"; }
        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".webp" || ext == ".avif")
        { color = "7878B4"; return $"IMG    {host}{path}"; }
        if (ext == ".zip" || ext == ".exe" || ext == ".msi" || ext == ".dmg" || ext == ".apk" || ext == ".deb" || ext == ".tar" || ext == ".gz" || ext == ".iso" || ext == ".bin" || ext == ".pak")
        { color = "32CD32"; return $"FILE   {host}{path}"; }
        color = "696969";
        return $"REQ    {method}  {host}{path}";
    }

    private static byte[] Encrypt(byte[] data, int offset, int length)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
        aes.Key = EncKey; aes.GenerateIV();
        byte[] ct;
        using (var enc = aes.CreateEncryptor())
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
            { cs.Write(data, offset, length); cs.FlushFinalBlock(); }
            ct = ms.ToArray();
        }
        byte[] hmac = ComputeHmac(aes.IV, ct);
        byte[] packet = new byte[16 + 32 + ct.Length];
        Buffer.BlockCopy(aes.IV, 0, packet, 0, 16);
        Buffer.BlockCopy(hmac, 0, packet, 16, 32);
        Buffer.BlockCopy(ct, 0, packet, 16 + 32, ct.Length);
        return packet;
    }

    private static byte[] Decrypt(byte[] packet)
    {
        if (packet.Length < 16 + 32 + 1) throw new CryptographicException("Packet too short.");
        var iv = packet[..16];
        var hmac = packet[16..48];
        var ct = packet[48..];
        if (!CryptographicEquals(hmac, ComputeHmac(iv, ct))) throw new CryptographicException("HMAC mismatch.");
        using var aes = Aes.Create();
        aes.KeySize = 256; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
        aes.Key = EncKey; aes.IV = iv;
        using var dec = aes.CreateDecryptor();
        using var ms = new MemoryStream(ct);
        using var cs = new CryptoStream(ms, dec, CryptoStreamMode.Read);
        using var result = new MemoryStream();
        cs.CopyTo(result);
        return result.ToArray();
    }

    private static byte[] ComputeHmac(byte[] iv, byte[] ct)
    {
        var buf = new byte[iv.Length + ct.Length];
        Buffer.BlockCopy(iv, 0, buf, 0, iv.Length);
        Buffer.BlockCopy(ct, 0, buf, iv.Length, ct.Length);
        using var h = new HMACSHA256(MacKey);
        return h.ComputeHash(buf);
    }

    private static bool CryptographicEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int d = 0;
        for (int i = 0; i < a.Length; i++) d |= a[i] ^ b[i];
        return d == 0;
    }

    private static string FormatBytes(long b)
    {
        if (b >= 1_073_741_824) return $"{b / 1_073_741_824.0:0.00} GB";
        if (b >= 1_048_576) return $"{b / 1_048_576.0:0.00} MB";
        if (b >= 1_024) return $"{b / 1_024.0:0.00} KB";
        return $"{b} B";
    }

    private static string GenerateNoise(int chars)
    {
        var sb = new StringBuilder(chars);
        var buf = new byte[2];
        using var rng = RandomNumberGenerator.Create();
        for (int i = 0; i < chars; i++)
        {
            rng.GetBytes(buf);
            sb.Append((char)(0x4E00 + (((buf[0] << 8) | buf[1]) % (0x9FFF - 0x4E00))));
        }
        return sb.ToString();
    }

    private static string RandomHex(int bytes)
    {
        var b = new byte[bytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(b);
        return BitConverter.ToString(b).Replace("-", "").ToLower();
    }

    private static string GetLocalIP()
    {
        if (_cachedLocalIP != null) return _cachedLocalIP;
        using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        s.Connect("8.8.8.8", 80);
        return _cachedLocalIP = ((IPEndPoint)s.LocalEndPoint).Address.ToString();
    }
}