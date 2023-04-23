using System.IO.MemoryMappedFiles;
using System.Text;
using ProcessB;

if (!OperatingSystem.IsWindows())
{

    string path = "/home/matheus/workspace/OS-Project/TEMP/";

    using (var file = new FileStream(path + "SharedMemory", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
    using (var shm = MemoryMappedFile.CreateFromFile(file, null, 2048, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true))
    {
        while (true)
        {
            if (OwnerMutex.WaitOne(path + "Mutex"))
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
                    OwnerMutex.Release(path + "Mutex");
                }
            }
            Thread.Sleep(2000);
        }
        shm.Dispose();
        file.Dispose();
        File.Delete(path + "Mutex");
        File.Delete(path + "SharedMemory");
    }
}
else
{
    try
    {
        using (var shm = MemoryMappedFile.CreateOrOpen("_SharedMemory", 2048))
        {
            Mutex mutex = new(false, "Mutex");
            while (true)
            {
                if (mutex.WaitOne())
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
                        mutex.ReleaseMutex();
                    }
                }
                Thread.Sleep(2000);
            }
            mutex.Dispose();
            shm.Dispose();
        }
    }
    catch (System.Exception)
    {
        Console.WriteLine("The connection have been closed.");
    }
}