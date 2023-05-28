import subprocess
from time import sleep
import PySimpleGUI as sg
# Definimos a função que executará os códigos C# no terminal
def RunProcess(folder):

    # Definimos em que terminal será executado, bem como os argumentos que seram executados
    terminal = ['cmd.exe', '/c', 'wt.exe']
    arguments = ['dotnet', 'run', '--project', folder]
    subprocess.Popen(terminal+arguments)

# Aqui temos um "switch" que executará os programas de acordo com a escolha do usuário
def Choise(op, n=0):

    # P2P
    if op == 1:
        RunProcess('.\P2P\ProcessA')
        RunProcess('.\P2P\ProcessB')

    # P-N
    elif op == 2:

        # A quantidade de processos que atuaram como assinantes da newsletter

        RunProcess('.\P-N\Sender')
        
        # 'n' significa a quantidade de processos escolhidos no P-N
        for i in range(n):
            sleep(2)
            RunProcess('.\P-N\Receiver')
# Função que cria o menu de escolha entre P2P e P-N
def menu():
    # Definimos um tema da GUI usando o pysimpleGUI
    sg.theme('DarkAmber')

    # Inserimos os inputs, labels e botões de ação em um layout
    layout = [  [sg.Radio("P2P", "group1", False)],
            [sg.Radio("PnP", "group1", False)],
            [sg.OK(), sg.Cancel("Sair")]]
    # Inicialização da janela da GUI, sendo inserido o layout e o título
    window = sg.Window('Escolha uma opção', layout)

    # O programa ficará em loop até que uma opção seja escolhida ou
    # o usuário escolha sair
    while True:             
        event, values = window.read()
        if event in (sg.WIN_CLOSED, 'Sair'):
            break
        # Chamada da opção 1(P2P), caso tenha sido escolhida
        if values[0]:
            Choise(1)
        # Chamada da opção 2(P-N), caso tenha sido escolhida
        elif values[1]:
            #Função que permite a escolha da quantidade 'n' de processos
            escolhaDeQuantidade()
# Função para exibir erros com a GUI, onde é passada a mensagem do erro
def erro(msg):
    sg.theme('DarkAmber')
    layout = [  [sg.Text(str(msg))],]

    window = sg.Window(str(msg), layout)
    event, values = window.read()

# Função responsável pela escolha da quantidade 'n' de processos
def escolhaDeQuantidade():
    # Criação da GUI
    sg.theme('DarkAmber')
    # Inserimos o input, label e botões de ação em um layout
    layout = [  [sg.Text('Quantos processos devem ser abertos?'), sg.InputText()],
            [sg.OK(), sg.Cancel()]]

    # Inicialização da janela da GUI, sendo inserido o layout e o título
    window = sg.Window('Qtd de Processos', layout)
    # equanto houver erro, será pedido novamente para inserir um número
    # 'n' de processos
    e = True
    while e:
        try:             
            event, values = window.read()
            if event in (sg.WIN_CLOSED, 'Cancel'):
                window.close()
                break
            n = int(values[0])

            # Erro caso tenha sido escolhida uma quantidade muito baixa
            # ou alta de processos
            if n<1:
                erro("Quantidade insuficiente de processos")
            elif n>15:
                erro("Muitos processos seriam abertos")
            else:
                # A abertura dos processos é chamada e o erro acaba

                Choise(2, n)
                e = False
        except:
            # Exibe um erro para o caso em que não foi possível
            # converter 'values[0]' para inteiro
            erro("Digite um valor inteiro válido")
# Main do projeto       
menu()

