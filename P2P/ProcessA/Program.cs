using System.Diagnostics;
using ProcessA;

Console.Clear();

 /*OBS:
        Todos os comentários contidos aqui se aplicam de forma analoga ao arquivo do ProcessB
    */

// Aqui é criada a thread que ficará responsável pela leitura da SHM,
// verificando se existem novas mensagens para o processo
Thread reader = new Thread(Message.ReadMessage);
// A thread é inicializada
reader.Start();

// Aqui é criada a thread que ficará responsável pela escrita na SHM,
// aguardando uma mensagem ou sinalização de envio de arquivo
Thread writer = new Thread(Message.TextMessageWindows);
// A thread é inicializada
writer.Start();

// o programa espera o término das threads
reader.Join();
writer.Join();
