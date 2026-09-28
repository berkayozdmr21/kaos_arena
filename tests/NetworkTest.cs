using System;
using System.Diagnostics;
using System.Threading;
class NetworkTest {
    static void Main() {
        using var host=new ArenaWire(); host.Host(17777);
        var clients=new[]{new ArenaWire(),new ArenaWire(),new ArenaWire()};
        try {
            foreach(var c in clients)c.Join("127.0.0.1",17777);
            int connections=0;var timer=Stopwatch.StartNew();
            while(connections<3&&timer.ElapsedMilliseconds<6000){while(host.events.TryDequeue(out var e))if(e.data=="!connected")connections++;Thread.Sleep(10);}
            Require(connections==3,"One host accepts three clients");
            foreach(var c in clients)c.Send(0,"input-frame");
            int frames=0;timer.Restart();while(frames<3&&timer.ElapsedMilliseconds<2000){while(host.events.TryDequeue(out var e))if(e.data=="input-frame")frames++;Thread.Sleep(10);}
            Require(frames==3,"Host receives three independent input streams");
            host.Broadcast("world-state");
            for(int i=0;i<3;i++){bool seen=false;timer.Restart();while(!seen&&timer.ElapsedMilliseconds<2000){while(clients[i].events.TryDequeue(out var e))if(e.data=="world-state")seen=true;Thread.Sleep(10);}Require(seen,"Client "+i+" receives shared world state");}
            host.Dispose();
            for(int i=0;i<3;i++){bool seen=false;timer.Restart();while(!seen&&timer.ElapsedMilliseconds<2000){while(clients[i].events.TryDequeue(out var e))if(e.data=="!disconnected")seen=true;Thread.Sleep(10);}Require(seen,"Client "+i+" detects host shutdown");}
            Console.WriteLine("PASS: 8 transport assertions. This does not test Unity gameplay or internet NAT traversal.");
        }finally{foreach(var c in clients)c.Dispose();}
    }
    static void Require(bool condition,string message){if(!condition)throw new Exception(message);Console.WriteLine("PASS: "+message);}
}
