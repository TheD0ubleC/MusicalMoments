using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class MiniDsl
    {
        static Dictionary<string, object> Vars = new();
        static Dictionary<string, Action<List<object>>> Builtins =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["MessageBox"] = args =>
                {
                    string text = args.Count > 0 ? args[0]?.ToString() : "";
                    string caption = args.Count > 1 ? args[1]?.ToString() : "";
                    MessageBox.Show(text, caption);
                },

                ["OpenSettings"] = args =>
                {
                    var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["sound"] = "ms-settings:sound",
                        ["display"] = "ms-settings:display",
                        ["network"] = "ms-settings:network"
                    };

                    if (map.TryGetValue(args[0].ToString(), out var uri))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = uri,
                            UseShellExecute = true
                        });
                    }
                },


                ["OpenUrl"] = args =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = args[0].ToString(),
                        UseShellExecute = true
                    });
                },

                ["Print"] = args =>
                {
                    Console.WriteLine(args[0]);
                }
            };
        public static void Execute(string script)
        {
            Vars.Clear();

            var lines = script.Split(';',
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;

                if (line.Contains("="))
                {
                    HandleAssignment(line);
                }
                else
                {
                    HandleCall(line);
                }
            }
        }

        static void HandleAssignment(string line)
        {
            var parts = line.Split('=', 2);
            string name = parts[0].Trim();
            string expr = parts[1].Trim();

            Vars[name] = Eval(expr);
        }
        static void HandleCall(string line)
        {
            var m = Regex.Match(line, @"^(\w+)\((.*)\)$");
            if (!m.Success) return;

            string func = m.Groups[1].Value;
            string argText = m.Groups[2].Value;

            if (!Builtins.TryGetValue(func, out var action))
                return;

            var args = new List<object>();

            if (!string.IsNullOrWhiteSpace(argText))
            {
                foreach (var p in SplitArgs(argText))
                {
                    args.Add(Eval(p));
                }
            }

            action(args);
        }

        static object Eval(string text)
        {
            text = text.Trim();


            if (text.StartsWith("'") && text.EndsWith("'"))
                return text[1..^1];

            if (Vars.TryGetValue(text, out var v))
                return v;


            if (double.TryParse(text, out double n))
                return n;

            foreach (var kv in Vars)
            {
                text = Regex.Replace(text,
                    $@"\b{kv.Key}\b",
                    kv.Value.ToString());
            }

            return new DataTable().Compute(text, null);
        }

        static List<string> SplitArgs(string input)
        {
            List<string> result = new();
            int depth = 0;
            bool str = false;
            string cur = "";

            foreach (char c in input)
            {
                if (c == '\'') str = !str;

                if (c == ',' && depth == 0 && !str)
                {
                    result.Add(cur);
                    cur = "";
                    continue;
                }

                if (c == '(') depth++;
                if (c == ')') depth--;

                cur += c;
            }

            if (cur.Length > 0)
                result.Add(cur);

            return result;
        }
    }
}
