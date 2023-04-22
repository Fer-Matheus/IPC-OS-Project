using System.IO.MemoryMappedFiles;
using System.Text;
using ProcessB;

using (var file = new FileStream("/home/matheus/TEMP/sharedMemory", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
using (var shm = MemoryMappedFile.CreateFromFile(file, null, 2048, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true))
{
    while (true)
    {
        if (OwnerMutex.WaitOne("/home/matheus/TEMP/mutex"))
        {
            using (var stream = shm.CreateViewStream())
            {
                StreamReader reader = new(stream);
                var received = reader.ReadToEnd();
                if (received != null)
                {
                    Console.WriteLine($"Message received: {received}");
                    stream.Write(Encoding.ASCII.GetBytes(""));
                }
            }
            using (var stream = shm.CreateViewStream())
            {
                Console.WriteLine("Enter a message to processA:\n (enter 0 to exit)");
                var message = Console.ReadLine();
                if (message == "0") break;
                stream.Write(Encoding.UTF8.GetBytes(message));
                OwnerMutex.Release("/home/matheus/TEMP/mutex");
            }
        }
        Thread.Sleep(2000);
    }
    shm.Dispose();
    file.Dispose();
    File.Delete("/home/matheus/TEMP/mutex");
    File.Delete("/home/matheus/TEMP/sharedMemory");
}