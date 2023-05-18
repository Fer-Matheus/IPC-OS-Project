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
                                var name = reader.ReadLine();
                                ReadFile(name);
                            }
                            else if (received != "\0" || received != "\n")
                            {
                                Console.WriteLine($"Message received: {received}");
                            }
                        }
                        using (var stream = shm.CreateViewStream())
                        {
                            Console.WriteLine("Enter a message to processB:\n (0 - exit\t3 - Seend File)");
                            String name = "";
                            var writer = new StreamWriter(stream);
                            var message = Console.ReadLine();
                            if (message == "0") break;
                            else if (message == "3")
                            {
                                name = WriteFile();
                                writer.WriteLine(message);
                                writer.WriteLine(name);
                            }
                            else { writer.WriteLine(message); }
                            writer.Flush();
                            mutex.ReleaseMutex();
                        }
                    }
                }
                mutex.Dispose();
                shm.Dispose();
            }
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error : {e.Message}");
            Console.WriteLine("The connection have been closed.");
            Console.ReadLine();
        }
    }

    private static void ReadFile(string name)
    {
        Console.WriteLine("File received!");
        using (var shmFile = MemoryMappedFile.OpenExisting("File"))
        {
            using (var streamFile = shmFile.CreateViewStream())
            {
                StreamReader reader = new(streamFile);
                System.Console.WriteLine($"File name: {name}");
                var buffer = new MemoryStream();
                streamFile.CopyTo(buffer);
                File.WriteAllBytes($"ProcessA/file/{name}", buffer.ToArray());
            }
            shmFile.Dispose();
        }
    }

    private static String WriteFile()
    {
        try
        {
            Console.WriteLine("Enter the file path:");
            var path = Console.ReadLine();
            var file = File.Open(path, FileMode.Open);
            var shm = MemoryMappedFile.CreateNew("File", file.Length * 2);
            using (var stream = shm.CreateViewStream())
            {
                file.CopyTo(stream);
            }
            return file.Name.Split("\\")[file.Name.Split("\\").Length - 1];
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error : {e.Message}");
            Console.WriteLine("The connection have been closed.");
            Console.ReadLine();
        }
        return null;
    }
}
