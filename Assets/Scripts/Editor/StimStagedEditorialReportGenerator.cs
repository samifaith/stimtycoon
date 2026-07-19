using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StimTycoon.Events;
using StimTycoon.Runtime;
using UnityEditor;
using UnityEngine;

namespace StimTycoon.Editor
{
    public static class StimStagedEditorialReportGenerator
    {
        private const string RelativeOutputPath = "Artifacts/Content/staged-editorial-review.md";
        private static readonly Regex Title = new Regex(@"(?m)^title:\s*(?<id>[a-z0-9_]+)\s*$");
        private static readonly Regex Tags = new Regex(@"(?m)^tags:\s*(?<tags>[^\r\n]+)$");
        private static readonly Regex Choice = new Regex(
            @"(?m)^->\s*(?<label>.+?)\s+#choice:(?<id>[a-z0-9_]+)\s*\r?\n\s+(?<result>[^\r\n]+)");

        [MenuItem("Tools/Stim Tycoon/Generate Staged Editorial Review")]
        public static void GenerateFromMenu()
        {
            var path = Generate();
            EditorUtility.RevealInFinder(path);
        }

        public static void GenerateForBatchMode()
        {
            Debug.Log($"Generated staged editorial review: {Generate()}");
        }

        public static string Generate()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            var nodes = ReadNodes(Path.Combine(Application.dataPath, "Dialogue", "Events"));
            var catalog = StagedStimEventCatalog.CreateAllStagedEvents()
                .ToDictionary(evt => evt.id, StringComparer.Ordinal);
            if (nodes.Count != 100 || catalog.Count != 100)
                throw new InvalidOperationException(
                    $"Editorial packet requires 100 Yarn nodes and 100 staged events; found {nodes.Count}/{catalog.Count}.");

            var outputPath = Path.Combine(projectRoot, RelativeOutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? projectRoot);
            File.WriteAllText(outputPath, Render(nodes, catalog), new UTF8Encoding(false));
            return outputPath;
        }

        private static string Render(
            IReadOnlyList<ReviewNode> nodes, IReadOnlyDictionary<string, StimEvent> catalog)
        {
            var output = new StringBuilder();
            var effectValues = new StimEffectValueResolver();
            output.AppendLine("# Staged Event Editorial Review");
            output.AppendLine();
            output.AppendLine("Generated evidence only. Every event remains **PENDING** until a human reviewer records all four approvals. Generating this file does not enable staged content.");
            output.AppendLine();
            output.AppendLine($"- Events: {nodes.Count}");
            output.AppendLine($"- Choices/outcomes: {nodes.Sum(node => node.choices.Count)}");
            output.AppendLine($"- Balance profile: `{StimEffectValueResolver.BalanceProfileId}`");
            output.AppendLine("- Required review: tone, age/cultural context, choice clarity, consequence clarity");
            output.AppendLine();
            output.AppendLine("## Balance rules");
            output.AppendLine();
            output.AppendLine("| Rule | Default | Allowed range | Units |");
            output.AppendLine("|---|---:|---:|---|");
            foreach (var rule in StimEffectValueResolver.GetDefinitions())
                output.AppendLine($"| `{rule.id}` | {rule.defaultValue:0.##} | " +
                                  $"{rule.minimumValue:0.##} to {rule.maximumValue:0.##} | " +
                                  $"{(rule.requiresWholeUnits ? "whole" : "fractional")} |");
            output.AppendLine();

            foreach (var node in nodes.OrderBy(item => item.id, StringComparer.Ordinal))
            {
                if (!catalog.TryGetValue(node.id, out var evt))
                    throw new InvalidOperationException($"Yarn node {node.id} has no staged catalog event.");
                output.AppendLine($"## {EscapeHeading(evt.titleKey)} (`{evt.id}`)");
                output.AppendLine();
                output.AppendLine($"- Status: **PENDING**");
                output.AppendLine($"- Category: {evt.category}");
                output.AppendLine($"- Ages: {evt.ageRange.minAge}–{evt.ageRange.maxAge}");
                output.AppendLine($"- Locations: {string.Join(", ", evt.locations)}");
                output.AppendLine($"- Tags: {string.Join(", ", node.tags)}");
                output.AppendLine($"- Setup: {node.body}");
                output.AppendLine("- Approval: [ ] Tone  [ ] Age/cultural context  [ ] Choice clarity  [ ] Consequence clarity");
                output.AppendLine();
                output.AppendLine("| Choice | Authored result | Classification | Consequences | Feed summary |");
                output.AppendLine("|---|---|---|---|---|");
                foreach (var authoredChoice in node.choices)
                {
                    var choice = evt.choices.Single(item => item.id == authoredChoice.id);
                    foreach (var outcome in choice.outcomes)
                    {
                        var effects = string.Join("; ", outcome.effects.Select(effect =>
                            $"{effect.type} {effect.targetId} {Signed(effectValues.Resolve(effect))}" +
                            (string.IsNullOrWhiteSpace(effect.valueRuleId) ? string.Empty :
                                $" ({effect.valueRuleId})")));
                        output.AppendLine($"| {Cell(authoredChoice.label)} | {Cell(authoredChoice.result)} | " +
                                          $"{outcome.classification} | {Cell(effects)} | {Cell(outcome.feedEntryKey)} |");
                    }
                }
                output.AppendLine();
                output.AppendLine("Reviewer: ____________________  Date: __________  Notes: ________________________________");
                output.AppendLine();
            }
            return output.ToString();
        }

        private static List<ReviewNode> ReadNodes(string eventsPath)
        {
            var files = new[] { "ChildhoodBatch20.yarn", "SchoolBatch20.yarn", "CareerBatch20.yarn",
                "HealthBatch20.yarn", "MoneyBatch20.yarn" };
            var nodes = new List<ReviewNode>();
            foreach (var file in files)
            foreach (var block in Regex.Split(File.ReadAllText(Path.Combine(eventsPath, file)), @"(?m)^===\s*$"))
            {
                if (string.IsNullOrWhiteSpace(block)) continue;
                var title = Title.Match(block);
                var tags = Tags.Match(block);
                var separator = block.IndexOf("---", StringComparison.Ordinal);
                var firstChoice = block.IndexOf("\n->", separator, StringComparison.Ordinal);
                if (!title.Success || !tags.Success || separator < 0 || firstChoice <= separator)
                    throw new InvalidOperationException($"{file} contains an incomplete Yarn node.");
                var choices = Choice.Matches(block).Cast<Match>()
                    .Select(match => new ReviewChoice(match.Groups["id"].Value,
                        match.Groups["label"].Value.Trim(), match.Groups["result"].Value.Trim()))
                    .ToList();
                nodes.Add(new ReviewNode(title.Groups["id"].Value,
                    tags.Groups["tags"].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                    block.Substring(separator + 3, firstChoice - separator - 3).Trim(), choices));
            }
            return nodes;
        }

        private static string Signed(float value) => $"{(value >= 0 ? "+" : string.Empty)}{value:0.##}";
        private static string Cell(string value) => value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        private static string EscapeHeading(string value) => value.Replace("#", "\\#");

        private sealed class ReviewNode
        {
            public ReviewNode(string id, IReadOnlyList<string> tags, string body,
                IReadOnlyList<ReviewChoice> choices)
            { this.id = id; this.tags = tags; this.body = body; this.choices = choices; }
            public readonly string id;
            public readonly IReadOnlyList<string> tags;
            public readonly string body;
            public readonly IReadOnlyList<ReviewChoice> choices;
        }

        private sealed class ReviewChoice
        {
            public ReviewChoice(string id, string label, string result)
            { this.id = id; this.label = label; this.result = result; }
            public readonly string id;
            public readonly string label;
            public readonly string result;
        }
    }
}
