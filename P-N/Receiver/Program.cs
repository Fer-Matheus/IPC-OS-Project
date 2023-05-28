using System.IO.MemoryMappedFiles;

Console.Clear();

// Definimos para cada processo as seguintes variáveis que serviram de controle das informações contidas na shared memory

// idProcess é iniciado como negativo para que seja identificado seu ainda não registro, bem como o senderID será usado para armazenar o id do emissor da mensagem
int idProcess = -1, senderID;

// stringControl será usada como registro dos processos que acessam a shared memory, e a message é a mensagem contida na shared memory 
string stringControl = "", message;

// O mutex será usado para coordenar o acesso a região crítica
Mutex mutex = new(false, "Mutex");

try
{
    while (true)
    {
        using (var shm = MemoryMappedFile.OpenExisting("Message"))
        {
            // Aqui se verifica se o processor ainda não se registrou na shared memory
            if (idProcess == -1)
            {
                if (mutex.WaitOne())
                {
                    // O id do processo será obtida ao se analisar a stringControl contida na shared memory
                    // OBS: a palavra reservada "out" serve para passar a referência de memória da variável

                    // Função que registra os processos na shared memory
                    idProcess = Register(idProcess, out stringControl, out message, shm);

                    // Após o registro, é liberado o acesso para os outros processos
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                if (mutex.WaitOne())
                {
                    // A função ReadMessage é usada para atualizar as informações contidas na shared memory
                    ReadMessage(out stringControl, out message, out senderID, shm);
                    try
                    {
                        // Nesse ponto é verificado se o processo já informou na stringControl que leu a mensagem, sendo verdade ele apenas ignora a mensagem já lida
                        if (stringControl[idProcess] == '1') continue;

                        // Do contrário ele lê e informa por meio da função WriteSignal
                        else
                        {
                            // Essa função apenas escreve "1" na stringControl, no index correspondente ao seu idProcess, informando que leu a mensagem
                            WriteSignal(stringControl, message, shm);

                            // Este método exibe as informações contidas na shared memory
                            ShowMessage(out stringControl, out message, shm);

                            // Nesse ponto testamos se a stringControl possui apenas "1" para todos os campos dos sinais, indicando que todos leram, mas verificamos também a message para que não caia no caso apenas de registro doss processos
                            if (!CheckStringControl(stringControl) && message != "")
                            {
                                // Aqui é informadoque todos já leram, e a mensagem é apagada
                                Console.WriteLine("We've all read the message\n");
                                CleanMessage(shm);
                            }
                        }

                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        // Este caso acontece caso a stringControl sejá reiniciada, pelo sender, para o valor "". Desse modo reiniciamos o id do processo também
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
    // Atualizamos as variaveis por meio dessa função
    ReadMessage(out stringControl, out message, out senderID, shm);

    using (var stream = shm.CreateViewStream())
    {
        // De posse dos valores atualizados, reescrevemos a shared memory com o registro do processo
        // OBS: O formato da shared memory é {stringControl\nsenderID\nmessage\n}; isso é melhor definido na seção 3. algo no trabalho escrito
        using (var writer = new BinaryWriter(stream))
        {
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(stringControl + "0");
            writer.Write(senderID);
            writer.Write(message);
            idProcess = stringControl.Length;
        }
    }
    // O id do processo é obtido e retornado para que sejá registrado
    return idProcess;
}

void ShowMessage(out string stringControl, out string message, MemoryMappedFile shm)
{
    using (var stream = shm.CreateViewStream())
    {
        // Método que lê e atualiza as variáveis com as novas informações da shared memory
        ReadMessage(out stringControl, out message, out senderID, shm);

        // Caso a mensagem esteja vazia, nada será exibido
        if (message != "")
        {
            Console.WriteLine($"Sender ID: {senderID}");
            Console.WriteLine("Message on the shared memory:");
            Console.WriteLine(message);
            Console.WriteLine();
        }
    }
}

void ReadMessage(out string stringControl, out string message, out int senderID, MemoryMappedFile shm)
{
    // Aqui apenas é realizada a leitura da shared memory e atualização dos valores das variáveis
    using (var stream = shm.CreateViewStream())
    {
        using (var reader = new BinaryReader(stream))
        {
            stringControl = reader.ReadString();
            senderID = reader.ReadInt32();
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

            // Aqui, recriamos a stringControl, mas trocando o valor no index correnspondente ao id do processo
            for (int i = 0; i < stringControl.Length; i++)
            {
                if (i == idProcess)
                    newStringControl += "1";
                else
                {
                    var temp = stringControl[i];
                    newStringControl += temp;
                }
            }
            // por fim atualizamos a shared memory
            writer.Write(newStringControl);
            writer.Write(senderID);
            writer.Write(message);
        }
    }
}
bool CheckStringControl(string stringControl)
{
    // Aqui checamos a stringControl para verificar se todos os processos já levam, por meio dos sinais estarem todos em "1" 
    foreach (char i in stringControl)
    {
        if (i != '1') return true;
    }
    return false;
}
void CleanMessage(MemoryMappedFile shm)
{
    // Ao ser verificado que todos leram, a mensagem é apagada. Então atualizamos a shared memory
    using (var stream = shm.CreateViewStream())
    {
        using (BinaryWriter writer = new(stream))
        {

            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(stringControl);
            writer.Write(senderID);
            writer.Write("");
        }
    }
}