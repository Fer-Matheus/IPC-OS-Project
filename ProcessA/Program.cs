using ProcessA;


if (OperatingSystem.IsWindows())
    Message.TextMessageWindows();
else
    Message.TextMessageLinux();
