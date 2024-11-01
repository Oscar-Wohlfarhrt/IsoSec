using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DSRSTests;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.CommandLine;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.Intrinsics.X86;


internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Argument<FileInfo> sourceFile = new("--source", "The IsoSec source file to process");
        Option<FileInfo?> csOutput = new(new string[] { "--parse", "-p" }, "The file with a CSharp equivalent code");
        Option<bool> showCS = new(new string[] { "--show-cs", "-s" }, () => false, "Shows the CSharp equivalent code");
        Option<string[]> csUsing = new(new string[] { "--using", "-u" }, () => new string[] { "System","System.Collections", "System.Collections.Generic" }, "The namespaces that CSharp compiler will use");
        Argument<List<string>> progArgs = new("--args",() => new(), "The arguments passed to the IsoSec program");
        Option<bool> hideInfo = new(new string[] { "--hide-info", "-hi" }, () => false, "Hides command info lines");

        RootCommand cmd = new("Simple IsoSec language transpiller and compiler");

        cmd.AddArgument(sourceFile);
        cmd.AddOption(csOutput);
        cmd.AddOption(showCS);
        cmd.AddOption(csUsing);
        cmd.AddArgument(progArgs);
        cmd.AddOption(hideInfo);

        cmd.SetHandler(CompileSource,sourceFile, csOutput, showCS, csUsing, progArgs, hideInfo);

        return await cmd.InvokeAsync(args);
    }
    static void CompileSource(FileInfo file, FileInfo? csFile,bool showCS, string[] usings, List<string> args, bool hideInfo)
    {
        List<string> usingsList = new(usings);
        string source = File.ReadAllText(file.FullName);
        Regex argsRe = new(@"^\s*(\/\*(?:.*\n)*\*\/|(?:\/{2}.*\n)*)");
        Regex argRe = new(@"(?:\/)*(.*):(.*)");
        Match argsMatch = argsRe.Match(source);
        Dictionary<string,string> fArgs = new();
        if (argsMatch.Success)
        {
            string[] fileArgs = argsMatch.Groups[1].Value.Split("\n",StringSplitOptions.RemoveEmptyEntries);
            foreach (string arg in fileArgs)
            {
                Match argMatch = argRe.Match(arg);
                if (argMatch.Success)
                {
                    string key = argMatch.Groups[1].Value.Trim();
                    string val = argMatch.Groups[2].Value.Trim();
                    if (!fArgs.ContainsKey(key))
                        fArgs.Add(key, val);
                }
            }

        }
        if (fArgs.ContainsKey("Author") && !hideInfo)
            Console.WriteLine($"Script Author: {fArgs["Author"]}");
        if (fArgs.ContainsKey("Usings"))
            Array.ForEach(fArgs["Usings"].Split(","),
                (u) =>
                {
                    string use = u.Trim();
                    if (!usingsList.Contains(use))
                        usingsList.Add(use);
                });
        if (!hideInfo) {
            Console.WriteLine($"Used libraries: {string.Join(", ", usingsList)}");
            Console.WriteLine("".PadRight(Console.WindowWidth, '-'));
        }

        ICharStream code = CharStreams.fromString(source);
        ITokenSource lexer = new IsoSecLexer(code);
        ITokenStream tokens = new CommonTokenStream(lexer);
        IsoSecParser parser = new(tokens);
        IsoSec2CSVisitor visitor = new();
        string csCode = visitor.Visit(parser.program());
        string headerStr = string.Join("\n", usingsList.Select((u) => $"using {u};"));
        string fullCSCode = $"{headerStr}\n\n{csCode}";
        
        if (showCS)
        {
            Console.WriteLine(fullCSCode);
        }

        if (csFile != null)
        {
            Console.WriteLine($"Writing C# code to '{csFile.FullName}'...");
            File.WriteAllText(csFile.FullName, fullCSCode);
        }
        else
        {
            var options = ScriptOptions.Default;
            options.WithImports(usings);
            Script script = CSharpScript.Create(fullCSCode+"\nAction<string[]> MainMethod = Main;", options);
            var scriptState = script.RunAsync().Result;
            Action<string[]> main = (Action<string[]>)scriptState.GetVariable("MainMethod").Value;
            args.Insert(0,file.FullName);
            main(args.ToArray());
        }
    }
}