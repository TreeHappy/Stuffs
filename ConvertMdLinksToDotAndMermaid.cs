
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string rootDir = Directory.GetCurrentDirectory();
        
        // Get all .md files with relative paths
        var mdFiles = Directory.EnumerateFiles(rootDir, "*.md", SearchOption.AllDirectories)
                              .Select(f => Path.GetRelativePath(rootDir, f))
                              .ToList();

        var connections = new HashSet<(string Source, string Target)>();

        foreach (var file in mdFiles)
        {
            string source = Path.ChangeExtension(file, null); // Remove .md
            string content = File.ReadAllText(Path.Combine(rootDir, file));

            // Match [[wikilinks]] or (regular.md) links
            var matches = Regex.Matches(content, @"\[\[(.*?)\]\]|\((.*?\.md)\)");
            
            foreach (Match match in matches)
            {
                string target = match.Groups[1].Success ? 
                    match.Groups[1].Value : 
                    match.Groups[2].Value;

                if (string.IsNullOrEmpty(target)) continue;

                // Normalize target path
                if (target.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    target = target[..^3]; // Remove .md

                if (source != target)
                    connections.Add((source, target));
            }
        }

        // Generate Graphviz DOT file
        File.WriteAllLines("graph.dot", 
            new[] { "digraph {" }
                .Concat(connections.Select(c => $"  \"{c.Source}\" -> \"{c.Target}\""))
                .Append("}")
        );

        // Generate Mermaid file
        File.WriteAllLines("graph.mmd", 
            new[] { "graph LR" }
                .Concat(connections.Select(c => 
                    $"  {SanitizeMermaid(c.Source)} --> {SanitizeMermaid(c.Target)}"))
        );

        Console.WriteLine("Generated graph.dot and graph.mmd");
    }

    static string SanitizeMermaid(string input) => 
        input.Replace(" ", "_")
             .Replace(".", "_")
             .Replace("/", "_")
             .Replace("\\", "_");
}

