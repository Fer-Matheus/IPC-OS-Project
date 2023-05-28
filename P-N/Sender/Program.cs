using System.Diagnostics;
using System.IO.MemoryMappedFiles;
Console.Clear();
// Temos a criação inicial da shared memory para que os demais n processos de leitura possam acessa-lá e se registrarem antes do envio das mensagens
MemoryMappedFile shm = MemoryMappedFile.CreateOrOpen("Message", 1024);

// Aqui pegamos o id do processo para que sejá enviado juntos da mensagens, a fim de identificar seu emissor
var id = Process.GetCurrentProcess().Id;

// O sender é deixado em loop para que consiga enviar novas mensagens até que escolha deixar o programa
while (true)
{
    try
    {

        Console.WriteLine("Enter a message to send across the shared memory:\n\t0 - exit");
        string message = Console.ReadLine();

        // Caso o Sender escolhe terminar o processo, a shared memory será fechada.
        if (message == "0") {
            shm.Dispose();
            break;
        }

        shm = MemoryMappedFile.CreateOrOpen("Message", message.Length);
        using (var stream = shm.CreateViewStream())
        {
            // A nova mensagem é escritaa na shared memory, junto com uma string de controle que servirá de registro, o id do próprio sender e a mensagem.
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