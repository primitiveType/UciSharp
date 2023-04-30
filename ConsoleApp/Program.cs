// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.IO.Pipes;
using UciSharp;

Console.WriteLine("Hello, World!");
string path = "C:\\Users\\Arthu\\Downloads\\arasan23.5\\arasanx-64.exe";
await TestWrapper(path);


async Task TestWrapper(string path1)
{
    ChessEngine engine = new(path1);

    await engine.StartAsync();

    string? consoleInput = null;
    while (consoleInput != "q")
    {
        if (consoleInput != null)
        {
            await engine.SendCommandAsync(consoleInput);
        }

        await Task.Delay(10);
        consoleInput = Console.ReadLine();
    }
    await engine.DisposeAsync();
}
// TestManual(path);

void TestManual(string s)
{
    using var stdinPipe = new AnonymousPipeServerStream(PipeDirection.Out);
    using var writer = new StreamWriter(stdinPipe);

// Write message to child process
// writer.WriteLine("uci");
// writer.Flush();

// Read output from child process
    using var stdoutPipe = new AnonymousPipeServerStream(PipeDirection.Out);
    var startInfo = new ProcessStartInfo
    {
        FileName = s,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        Arguments = $"{stdoutPipe.GetClientHandleAsString()} {stdinPipe.GetClientHandleAsString()}"
    };
    using var process = Process.Start(startInfo);
    var output = process.StandardOutput.ReadToEnd();
    Console.WriteLine(output);
    process.WaitForExit();
}
