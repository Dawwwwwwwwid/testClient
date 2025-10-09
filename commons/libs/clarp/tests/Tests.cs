namespace LeForg.Clarp.Tests;

// TODO: fix errors when testing, args arent being passed to the testclass

[TestClass]
public sealed class Tests
{
    [TestMethod]
    public void TestArgs()
    {
        Console.WriteLine("Testing arguments");

        string[] args = Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            Console.WriteLine($"Argument: {arg}");
        }

        ArgParser p = new();

        Assert.IsTrue(p.Args.Contains("arg1"));
        Assert.IsTrue(p.Args.Contains("arg2"));
        Assert.AreEqual(2, p.Args.Count);
    }

    [TestMethod]
    public void TestFlags()
    {
        Console.WriteLine("Testing flags");

        ArgParser p = new();

        Assert.IsTrue(p.Flags.Contains("flag1"));
        Assert.IsTrue(p.Flags.Contains("flag2"));
        Assert.AreEqual(2, p.Flags.Count);
    }

    [TestMethod]
    public void TestOptions()
    {
        Console.WriteLine("Testing options");

        ArgParser p = new();

        Assert.AreEqual("option1 value", p.Options["option1"]);
        Assert.AreEqual("this is also valid", p.Options["option2"]);
        Assert.AreEqual(2, p.Options.Count);
    }

    [TestMethod]
    public void TestPassthroughArgs()
    {
        Console.WriteLine("Testing passthrough arguments");

        ArgParser p = new();

        Assert.AreEqual("passthrough arguments are useful", string.Join(" ", p.Passthrough));
        Assert.AreEqual(3, p.Passthrough.Count);
    }

    [TestMethod]
    public void TestPipeOutput()
    {
        Console.WriteLine("Testing pipe output");

        ArgParser p = new();

        Assert.AreEqual("Hello, World!", p.Pipe);
    }
}
