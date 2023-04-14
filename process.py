from os import *
import os
from threading import *
import multiprocessing.shared_memory as sh
from multiprocessing import Process
from time import sleep
import numpy
mutex = Lock()
def processo(i):
    a = numpy.random.randint(1, 5)
    sleep(a)
    mutex.acquire()
    print(f"processo {i} esperou {a} segundos para acessar a regiao critica")
    shm = sh.SharedMemory(name="shM", create=False)
    mutex.release()
if __name__ == '__main__':
    a = sh.SharedMemory(name="shM", create = True, size=4096)
    p = Process(target=processo, name="processo")
    pro = []
    for i in range(10):
       pro.append(Process(target=processo, name=f"processo{i}", args=[i]))
       pro[i].start()
    for i in range(10):
       pro[i].join()


  
 