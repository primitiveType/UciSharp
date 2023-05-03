namespace UciSharp.Tests;

public class OptionsTests
{
    [TestCase("option name Ponder type check default true", "Ponder", OptionType.Check, "true")]
    [TestCase("option name Hash type spin default 64 min 4 max 64000", "Hash", OptionType.Spin, "64", 4, 64000)]
    [TestCase("option name Contempt type spin default 0 min -200 max 200", "Contempt", OptionType.Spin, "0", -200, 200)]
    [TestCase("option name Use tablebases type check default true", "Use tablebases", OptionType.Check, "true")]
    [TestCase("option name SyzygyTbPath type string default syzygy", "SyzygyTbPath", OptionType.String, "syzygy")]
    [TestCase("option name SyzygyUse50MoveRule type check default true", "SyzygyUse50MoveRule", OptionType.Check, "true")]
    [TestCase("option name SyzygyProbeDepth type spin default 4 min 0 max 64", "SyzygyProbeDepth", OptionType.Spin, "4", 0, 64)]
    [TestCase("option name MultiPV type spin default 1 min 1 max 10", "MultiPV", OptionType.Spin, "1", 1, 10)]
    [TestCase("option name OwnBook type check default true", "OwnBook", OptionType.Check, "true")]
    [TestCase("option name Favor frequent book moves type spin default 50 min 0 max 100", "Favor frequent book moves", OptionType.Spin, "50", 0, 100)]
    [TestCase("option name Favor best book moves type spin default 50 min 0 max 100", "Favor best book moves", OptionType.Spin, "50", 0, 100)]
    [TestCase("option name Favor high-weighted book moves type spin default 100 min 0 max 100", "Favor high-weighted book moves", OptionType.Spin,
        "100", 0, 100)]
    [TestCase("option name Randomize book moves type spin default 50 min 0 max 100", "Randomize book moves", OptionType.Spin, "50", 0, 100)]
    [TestCase("option name Threads type spin default 1 min 1 max 256", "Threads", OptionType.Spin, "1", 1, 256)]
    [TestCase("option name UCI_LimitStrength type check default false", "UCI_LimitStrength", OptionType.Check, "false")]
    [TestCase("option name UCI_Elo type spin default 3300 min 1000 max 3300", "UCI_Elo", OptionType.Spin, "3300", 1000, 3300)]
    [TestCase("option name Use NNUE type check default true", "Use NNUE", OptionType.Check, "true")]
    [TestCase("option name NNUE file type string default arasan-d10-20220723.nnue", "NNUE file", OptionType.String, "arasan-d10-20220723.nnue")]
    [TestCase("option name Move overhead type spin default 30 min 0 max 1000", "Move overhead", OptionType.Spin, "30", 0, 1000)]
    public void TestOptionsRegex(string input, string expectedName, OptionType expectedType, string expectedDefault, int expectedMin = -1,
        int expectedMax = -1)
    {
        Option option = new Option(input);


        Assert.That(option.Name, Is.EqualTo(expectedName));
        Assert.That(option.Type, Is.EqualTo(expectedType));
        Assert.That(option.Default, Is.EqualTo(expectedDefault));
        Assert.That(option.Min, Is.EqualTo(expectedMin));
        Assert.That(option.Max, Is.EqualTo(expectedMax));
    }
}

public class Tests
{
    private readonly OptionsTests _optionsTests = new();
    private readonly string path = "C:\\Users\\Arthu\\Downloads\\arasan23.5\\arasanx-64.exe";
    private ChessEngine _engine = null!;

    [SetUp]
    public async Task SetupAsync()
    {
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        _engine = new ChessEngine(path);
        await _engine.StartAsync();
        await _engine.WaitForReadyAsync();
        
    }

    private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.WriteLine(e.Exception.Message);
        
        Assert.Fail(e.Exception.Message);
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await _engine.WaitForReadyAsync();
        await _engine.DisposeAsync();
    }


    [Test]
    public async Task TestChessEngineOptionsPopulatedAsync()
    {
        Assert.That(await _engine.StartAsync(), Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task TestSetOptionsAsync()
    {
        await _engine.StartAsync();

        await _engine.SetOptions(new List<Option>
        {
            new("Ponder", "false")
        });
    }

    [Test]
    public async Task TestChessEngineGameAsync()
    {
        await _engine.StartGameAsync();
        Console.WriteLine(await _engine.GoAsync());

        await _engine.GoAsync();
        await _engine.GoAsync();
        await _engine.GoAsync();
    }

    [Test]
    [Timeout(10_000)]
    public async Task BufferSurvivesMakingManyMovesAsync()
    {
        string positionAfterFirstMove = @"rnbqkbnr/ppp1pp1p/6p1/3p4/8/PPP5/3PPPPP/RNBQKBNR b KQkq - 0 6";
        // string positionAfterSecondMove = @"rnbqkbnr/pppppppp/8/8/8/P7/1PPPPPPP/RNBQKBNR b KQkq - 0 2";

        for (int i = 0; i < 400; i++)
        {
            await _engine.SetPositionAsync(positionAfterFirstMove);
            await _engine.WaitForReadyAsync();
            var move = await _engine.GoAsync();

            await _engine.SetPositionAsync(positionAfterFirstMove);
            await _engine.WaitForReadyAsync();
            var move2 = await _engine.GoAsync();
            Console.WriteLine("S_________________");

            Console.WriteLine(move);
            Console.WriteLine(move2);
            await _engine.WaitForReadyAsync();
        }
    }
}
