using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace ProcessB;

public static class Message
{
    /*OBS:
        Todos os comentários contidos aqui se aplicam de forma analoga ao arquivo do ProcessB
    */
    public static void TextMessageWindows()
    {
        var id = Process.GetCurrentProcess().Id;
        try
        {
            // Aqui temos a criação, ou uma abertura se já existente, de uma memória compartilhada usando o nome preestabelecido
            using (var shm = MemoryMappedFile.CreateOrOpen("_SharedMemory", 2048))
            {
                // Mutex que servirá para coordenar os processos quanto ao acesso à shared memory
                Mutex mutex = new(false, "Mutex");

                // Os processos são deixados em loop que continuará até que um dos dois escolha parar e saia do mutex

                // É dada ao usuário a opção de escolher o tipo de envio file
                Console.WriteLine("Enter a message to processA:\n\t3 - Send File");
                while (true)
                {
                    // Essa view é a responsável por escrever tanto a mensagem como o possivel arquivo
                    using (var stream = shm.CreateViewStream())
                    {

                        // A variavél que receberá o nome do eventual file é iniciado

                        String name = "";

                        // Aqui atribuimos as funcionalidade de escrita à view, a fim de facilitar a conversão para bytes
                        StreamWriter writer = new(stream);

                        // Nesse ponto é lida a mensagem, ou possiveis flags de exit ou send file
                        var message = Console.ReadLine();

                        // Nesse ponto é verificado o acesso à shared memory, onde caso não haja alguém na região crítica o acesso é liberado, caso contrário, o processo irá esperar até que um sinal de release sejá recebido

                        // Saída da execução do programa
                        if (message == "0") break;

                        // Acionamento do fluxo de envio do file via shared memory
                        else if (message == "3")
                        {
                            // A leitura da shared memory é definida na seção 3. Código do trabalho escrito.
                            name = WriteFile();
                            if (mutex.WaitOne())
                            {
                                writer.WriteLine(message);
                                writer.WriteLine(name);
                                writer.WriteLine(id);
                            }
                        }
                        else
                        {
                            if (mutex.WaitOne())
                            {
                                // No caso em que as flags não são usadas, apenas a mensagem é escrita na shared memory
                                writer.WriteLine(message);
                                writer.WriteLine(id);
                            }
                        }

                        // Aqui é feito a limpeza do buffer de escrita e é finalizada a conexão com a view
                        writer.Flush();

                        // Nesse ponto, liberamos o acesso a shared memory ao outro processo
                        mutex.ReleaseMutex();
                    }
                }

                // Aqui realizamos a desassociação das ferramentas usadas
                mutex.Dispose();
                shm.Dispose();
            }
        }
        catch (System.Exception)
        {
            // Em caso de erros, a conexão será fechada

            Console.WriteLine("The connection have been closed.");
            Thread.Sleep(4000);
        }
    }

    // Função que define a leitura do arquivo
    private static void ReadFile(string name)
    {
        Console.WriteLine("File received!");
        // Nesse ponto é aberta a conexão com shared memory que contém o arquivo compartilhado, usando o nome preestabelecido "File"
        using (var shmFile = MemoryMappedFile.OpenExisting("File"))
        {
            using (var streamFile = shmFile.CreateViewStream())
            {
                StreamReader reader = new(streamFile);

                // Informado para o usuário o nome do arquivo recebido
                System.Console.WriteLine($"File name: {name}");

                // Aqui usamos um buffer para comportar os bytes lidos da shared memory, que posteriormente serão usados para recriar o arquivo.
                var buffer = new MemoryStream();
                streamFile.CopyTo(buffer);
                // O arquivo enviado é salvo na pasta de files do destinatário
                File.WriteAllBytes($"P2P/ProcessB/file/{name}", buffer.ToArray());
            }
            shmFile.Dispose();
        }
    }

    // Função que define o fluxo de envio dos arquivos via shared memory
    private static String WriteFile()
    {
        try
        {

            Console.WriteLine("Enter the file name:");
            var path = Console.ReadLine();


            // Nesse ponto, o arquivo escolhido para ser enviado é carregado para uma variável do tipo FileStream que trata o arquivo para bytes
            var file = File.Open($"P2P/ProcessB/file/{path}", FileMode.OpenOrCreate);

            // Nesse ponto é criada uma shared memory exclusiva para o envio de arquivos

            var shm = MemoryMappedFile.CreateOrOpen("File", file.Length);
            using (var stream = shm.CreateViewStream())
            {
                // Usando uma view para acessar a shared memory, usamos a instância do arquivo para copiar os bytes para a shared memomy
                file.CopyTo(stream);
            }
            // Nesse ponto pegamos apenas o nome do arquivo enviado, para que sejá informado na shared memory principal
            string name = file.Name.Split("\\")[file.Name.Split("\\").Length - 1];
            file.Close();
            return name;

        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
    public static void ReadMessage()
    {
        while (true)
        {
            using (var shm = MemoryMappedFile.CreateOrOpen("_SharedMemory", 2048))
            {
                // Aqui utilizamos views que vão acessar a shared memory e nos possibilitar ler e escrever na mesma.
                using (var stream = shm.CreateViewStream())
                {
                    // Aqui usamos a Classe StreamReader que irá definir a view de modo que possamos ler os bytes mais facilmente
                    StreamReader reader = new(stream);

                    // A leitura da shared memory é definida na seção 2.2 do trabalho escrito.
                    // Leitura da flag
                    var received = reader.ReadLine();

                    // Mensagem escrita na shared memory
                    if (!(received[0] == 0))
                    {
                        // Tratamento quanto à flag lida na shared memory
                        if (received == "3")
                        {
                            // Caso a flag corresponda a mesma definida para "files", o fluxo de leitura do arquivo é iniciado
                            var name = reader.ReadLine();
                            var id = reader.ReadLine();
                            if (id != Process.GetCurrentProcess().Id.ToString())
                            {
                                ReadFile(name);
                                stream.Seek(0, SeekOrigin.Begin);
                                for (int i = 0; i < received.Length + id.Length + name.Length + 3; i++)
                                {
                                    stream.WriteByte(0);
                                }
                            }
                        }
                        else
                        {
                            var id = reader.ReadLine();
                            if (id != Process.GetCurrentProcess().Id.ToString())
                            {
                                Console.WriteLine($"Process {id} sent: {received}");
                                stream.Seek(0, SeekOrigin.Begin);
                                for (int i = 0; i < received.Length + id.Length + 2; i++)
                                {
                                    stream.WriteByte(0);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
