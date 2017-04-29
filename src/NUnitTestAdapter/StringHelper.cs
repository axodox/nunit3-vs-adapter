using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NUnit.VisualStudio.TestAdapter
{
    public static class StringHelper
    {
        private const string _typeArgumentPattern = @"(?><(?>[^<>]*|(?<n><)|(?<-n>>))*>(?(n)(?!)))";

        private static readonly Regex _pathRegex = new Regex(@"(?:^|[.+])(?>[^.+()<>]+|" + _typeArgumentPattern + @"|\((?>[^'""()]|'.'|""(?>[^""]|(?<=\\)"")*"")*\))+", RegexOptions.Compiled);

        private static readonly Regex _typeArgumentRegex = new Regex(_typeArgumentPattern, RegexOptions.Compiled);

        private static readonly Regex _nameRegex = new Regex(@"^[.+]?\w*" + _typeArgumentPattern + @"?", RegexOptions.Compiled);
        
        public static string[] SplitPath(this string path)
        {
            return _pathRegex
              .Matches(path)
              .OfType<Match>()
              .Select(p => p.Value)
              .ToArray();
        }

        public static string FixQualifiedName(this string qualifiedName, string className, string methodName)
        {
            var simplePath = (className + "." + methodName).SplitPath();
            var complexPath = qualifiedName.SplitPath();
            var path = string.Empty;
            for (int i = 0; i < Math.Min(simplePath.Length, complexPath.Length); i++)
            {
                if(complexPath[i].CleanName(true) == simplePath[i].CleanName(true))
                {
                    path += complexPath[i];
                }
                else
                {
                    var remainingPath = string.Join(string.Empty, simplePath.Skip(i).ToArray());
                    var args = string.Join(string.Empty, complexPath.Skip(i).ToArray());
                    if (complexPath.Length > 0 && args.StartsWith("."))
                    {
                        args = args.Substring(1);
                    }
                    path += remainingPath + "(" + args + ")";
                    break;
                }
            }
            return path;
        }

        private static string RemoveGenericEnding(string name)
        {
            if (name.IndexOf('<') != -1)
            {
                return name.Substring(0, name.IndexOf('<'));
            }

            if (name.IndexOf('`') != -1)
            {
                return name.Substring(0, name.IndexOf('`'));
            }

            return name;
        }

        public static string CleanName(this string name, bool removeGenericEnding = false)
        {
            name = _nameRegex.Match(name).Value;
            if (removeGenericEnding && name.IndexOf('<') != -1)
            {
                name = RemoveGenericEnding(name);
            }
            else
            {
                name = _typeArgumentRegex.Replace(name, p => "`" + (p.Value.Where(q => q == ',').Count() + 1));
            }
            return name;
        }
    }
}
