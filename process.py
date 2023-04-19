from os import *
import os
from threading import *
import multiprocessing.shared_memory as sh
mutex = Lock()
def strId(id):
    strid = str(id)
    i = 5 - strid.__len__()
    for a in range(i):
        strid = '0'+strid
    return strid
def leituraT():
  while(True):
    mutex.acquire()
    shm = sh.SharedMemory(name="shM", create=False)
    if(bytes(shm.buf[:1]).decode() == '\0'):
        mutex.release()
        continue
    elif(int(bytes(shm.buf[:5]).decode()) == getpid()):
        mutex.release()
        continue
    else:
        i = 5
        while(bytes(shm.buf[i:i+1]).decode() != '\0'):
            i+=1
        print(f"Processo {bytes(shm.buf[:5]).decode()}:{bytes(shm.buf[5:i]).decode()}\n")
        shm.buf[:i] = b'\0'*i
        shm.close()
        mutex.release()
    
def escritaT():
  while(True):
    
    msg = input("digite uma mensagem\n")
    mutex.acquire()
    shm = sh.SharedMemory(name="shM", create=False)
    i = msg.__len__() + 5
    shm.buf[:i] = bytes(strId(getpid())+msg, 'utf-8')
    shm.close()
    mutex.release()    
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
 