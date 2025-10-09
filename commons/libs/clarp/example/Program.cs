namespace LeForg.Clarp.Tests;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing arguments");

        string[] a = Environment.GetCommandLineArgs();
        foreach (var arg in a)
        {
            Console.WriteLine($"Argument: {arg}");
        }

        ArgParser p = new();

        Console.WriteLine(p.ToString());

        Console.WriteLine("Testing arguments");

        if (!p.Args.Contains("arg1")) throw new Exception("Error: 'arg1' is not present in the arguments.");
        if (!p.Args.Contains("arg2")) throw new Exception("Error: 'arg2' is not present in the arguments.");
        if (p.Args.Count != 2) throw new Exception($"Error: Expected 2 arguments, but found {p.Args.Count}.");

        Console.WriteLine("Testing flags");

        if (!p.Flags.Contains("flag1")) throw new Exception("Error: 'flag1' is not present in the flags.");
        if (!p.Flags.Contains("flag2")) throw new Exception("Error: 'flag2' is not present in the flags.");
        if (p.Flags.Count != 2) throw new Exception($"Error: Expected 2 flags, but found {p.Flags.Count}.");

        Console.WriteLine("Testing options");

        if (p.Options["option1"] != "option1 value") throw new Exception("Error: 'option1' does not have the expected value.");
        if (p.Options["option2"] != "this is also valid") throw new Exception("Error: 'option2' does not have the expected value.");
        if (p.Options.Count != 2) throw new Exception($"Error: Expected 2 options, but found {p.Options.Count}.");

        Console.WriteLine("Testing passthrough arguments");

        if (string.Join(" ", p.Passthrough) != "passthrough arguments are useful") throw new Exception("Error: Passthrough arguments are not as expected.");
        if (p.Passthrough.Count != 3) throw new Exception($"Error: Expected 3 passthrough arguments, but found {p.Passthrough.Count}.");

        Console.WriteLine("Testing pipe output");

        if (p.Pipe != "Hello, World!") throw new Exception("Error: Pipe output is not as expected.");

        Console.WriteLine("All tests passed.");

    }
}
