using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessA;

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
                            if (received == "3")
                            {
                                using (var shmFile = MemoryMappedFile.OpenExisting("File"))
                                {
                                    using (var streamFile = shmFile.CreateViewStream())
                                    {
                                        StreamReader readerFile = new(streamFile);
                                        var file = readerFile.ReadToEnd();
                                        Console.WriteLine(file);
                                    }
                                    shmFile.Dispose();
                                }
                            }
                            else if (received != null)
                            {
                                Console.WriteLine($"Message received: {received}");
                            }
                        }
                        using (var stream = shm.CreateViewStream())
                        {
                            Console.WriteLine("Enter a message to processB:\n (0 - exit\t3 - Seend File)");
                            // var message = Console.ReadLine();
                            var message = "3";
                            if (message == "0") break;
                            else if (message == "3")
                            {
                                WriteFile();
                            }
                            var writer = new StreamWriter(stream);
                            writer.WriteLine(message);
                            writer.Flush();
                            mutex.ReleaseMutex();
                        }
                    }
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

    private static void WriteFile()
    {

        try
        {
            Mutex mutex = new(false, "MutexFile");
            if (mutex.WaitOne())
            {
                Console.WriteLine("Enter the file path:");
                var path = Console.ReadLine();
                var file = File.Open(path, FileMode.OpenOrCreate);
                while (true)
                {
                    using (var shm = MemoryMappedFile.CreateOrOpen("File", 1024))
                    {
                        using (var stream = shm.CreateViewStream())
                        {
                            file.CopyTo(stream);
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
        }
        catch (System.Exception)
        {
            
        }
    }
}
