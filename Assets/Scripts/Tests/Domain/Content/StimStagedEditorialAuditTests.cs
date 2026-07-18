using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using StimTycoon.Events;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Content
{
    [Category("ContentContract")]
    public sealed class StimStagedEditorialAuditTests
    {
        private const string EventsPath = "Assets/Dialogue/Events";
        private static readonly string[] BatchFiles =
        {
            "ChildhoodBatch20.yarn", "SchoolBatch20.yarn", "CareerBatch20.yarn",
            "HealthBatch20.yarn", "MoneyBatch20.yarn"
        };
        private static readonly string[] DraftMarkers =
        {
            "lorem ipsum", "placeholder", "todo", "tbd", "fixme", "write copy"
        };
        private static readonly Regex Title = new Regex(
            @"(?m)^title:\s*(?<id>[a-z0-9_]+)\s*$", RegexOptions.Compiled);
        private static readonly Regex Tags = new Regex(
            @"(?m)^tags:\s*(?<tags>[^\r\n]+)$", RegexOptions.Compiled);
        private static readonly Regex Choice = new Regex(
            @"(?m)^->\s*(?<label>.+?)\s+#choice:(?<id>[a-z0-9_]+)\s*\r?\n\s+(?<result>[^\r\n]+)",
            RegexOptions.Compiled);

        [Test]
        public void StagedYarnCopy_IsOriginalCompleteAndPublicationReady()
        {
            var nodes = ReadStagedNodes();

            Assert.That(nodes, Has.Count.EqualTo(100));
            Assert.That(nodes.Select(node => node.id).Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(100));
            Assert.That(nodes.Select(node => node.body).Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(100),
                "Every staged event needs distinct authored setup copy.");
            Assert.That(nodes.SelectMany(node => node.results).Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(200), "Every staged choice needs distinct authored result copy.");

            foreach (var node in nodes)
            {
                Assert.That(WordCount(node.body), Is.GreaterThanOrEqualTo(8),
                    $"{node.id} setup copy is too thin for editorial review.");
                Assert.That(EndsAsSentence(node.body), Is.True, $"{node.id} setup copy needs sentence punctuation.");
                Assert.That(node.tags, Does.Contain("representative_event"));
                Assert.That(node.tags, Does.Contain(node.id.Substring(0, node.id.IndexOf('_'))),
                    $"{node.id} must carry its category tag.");
                Assert.That(node.choiceIds, Has.Count.EqualTo(2));
                Assert.That(node.choiceIds.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(2));
                Assert.That(node.labels.Distinct(StringComparer.OrdinalIgnoreCase).Count(), Is.EqualTo(2));
                foreach (var result in node.results)
                {
                    Assert.That(WordCount(result), Is.GreaterThanOrEqualTo(5),
                        $"{node.id} result copy is too thin for editorial review.");
                    Assert.That(EndsAsSentence(result), Is.True,
                        $"{node.id} result copy needs sentence punctuation.");
                }
                AssertNoDraftMarkers(node.id, node.body);
                foreach (var text in node.labels.Concat(node.results)) AssertNoDraftMarkers(node.id, text);
            }
        }

        [Test]
        public void StagedCatalog_UsesCategoryCorrectFeedLanguageAndDistinctConsequences()
        {
            foreach (var evt in StagedStimEventCatalog.CreateAllStagedEvents())
            {
                var expectedMoment = ExpectedMoment(evt.category);
                var signatures = new HashSet<string>(StringComparer.Ordinal);
                foreach (var choice in evt.choices)
                foreach (var outcome in choice.outcomes)
                {
                    StringAssert.Contains(expectedMoment, outcome.feedEntryKey,
                        $"{evt.id}/{choice.id} leaked another life stage into its feed copy.");
                    var signature = string.Join("|", outcome.effects.Select(effect =>
                        $"{effect.type}:{effect.targetId}:{effect.value}"));
                    Assert.That(signatures.Add(signature), Is.True,
                        $"{evt.id} choices must preview meaningfully distinct consequences.");
                }
            }
        }

        private static IReadOnlyList<EditorialNode> ReadStagedNodes()
        {
            var nodes = new List<EditorialNode>();
            foreach (var file in BatchFiles)
            {
                var source = File.ReadAllText(Path.Combine(EventsPath, file));
                foreach (var block in Regex.Split(source, @"(?m)^===\s*$"))
                {
                    if (string.IsNullOrWhiteSpace(block)) continue;
                    var title = Title.Match(block);
                    var tags = Tags.Match(block);
                    var separator = block.IndexOf("---", StringComparison.Ordinal);
                    var firstChoice = block.IndexOf("\n->", separator, StringComparison.Ordinal);
                    Assert.That(title.Success && tags.Success && separator >= 0 && firstChoice > separator,
                        Is.True, $"{file} contains an incomplete authored node.");
                    var choices = Choice.Matches(block).Cast<Match>().ToList();
                    nodes.Add(new EditorialNode(
                        title.Groups["id"].Value,
                        tags.Groups["tags"].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                        block.Substring(separator + 3, firstChoice - separator - 3).Trim(),
                        choices.Select(match => match.Groups["id"].Value).ToList(),
                        choices.Select(match => match.Groups["label"].Value.Trim()).ToList(),
                        choices.Select(match => match.Groups["result"].Value.Trim()).ToList()));
                }
            }
            return nodes;
        }

        private static string ExpectedMoment(EventCategory category)
        {
            switch (category)
            {
                case EventCategory.Childhood: return "childhood moment";
                case EventCategory.School: return "school moment";
                case EventCategory.Career: return "workplace moment";
                case EventCategory.Health: return "health moment";
                case EventCategory.Money: return "money moment";
                default: throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        private static int WordCount(string text) => Regex.Matches(text, @"\b[\p{L}\p{N}']+\b").Count;
        private static bool EndsAsSentence(string text) => Regex.IsMatch(text, @"[.!?][""']?$");

        private static void AssertNoDraftMarkers(string owner, string text)
        {
            foreach (var marker in DraftMarkers)
                Assert.That(text.IndexOf(marker, StringComparison.OrdinalIgnoreCase), Is.EqualTo(-1),
                    $"{owner} contains draft marker '{marker}'.");
        }

        private sealed class EditorialNode
        {
            public EditorialNode(string id, IReadOnlyList<string> tags, string body,
                IReadOnlyList<string> choiceIds, IReadOnlyList<string> labels,
                IReadOnlyList<string> results)
            {
                this.id = id;
                this.tags = tags;
                this.body = body;
                this.choiceIds = choiceIds;
                this.labels = labels;
                this.results = results;
            }

            public readonly string id;
            public readonly IReadOnlyList<string> tags;
            public readonly string body;
            public readonly IReadOnlyList<string> choiceIds;
            public readonly IReadOnlyList<string> labels;
            public readonly IReadOnlyList<string> results;
        }
    }
}
