import subprocess


def RunProcess(folder):
    terminal = ['cmd.exe', '/c', 'wt.exe']
    arguments = ['dotnet', 'run', '--project', folder]
    subprocess.Popen(terminal+arguments)


def Choise(op):
    if op == 1:
        RunProcess('.\ProcessA')
        RunProcess('.\ProcessB')


RunProcess('.\ProcessA')
RunProcess('.\ProcessB')
