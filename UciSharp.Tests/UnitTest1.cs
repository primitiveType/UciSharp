using System.Text.RegularExpressions;

namespace UciSharp.Tests;

public class Tests
{
    private readonly string path = "C:\\Users\\Arthu\\Downloads\\arasan23.5\\arasanx-64.exe";
    private ChessEngine _engine;

    [SetUp]
    public async Task Setup()
    {
        _engine = new(path);
        await _engine.StartAsync();
        await _engine.WaitForReadyAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _engine.WaitForReadyAsync();
        await _engine.DisposeAsync();
    }


    [Test]
    public async Task TestChessEngineOptionsPopulated()
    {
        Assert.That(_engine.AvailableOptions, Has.Count.GreaterThan(0));
    }
    [Test]
    public async Task TestChessEngineGame()
    {
        //TODO: set up some abstraction for getting a response back somewhat reliably for a particular command.
        //The readyok pattern is ok, but needs to be re-usable so I don't have to set up TCS's all the time.
        //I don't think I can guarantee anything about multiple commands of the same type, but that shouldn't be a real concern.
        await _engine.StartGameAsync();
        await _engine.GoAsync();
        await _engine.GoAsync();
        await _engine.GoAsync();
        await _engine.GoAsync();
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
}
