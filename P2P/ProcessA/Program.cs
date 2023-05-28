using System.Diagnostics;
using ProcessA;

var id = Process.GetCurrentProcess().Id;

Thread reader = new Thread(Message.ReadMessage);
reader.Start();

Thread writer = new Thread(Message.TextMessageWindows);
writer.Start();


reader.Join();
writer.Join();
