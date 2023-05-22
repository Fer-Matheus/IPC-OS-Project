using System.Diagnostics;
using System.IO.MemoryMappedFiles;
MemoryMappedFile shm = MemoryMappedFile.CreateOrOpen("Message", 1024);


while (true)
{
    try
    {

        Console.WriteLine("Enter a message to send across the shared memory:\n\t0 - exit");
        string message = Console.ReadLine();

        if (message == "0") break;

        var id = Process.GetCurrentProcess().Id;
        
        shm = MemoryMappedFile.CreateOrOpen("Message", message.Length);
        using (var stream = shm.CreateViewStream())
        {
            using (BinaryWriter writer = new(stream))
            {
                writer.Write("");
                writer.Write(id);
                writer.Write(message);
            }
        }
    }
    catch (System.Exception)
    {

    }
}
shm.Dispose();