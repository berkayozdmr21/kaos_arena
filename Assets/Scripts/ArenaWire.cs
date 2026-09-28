using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Prototype transport. All Unity state is read/written on the main thread.
public sealed class ArenaWire : IDisposable {
    public struct Event { public int peer; public string data; }
    sealed class Peer { public TcpClient client; public StreamWriter writer; public object gate = new object(); }
    readonly ConcurrentDictionary<int, Peer> peers = new ConcurrentDictionary<int, Peer>();
    public readonly ConcurrentQueue<Event> events = new ConcurrentQueue<Event>();
    TcpListener listener;
    volatile bool running = true;
    int nextId;
    public void Host(int port) {
        listener = new TcpListener(IPAddress.Any, port); listener.Start(3);
        new Thread(() => {
            try { while (running) {
                var c = listener.AcceptTcpClient();
                if (peers.Count >= 3) { c.Close(); continue; }
                Attach(c, Interlocked.Increment(ref nextId));
            }} catch (Exception e) { if (running) events.Enqueue(new Event { peer = -1, data = "!error " + e.Message }); }
        }) { IsBackground = true }.Start();
    }
    public void Join(string address, int port) {
        new Thread(() => { try {
            var c = new TcpClient(); var task = c.ConnectAsync(address, port);
            if (!task.Wait(5000)) { c.Close(); throw new IOException("Baglanti zaman asimi"); }
            if (!running) { c.Close(); return; } Attach(c, 0);
        } catch(Exception e) { events.Enqueue(new Event {peer = -1, data = "!error " + e.GetBaseException().Message}); }
        }) { IsBackground = true }.Start();
    }
    void Attach(TcpClient c, int id) {
        c.NoDelay = true; c.SendTimeout = 150; c.ReceiveTimeout = 15000;
        var p = new Peer { client = c, writer = new StreamWriter(c.GetStream()) {AutoFlush = true} };
        peers[id] = p; events.Enqueue(new Event {peer = id, data = "!connected"});
        new Thread(() => { try {
            using (var reader = new StreamReader(c.GetStream())) { while (running) {
                // Bound each incoming frame before handing it to the JSON parser.
                var chars = new System.Text.StringBuilder(); int ch;
                while ((ch = reader.Read()) != -1 && ch != '\n') { if (chars.Length >= 32768) throw new IOException("Frame too large"); if (ch != '\r') chars.Append((char)ch); }
                if (ch == -1) break;
                events.Enqueue(new Event {peer = id, data = chars.ToString()});
                if (events.Count > 2048) throw new IOException("Input flood");
            }}
        } catch {} finally { peers.TryRemove(id, out _); c.Close(); events.Enqueue(new Event {peer = id, data = "!disconnected"}); }
        }) { IsBackground = true }.Start();
    }
    public void Send(int id, string json) {
        if (!peers.TryGetValue(id, out var p)) return;
        try { lock(p.gate) p.writer.WriteLine(json); } catch { p.client.Close(); }
    }
    public void Broadcast(string json) { foreach (var id in peers.Keys) Send(id, json); }
    public void Disconnect(int id) { if (peers.TryRemove(id, out var p)) p.client.Close(); }
    public void Dispose() { running = false; listener?.Stop(); foreach (var p in peers.Values) p.client.Close(); peers.Clear(); }
}
