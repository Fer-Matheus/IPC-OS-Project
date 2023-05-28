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
        for i in range(n):
            sleep(2)
            RunProcess('.\P-N\Receiver')

def menu():
    sg.theme('DarkAmber')
    layout = [  [sg.Radio("P2P", "group1", False)],
            [sg.Radio("PnP", "group1", False)],
            [sg.OK(), sg.Cancel()]]

    window = sg.Window('Escolha uma opção', layout)
    while True:             
        event, values = window.read()
        if event in (sg.WIN_CLOSED, 'Cancel'):
            break
        if values[0]:
            Choise(1)
        elif values[1]:
            escolhaDeQuantidade()
def erro(msg):
    sg.theme('DarkAmber')
    layout = [  [sg.Text(str(msg))],]

    window = sg.Window(str(msg), layout)
    event, values = window.read()
def escolhaDeQuantidade():
    sg.theme('DarkAmber')
    layout = [  [sg.Text('Quantos processos devem ser abertos?'), sg.InputText()],
            [sg.OK(), sg.Cancel()]]

    window = sg.Window('Qtd de Processos', layout)
    e = True
    while e:
        try:             
            event, values = window.read()
            if event in (sg.WIN_CLOSED, 'Cancel'):
                window.close()
                break
            n = int(values[0])
            if n<1:
                erro("Quantidade insuficiente de processos")
            elif n>15:
                erro("Muitos processos seriam abertos")
            else:
                Choise(2, n)
                e = False
        except:
            erro("Digite um valor inteiro válido")
# Main do projeto       
menu()

