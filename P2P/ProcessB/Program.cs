using System.Diagnostics;
using ProcessB;

var id = Process.GetCurrentProcess().Id;

Thread reader = new Thread(Message.ReadMessage);
reader.Start();

Thread writer = new Thread(Message.TextMessageWindows);
writer.Start();


writer.Join();
reader.Join();
