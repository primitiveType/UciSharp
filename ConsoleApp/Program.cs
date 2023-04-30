// See https://aka.ms/new-console-template for more information

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
