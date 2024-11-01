using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DSRSTests;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.CommandLine;
using System.Reflection;


internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Argument<FileInfo> sourceFile = new("--source", "The IsoSec source file to process");
        Option<FileInfo?> csOutput = new(new string[] { "--parse", "-p" }, "The file with a CSharp equivalent code");
        Option<bool> showCS = new(new string[] { "--show-cs", "-s" }, () => false, "Shows the CSharp equivalent code");
        Option<string[]> csUsing = new(new string[] { "--using", "-u" }, () => new string[] { "System","System.Collections", "System.Collections.Generic" }, "The namespaces that CSharp compiler will use");
        Argument<List<string>> progArgs = new("--args",() => new(), "The arguments passed to the IsoSec program");

        RootCommand cmd = new();

        cmd.AddArgument(sourceFile);
        cmd.AddOption(csOutput);
        cmd.AddOption(showCS);
        cmd.AddOption(csUsing);
        cmd.AddArgument(progArgs);

        cmd.SetHandler(CompileSource,sourceFile, csOutput, showCS, csUsing, progArgs);

        return await cmd.InvokeAsync(args);
    }
    static void CompileSource(FileInfo file, FileInfo? csFile,bool showCS, string[] usings, List<string> args)
    {
        string source = File.ReadAllText(file.FullName);

        ICharStream code = CharStreams.fromString(source);
        ITokenSource lexer = new IsoSecLexer(code);
        ITokenStream tokens = new CommonTokenStream(lexer);
        IsoSecParser parser = new(tokens);
        IsoSec2CSVisitor visitor = new();
        string csCode = visitor.Visit(parser.program());
        string headerStr = string.Join("\n", usings.Select((u) => $"using {u};"));
        string fullCSCode = $"{headerStr}\n\n{csCode}";
        
        if (showCS)
        {
            Console.WriteLine(fullCSCode);
        }

        if (csFile != null)
        {
            Console.WriteLine(fullCSCode);
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