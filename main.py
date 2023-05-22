import subprocess
from time import sleep


def RunProcess(folder):
    terminal = ['cmd.exe', '/c', 'wt.exe']
    arguments = ['dotnet', 'run', '--project', folder]
    subprocess.Popen(terminal+arguments)


def Choise(op):
    if op == 1:
        RunProcess('.\P2P\ProcessA')
        RunProcess('.\P2P\ProcessB')
    elif op == 2:
        n = int(input("How many process do you want to shared?\n"))
        RunProcess('.\P-N\Sender')
        for i in range(n):
            sleep(2)
            RunProcess('.\P-N\Receiver')


op = int(input("Enter your choice:\n1 - P2P\t2 - P-N\n\n"))
Choise(op)
