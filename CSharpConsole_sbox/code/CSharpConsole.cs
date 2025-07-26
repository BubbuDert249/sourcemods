using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CSharpConsoleAddon
{
    public class CSharpConsole : Panel
    {
        private TextEntry InputEntry;
        private TextPanel OutputPanel;
        private Button RunButton;

        private Dictionary<string, object> variables = new();
        private static readonly Regex varDeclRegex = new(@"^(int|float|string|bool)\s+([a-zA-Z_]\w*)\s*=\s*(.+);$");
        private static readonly Regex assignmentRegex = new(@"^([a-zA-Z_]\w*)\s*=\s*(.+);$");
        private static readonly Regex consoleWriteLineRegex = new(@"^Console\.WriteLine\((.+)\);$");
        private static readonly Regex printRegex = new(@"^print\((.+)\);$");
        private static readonly Regex returnRegex = new(@"^return\s+(.+);$");

        public CSharpConsole()
        {
            StyleSheet.Load("/csharp_console/CSharpConsole.scss");

            // Layout: Vertical panel with OutputPanel (top) and a horizontal panel (bottom) for input + button
            OutputPanel = Add.TextPanel("");
            OutputPanel.AddClass("output");
            OutputPanel.SetProperty("overflow-y", "auto"); // make output scrollable

            var bottomPanel = Add.Panel();
            bottomPanel.AddClass("bottom-panel");
            bottomPanel.Style.FlexDirection = FlexDirection.Row;

            InputEntry = bottomPanel.Add.TextEntry("");
            InputEntry.AddClass("input");
            InputEntry.Hint = "Enter C# code...";
            InputEntry.AllowEmoji = false;

            RunButton = bottomPanel.Add.Button("Run");
            RunButton.AddClass("run-button");
            RunButton.AddEventListener("onclick", () => RunCode());

            InputEntry.OnEnter = RunCode;
        }

        private void RunCode()
        {
            var code = InputEntry.Text.Trim();
            if (string.IsNullOrEmpty(code))
                return;

            var output = Evaluate(code);
            AppendOutput("> " + code + "\n" + output + "\n");
            InputEntry.Text = "";
        }

        private void AppendOutput(string text)
        {
            OutputPanel.Text += text;

            // Scroll to bottom when new output added
            OutputPanel.Style.ScrollTop = OutputPanel.ScrollHeight;
        }

        private string Evaluate(string code)
        {
            // var declaration: int x = 5;
            var m = varDeclRegex.Match(code);
            if (m.Success)
            {
                var type = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                var valueStr = m.Groups[3].Value.Trim();

                object val = null;
                if (!TryParseValue(type, valueStr, out val))
                    return $"Error: cannot parse value '{valueStr}' as {type}";

                variables[name] = val;
                return $"Variable '{name}' declared as {type} with value {val}";
            }

            // assignment: x = 10;
            m = assignmentRegex.Match(code);
            if (m.Success)
            {
                var name = m.Groups[1].Value;
                var valueStr = m.Groups[2].Value.Trim();

                if (!variables.ContainsKey(name))
                    return $"Error: variable '{name}' not declared";

                var oldVal = variables[name];
                var type = oldVal.GetType();

                object val = null;
                if (!TryParseValue(type.Name.ToLower(), valueStr, out val))
                    return $"Error: cannot parse value '{valueStr}' as {type.Name}";

                variables[name] = val;
                return $"Variable '{name}' updated to {val}";
            }

            // Console.WriteLine(...)
            m = consoleWriteLineRegex.Match(code);
            if (m.Success)
            {
                var inner = m.Groups[1].Value;
                var result = EvalExpression(inner);
                return result?.ToString() ?? "null";
            }

            // print(...) alias
            m = printRegex.Match(code);
            if (m.Success)
            {
                var inner = m.Groups[1].Value;
                var result = EvalExpression(inner);
                return result?.ToString() ?? "null";
            }

            // return ...;
            m = returnRegex.Match(code);
            if (m.Success)
            {
                var inner = m.Groups[1].Value;
                var result = EvalExpression(inner);
                return $"Return: {result?.ToString() ?? "null"}";
            }

            // just a variable name
            if (variables.ContainsKey(code.TrimEnd(';')))
            {
                return variables[code.TrimEnd(';')].ToString();
            }

            return "Error: unsupported or invalid command";
        }

        private bool TryParseValue(string type, string valueStr, out object value)
        {
            value = null;
            try
            {
                switch (type)
                {
                    case "int":
                        if (int.TryParse(valueStr, out var i)) { value = i; return true; }
                        break;
                    case "float":
                        if (float.TryParse(valueStr, out var f)) { value = f; return true; }
                        break;
                    case "string":
                        if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
                        {
                            value = valueStr[1..^1];
                            return true;
                        }
                        break;
                    case "bool":
                        if (bool.TryParse(valueStr, out var b)) { value = b; return true; }
                        break;
                }
            }
            catch { }
            return false;
        }

        private object EvalExpression(string expr)
        {
            expr = expr.Trim();

            // If it's a string literal in quotes
            if (expr.StartsWith("\"") && expr.EndsWith("\""))
                return expr[1..^1];

            // Try to parse as int
            if (int.TryParse(expr, out var i))
                return i;

            // Try to parse as float
            if (float.TryParse(expr, out var f))
                return f;

            // bool true/false
            if (expr == "true") return true;
            if (expr == "false") return false;

            // Variable lookup
            if (variables.ContainsKey(expr))
                return variables[expr];

            // Simple math (only +, - with ints)
            var match = Regex.Match(expr, @"^(\w+)\s*([\+\-])\s*(\w+)$");
            if (match.Success)
            {
                var leftStr = match.Groups[1].Value;
                var op = match.Groups[2].Value;
                var rightStr = match.Groups[3].Value;

                if (variables.ContainsKey(leftStr) && int.TryParse(rightStr, out var rightInt))
                {
                    var leftVal = variables[leftStr];
                    if (leftVal is int leftInt)
                    {
                        return op == "+" ? leftInt + rightInt : leftInt - rightInt;
                    }
                }
            }

            return $"Error: cannot evaluate expression '{expr}'";
        }
    }
}
