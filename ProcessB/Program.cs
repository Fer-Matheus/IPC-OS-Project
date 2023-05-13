using ProcessB;

if (OperatingSystem.IsWindows())
    Message.TextMessageWindows();
else
    Message.TextMessageLinux();
