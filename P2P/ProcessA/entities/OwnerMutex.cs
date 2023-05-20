using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessA;

public static class OwnerMutex
{
    public static bool WaitOne(string path)
    {
        using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            using (var reader = new StreamReader(file))
            {
                int signal;
                string value = reader.ReadToEnd();
                try
                {
                    signal = int.Parse(value);
                }
                catch (System.Exception)
                {
                    signal = 0;
                }
                if (signal != 0) return false;
            }
        }
        using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            using (var stream = new StreamWriter(file.Name, false))
            {
                stream.Write(1);
            }
        }
        return true;
    }
    public static void Release(string path)
    {
        using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            using (var stream = new StreamWriter(file.Name, false))
            {
                stream.Write(0);
            }
        }
    }
}
