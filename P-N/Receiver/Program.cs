using System.IO.MemoryMappedFiles;

int idProcess = -1;
string stringControl, message;

var mutex = new Mutex(false, "Mutex");

try
{
    while (true)
    {
        using (var shm = MemoryMappedFile.OpenExisting("Message"))
        {
            if (idProcess == -1)
            {
                if (mutex.WaitOne())
                {
                    idProcess = Register(idProcess, out stringControl, out message, shm);
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                if (mutex.WaitOne())
                {
                    ReadMessage(out stringControl, out message, shm);
                    try
                    {
                        if (stringControl[idProcess] == '1') continue;
                        else
                        {
                            WriteSignal(stringControl, message, shm);
                            ReadShowMessage(out stringControl, out message, shm);

                        }
                    }
                    catch (System.Exception)
                    {
                        idProcess = -1;
                    }
                    finally { mutex.ReleaseMutex(); }
                }
            }
        }
        
    }
}
catch (System.Exception)
{

}


int Register(int idProcess, out string stringControl, out string message, MemoryMappedFile shm)
{
    ReadMessage(out stringControl, out message, shm);
    using (var stream = shm.CreateViewStream())
    {
        using (var writer = new BinaryWriter(stream))
        {
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(stringControl + "0");
            writer.Write(message);
            idProcess = stringControl.Length;
        }
    }

    return idProcess;
}

void ReadShowMessage(out string stringControl, out string message, MemoryMappedFile shm)
{
    using (var stream = shm.CreateViewStream())
    {
        using (var reader = new BinaryReader(stream))
        {
            stringControl = reader.ReadString();
            message = reader.ReadString();

            Console.WriteLine("StringControl: " + stringControl);
            Console.WriteLine("Message on the shared memory:");
            Console.WriteLine(message);
            Console.WriteLine();
        }
    }
}
void ReadMessage(out string stringControl, out string message, MemoryMappedFile shm)
{
    using (var stream = shm.CreateViewStream())
    {
        using (var reader = new BinaryReader(stream))
        {
            stringControl = reader.ReadString();
            message = reader.ReadString();
        }
    }
}
void WriteSignal(string stringControl, string message, MemoryMappedFile shm)
{
    using (var stream = shm.CreateViewStream())
    {
        using (var writer = new BinaryWriter(stream))
        {
            writer.Seek(0, SeekOrigin.Begin);
            string newStringControl = "";
            for (int i = 0; i < stringControl.Length; i++)
            {
                if (i == idProcess)
                    newStringControl += "1";
                else{
                    var temp = stringControl[i];
                    newStringControl += temp;
                }
            }
            writer.Write(newStringControl);
            writer.Write(message);
        }
    }
}