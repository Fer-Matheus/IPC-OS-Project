from os import getpid
import os
from threading import *
import multiprocessing.shared_memory as sh
from time import sleep
mutex = Lock()
path = os.getcwd()
newShm = ""
a = 0
def strId(id):
    strid = str(id)
    i = 5 - strid.__len__()
    for a in range(i):
        strid = '0'+strid
    return strid
def leituraT():
  global path
  dirpath =  path+"\\"+str(getpid())
  while(True):
    sleep(0.1)
    mutex.acquire()
    shm = sh.SharedMemory(name="shM", create=False)
    if(bytes(shm.buf[:1]).decode() == '\0'):
        mutex.release()
        continue
    elif(int(bytes(shm.buf[:5]).decode()) == getpid()):
        mutex.release()
        continue
    else:
        shm2 = sh.SharedMemory(name="FileShm", create=False)
        j=5
        while(bytes(shm.buf[j:j+1]).decode() != '\0'):
            j+=1
        tam = ""
        k = j+1
        while(bytes(shm.buf[k:k+1]).decode() != '\0'):
            tam = bytes(shm.buf[k:k+1]).decode() + tam
            k+=1
        tam = int(tam)
        filepath = bytes(shm.buf[5:j]).decode()
        filename = filepath.split("\\")[filepath.split("\\").__len__()-1]
        print(f"Processo {bytes(shm.buf[:5]).decode()} enviou o arquivo:{filename}\n")
        if not os.path.exists(dirpath):
            os.mkdir(dirpath)
        with open(dirpath+"\\"+filename, 'wb') as file:
            file.write(shm2.buf[:tam])


        shm.buf[:j] = b'\0'*j
        shm2.buf[:tam] = b'\0'*tam
        shm.close()
        shm2.close()
        mutex.release()
    
def escritaT():
  while(True):
    global a
    sleep(0.1)
    filepath = input("Digite o caminho do arquivo:\n")
    file = open(filepath, 'rb')
    file = file.read()
    filename = filepath.split("\\")[filepath.split("\\").__len__()-1]
    mutex.acquire()
    shm = sh.SharedMemory(name="shM", create=False)
    a = 1
    newShm = sh.SharedMemory(name="FileShm", create=True, size=file.__len__()*2)
    newShm.buf[:file.__len__()] = file
    a = 0
    shm.buf[:5+filepath.__len__()+str(file.__len__()).__len__()+1] = bytes(strId(getpid())+filepath+'\0'+str(file.__len__()), 'utf-8')
    shm.close()
    mutex.release()
    b = 1
    while(b == 1):
        sleep(0.1)
        try:
            if(bytes(newShm.buf[:1]).decode() == '\0'):
                newShm.close()
                newShm = ""
                b = 0   
        finally:
            continue
if __name__ == '__main__':
    shm =""
    flag = 0
    try:
        shm = sh.SharedMemory(name="shM", create=True, size=4096)
        flag = 1
    except:
        pass
    leitura = Thread(target=leituraT)
    escrita = Thread(target=escritaT)
    leitura.start()
    escrita.start()
    leitura.join()
    escrita.join()
    if(flag):
        shm.close()
        # mutexshm.close()
 