using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace DSRSTests
{
    internal class IsoSec2CSVisitor : IsoSecBaseVisitor<string>
    {
        public override string VisitProgram([NotNull] IsoSecParser.ProgramContext context)
        {
            string output = "";
            foreach (var d in context.decLine())
            {
                output += $"{Visit(d)}\n";
            }
            output +="\n";
            foreach (var f in context.funcDeclaration())
            {
                output += $"{Visit(f)}\n";
            }
            return output;
        }

        public override string VisitDecLine([NotNull] IsoSecParser.DecLineContext context)
        {
            return $"{Visit(context.dec)};";
        }

        public override string VisitDeclarationBase([NotNull] IsoSecParser.DeclarationBaseContext context)
        {
            return $"{context.type.GetText()} {context.name.Text}";
        }

        public override string VisitDeclaration([NotNull] IsoSecParser.DeclarationContext context)
        {
            string output = $"{Visit(context.baseDec)}";
            output += context.value != null ? $" = {Visit(context.value)}" : "";
            return output;
        }

        public override string VisitFuncDeclaration([NotNull] IsoSecParser.FuncDeclarationContext context)
        {
            var decArgs = context.declararionArgs();

            string lines = string.Join("\n    ",context.line().Select(Visit));
            string args = decArgs != null ? Visit(decArgs) : "";
            return $"{context.type.GetText()} {context.name.Text} ({args}) {{\n    {lines}\n}}";
        }
        public override string VisitReturnStat([NotNull] IsoSecParser.ReturnStatContext context)
        {
            return $"return {Visit(context.exp())};";
        }
        public override string VisitDeclararionArgs([NotNull] IsoSecParser.DeclararionArgsContext context)
        {
            return string.Join(", ",context.declaration().Select(Visit));
        }

        public override string VisitLine([NotNull] IsoSecParser.LineContext context)
        {
            return $"{Visit(context.lstat)}";
        }
        public override string VisitCtrlStat([NotNull] IsoSecParser.CtrlStatContext context)
        {
            return $"{VisitChildren(context).Replace("\n", "\n    ")}";
        }
        public override string VisitDecStat([NotNull] IsoSecParser.DecStatContext context)
        {
            return $"{VisitChildren(context)};";
        }
        public override string VisitExpStat([NotNull] IsoSecParser.ExpStatContext context)
        {
            return $"{VisitChildren(context)};";
        }
        public override string VisitTerminal(ITerminalNode node)
        {
            return node.Symbol.Text;
        }
        public override string VisitExp([NotNull] IsoSecParser.ExpContext context)
        {
            if (context.op != null)
            {
                string op = context.op != null ? context.op.Text : "";
                string left = context.left != null ? Visit(context.left) : "";
                string right = context.right != null ? Visit(context.right) : "";

                if (context.right == null) // Prefix
                    return $"{left}{op}";
                else if (context.left == null) // Postfix
                    return $"{op}{right}";
                else if (context.op.Type == IsoSecLexer.AssignOp) // Assign
                    return $"{left} {op} {right}";
                else if (context.op.Type == IsoSecLexer.PowerOp) // Power
                    return $"Math.Pow({left}, {right})";
                else // Default Operation
                    return $"{left} {op} {right}";
            }
            else if (context.type != null)
            {
                return $"({context.type.GetText()}){Visit(context.right)}";
            }
            else if (context.left != null)
            {
                return $"({Visit(context.left)})";
            }
            else
                return VisitChildren(context);
        }
        public override string VisitCompVar([NotNull] IsoSecParser.CompVarContext context)
        {
            if (context.at == null)
                return $"{Visit(context.left)}.{Visit(context.right)}";
            else
                return Visit(context.at);
        }
        public override string VisitAtom([NotNull] IsoSecParser.AtomContext context)
        {
            return VisitChildren(context);
        }
        public override string VisitVar([NotNull] IsoSecParser.VarContext context)
        {
            var argExps = context.args();
            string args = argExps != null?$"[{Visit(argExps)}]":"";
            return $"{context.name.Text}{args}";
        }
        public override string VisitFunc([NotNull] IsoSecParser.FuncContext context)
        {
            var argExps = context.args();
            string args = argExps != null ? $"{Visit(argExps)}" : "";
            return $"{context.name.Text}({args})";
        }
        public override string VisitArgs([NotNull] IsoSecParser.ArgsContext context)
        {

            return string.Join(", ", context.exp().Select(Visit));
        }

        public override string VisitConst([NotNull] IsoSecParser.ConstContext context)
        {
            return context.GetText();
        }
        public override string VisitNewObj([NotNull] IsoSecParser.NewObjContext context)
        {
            var args = context.args();
            return $"new {context.typeName().GetText()}({(args!=null?Visit(args):"")})";
        }

        public override string VisitIf([NotNull] IsoSecParser.IfContext context)
        {

            string lines = string.Join("\n    ",context.line().Select(Visit));
            string output = $"if ({Visit(context.cond)}) {{\n    {lines}\n}}";

            foreach (var ei in context.ifElse())
            {
                string elseIfLines = string.Join("\n    ", ei.line().Select(Visit));
                output += $"else if ({Visit(ei.cond)}) {{\n    {elseIfLines}\n}}";
            }
            var @else = context.@else();
            if (@else != null)
            {
                string elseLines = string.Join("\n    ", @else.line().Select(Visit));
                output += $"else {{\n    {elseLines}\n}}";
            }
            return output;
        }
        public override string VisitWhile([NotNull] IsoSecParser.WhileContext context)
        {
            string lines = string.Join("\n    ", context.line().Select(Visit));
            return $"while ({Visit(context.cond)}) {{\n    {lines}\n}}"; ;
        }
        public override string VisitDoWhile([NotNull] IsoSecParser.DoWhileContext context)
        {
            string lines = string.Join("\n    ", context.line().Select(Visit));
            return $"do {{\n    {lines}\n}} while ({Visit(context.cond)});";
        }
        public override string VisitFor([NotNull] IsoSecParser.ForContext context)
        {
            string lines = string.Join("\n    ", context.line().Select(Visit));
            return $"for ({Visit(context.dvar != null ? context.dvar : context.ivar)}; {Visit(context.cond)}; {Visit(context.iexp)}) {{\n    {lines}\n}}"; ;
        }

        public override string VisitChildren(IRuleNode node)
        {
            string output = "";
            int count = node.ChildCount;
            for (int i = 0; i < count; i++)
                output += Visit(node.GetChild(i));

            return output;
        }
    }
}
