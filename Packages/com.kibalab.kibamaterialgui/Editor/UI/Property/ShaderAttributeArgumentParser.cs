#nullable enable

using System;
using System.Collections.Generic;

namespace KIBA_.KIBAMaterialGUI.Editor.UI.Property
{
    public static class ShaderAttributeArgumentParser
    {
        public static string[] Split(string args)
        {
            if (string.IsNullOrWhiteSpace(args)) return Array.Empty<string>();

            var list = new List<string>();
            var start = 0;
            var depth = 0;
            var inSingle = false;
            var inDouble = false;
            for (int i = 0; i < args.Length; i++)
            {
                var c = args[i];
                if (inSingle)
                {
                    if (c == '\'') inSingle = false;
                    else if (c == '\\') i++;
                    continue;
                }

                if (inDouble)
                {
                    if (c == '"') inDouble = false;
                    else if (c == '\\') i++;
                    continue;
                }

                if (c == '\'')
                {
                    inSingle = true;
                    continue;
                }

                if (c == '"')
                {
                    inDouble = true;
                    continue;
                }

                if (c == '(')
                {
                    depth++;
                    continue;
                }

                if (c == ')')
                {
                    if (depth > 0) depth--;
                    continue;
                }

                if (c != ',' || depth != 0) continue;

                list.Add(args.Substring(start, i - start).Trim());
                start = i + 1;
            }

            if (start <= args.Length)
                list.Add(args.Substring(start).Trim());

            return list.Count == 0 ? Array.Empty<string>() : list.ToArray();
        }

        public static string TrimQuotes(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            value = value.Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            {
                return value.Substring(1, value.Length - 2).Trim();
            }

            return value;
        }
    }
}


