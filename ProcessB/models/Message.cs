using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessB;

public static class Message
{
    public static void TextMessageWindows()
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
                            var received = reader.ReadLine();
                            if (received != null)
                            {
                                Console.WriteLine($"Message received: {received}");
                            }
                        }
                        using (var stream = shm.CreateViewStream())
                        {
                            Console.WriteLine("Enter a message to processA:\n (enter 0 to exit)");
                            var message = Console.ReadLine();
                            if (message == "0") break;
                            var writer = new StreamWriter(stream);
                            writer.WriteLine(message);
                            writer.Flush();
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
    public static void TextMessageLinux()
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
                        var received = reader.ReadLine();
                        if (received != null)
                        {
                            Console.WriteLine($"Message received: {received}");
                        }
                    }
                    using (var stream = shm.CreateViewStream())
                    {
                        Console.WriteLine("Enter a message to processA:\n (enter 0 to exit)");
                        var message = Console.ReadLine();
                        if (message == "0") break;
                        var writer = new StreamWriter(stream);
                        writer.WriteLine(message);
                        writer.Flush();
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
}
