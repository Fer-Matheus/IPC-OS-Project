import os, sys, subprocess
from multiprocessing import Lock, Process, shared_memory as sh
from threading import Thread
from time import sleep
mutex = Lock()
def strId(id):
    strid = str(id)
    i = 5 - strid.__len__()
    for a in range(i):
        strid = '0'+strid
    return strid
def openNewTerminal():
    console = ['cmd.exe', '/c', 'start'] # or something
    cmd = ['python', 'process.py']
    return subprocess.Popen(console + cmd)
def transmissao(n, id):
    global mutex
    while(True):
        sleep(0.1)
        mutex.acquire()
        shm = sh.SharedMemory(name="shM", create=False)
        if(bytes(shm.buf[n:n+1]).decode() == '\0' or bytes(shm.buf[id:id+1]).decode() == '1'):
            if(bytes(shm.buf[0:n]).decode() == '1'*n):
                i = n
                while(bytes(shm.buf[i:i+1]).decode() != '\0'):
                    i+=1
                shm.buf[:i] = b'\0'*i
                print("Mensagem expirada")
            shm.close()
            mutex.release()
            continue
        else:
            i = n
            while(bytes(shm.buf[i:i+1]).decode() != '\0'):
                i+=1
            print(f"Processo {os.getpid()} recebeu :{bytes(shm.buf[n:i]).decode()}")
            shm.buf[id:id+1] = b'1'
            shm.close()
            mutex.release()
def senderProcess(n):
    while(True):
        msg = input("mensagem: \n")
        mutex.acquire()
        shm = sh.SharedMemory(name="shM", create=False)
        i = msg.__len__() + n
        shm.buf[n:i] = bytes(msg, 'utf-8')
        shm.close()
        mutex.release() 
if __name__ == '__main__':
    shm = ""
    opcao = int(input("Digite:\n1 - P2P\n2 - PnP\n3 - Envio de arquivos\n"))
    if(opcao == 1):
        shm = sh.SharedMemory(name="shM", create=True, size=4096)
        openNewTerminal()
        openNewTerminal()
    elif(opcao == 2):
        shm = sh.SharedMemory(name="shM", create=True, size=4096)
        n = int(input("Quantos processos devem ser abertos?\n"))
        processes = []
        for i in range(n):
            processes.append(Process(target=transmissao, args=[n, i]))
        sender = Thread(target=senderProcess, args=[n])
        sender.start()
        for i in range(n):
            processes[i].start()
        sender.join()
        for i in range(n):
            processes[i].join()
    shm.close()
