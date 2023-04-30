using System.Text.RegularExpressions;

namespace UciSharp.Tests;

public class Tests
{
    private readonly string path = "C:\\Users\\Arthu\\Downloads\\arasan23.5\\arasanx-64.exe";

    [SetUp]
    public void Setup() { }

    [Test]
    public async Task TestChessEngineOptionsPopulated()
    {
        ChessEngine engine = new(path);
        MemoryStream stream = new MemoryStream(new byte[255]);
        StreamWriter writer = new(stream);
        await engine.Start(stream);
        await writer.WriteAsync("uci\n");
        await engine.Stop();
        Assert.That(engine.AvailableOptions, Has.Count.GreaterThan(0));
    }

    [Test]
    public void TestOptionsRegex()
    {
        string regexString =
            @"^option\s+name\s+\S+\s+(?:type\s+)?(?<typeval>\S+)\s+(?:min\s+(?<minval>\S+)\s+)?(?:max\s+(?<maxval>\S+)\s+)?(?:default\s+(?<defaultval>\S+)\s+)?(?:var\s+(?<varval>\S+)\s+)?";
        Regex regex = new(regexString);
        Match answer = regex.Match("option name Ponder type check default true");

        Assert.That(answer.Success, Is.True);
        Assert.That(answer.Groups.TryGetValue("typeval", out Group? type));
        Assert.That(type.Value, Is.EqualTo("check"));
    }

    // [Test]
    // public async Task Test1()
    // {
    //     await using Stream input = Console.OpenStandardInput();
    //     await using FileStream output = File.Create("chessoutput.txt");
    //     Command command = Cli.Wrap(path)
    //         .WithStandardInputPipe(PipeSource.FromStream(input)).WithStandardOutputPipe(PipeTarget.ToStream(output));
    //     PipeSource pipe = command.StandardInputPipe;
    //     CommandTask<CommandResult> task = command.ExecuteAsync();
    //
    //     await task;
    // }
}
