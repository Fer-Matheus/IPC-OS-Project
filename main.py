import subprocess
from time import sleep

# Definimos a função que executará os códigos C# no terminal
def RunProcess(folder):

    # Definimos em que terminal será executado, bem como os argumentos que seram executados
    terminal = ['cmd.exe', '/c', 'wt.exe']
    arguments = ['dotnet', 'run', '--project', folder]
    subprocess.Popen(terminal+arguments)

# Aqui temos um "switch" que executará os programas de acordo com a escolha do usuário
def Choise(op):

    # P2P
    if op == 1:
        RunProcess('.\P2P\ProcessA')
        RunProcess('.\P2P\ProcessB')

    # P-N
    elif op == 2:

        # A quantidade de processos que atuaram como assinantes da newsletter
        n = int(input("How many process do you want to shared?\n"))

        RunProcess('.\P-N\Sender')
        for i in range(n):
            sleep(2)
            RunProcess('.\P-N\Receiver')

# Uma pequena tratação de erro pro caso de ser digitado valores não validos
while (True):
    try:
        op = int(input("Enter your choice:\n1 - P2P\t2 - P-N\n\n"))
        if op != 1 and op != 2:
            raise Exception
        break
    except:
        print('Enter a valid value!')

# Main do projeto       
Choise(op)
