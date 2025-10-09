namespace LeForg.Clarp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// A class for parsing command-line arguments, flags, options, and passthrough arguments.
/// It processes the raw arguments provided via `Environment.GetCommandLineArgs` and categorizes them into
/// different groups such as arguments, flags, options, and passthrough arguments.
/// </summary>
public class ArgParser
{
    /// <summary>
    /// Gets the list of positional arguments provided in the command-line input.
    /// </summary>
    public List<string> Args { get; private set; }

    /// <summary>
    /// Gets the list of flags (single-dash options) provided in the command-line input.
    /// </summary>
    public List<string> Flags { get; private set; }

    /// <summary>
    /// Gets the dictionary of options (double-dash options with associated values) 
    /// provided in the command-line input.
    /// </summary>
    public Dictionary<string, string> Options { get; private set; }

    /// <summary>
    /// Gets the list of passthrough arguments (arguments after `--`) provided 
    /// in the command-line input.
    /// </summary>
    public List<string> Passthrough { get; private set; }

    /// <summary>
    /// Gets the raw list of command-line arguments, excluding the program name.
    /// </summary>
    public List<string> Raw { get; private set; }

    /// <summary>
    /// Gets the pipe character used to separate arguments and options from the passthrough arguments.
    /// </summary>
    public string Pipe { get; private set; }

    /// <summary>
    /// Constructs an instance of `ArgParser` and initializes the internal properties.
    /// Automatically parses the raw arguments provided via `Environment.GetCommandLineArgs`.
    /// </summary>
    public ArgParser()
    {
        Args = [];
        Flags = [];
        Options = [];
        Passthrough = [];
        Raw = [.. Environment.GetCommandLineArgs().Skip(1)];

        using (var reader = new StreamReader(Console.OpenStandardInput()))
        {
            Pipe = reader.ReadToEnd().Trim();
        }

        Parse();
    }

    /// <summary>
    /// Parses the raw arguments and categorizes them into different groups.
    /// - Arguments (positional parameters)
    /// - Flags (single-dash options)
    /// - Options (double-dash options, followed by a value)
    /// - Passthrough arguments (arguments after `--`)
    /// </summary>
    private void Parse()
    {
        bool option = false;
        string key = string.Empty;
        bool passthrough = false;

        foreach (var arg in Raw)
        {
            if (passthrough)
            {
                Passthrough.Add(arg);
            }
            else if (option)
            {
                Options[key] = arg;
                option = false;
            }
            else if (arg == "--")
            {
                passthrough = true;
            }
            else if (arg.StartsWith("--"))
            {
                if (arg.Contains('='))
                {
                    var parts = arg.Split('=', 2);
                    key = parts[0][2..];
                    var val = parts[1];
                    Options[key] = val;
                }
                else
                {
                    key = arg[2..];
                    option = true;
                }
            }
            else if (arg.StartsWith("-"))
            {
                Flags.Add(arg[1..]);
            }
            else
            {
                Args.Add(arg);
            }
        }
    }

    public override string ToString()
    {
        return $"Args: {string.Join(", ", Args)}, \nFlags: {string.Join(", ", Flags)}, \nOptions: {string.Join(", ", Options)}, \nPassthrough: {string.Join(", ", Passthrough)}, \nPipe: \n{Pipe}";
    }
}
