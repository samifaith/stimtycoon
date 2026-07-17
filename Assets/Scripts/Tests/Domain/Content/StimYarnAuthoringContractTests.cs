using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace StimTycoon.Tests.Domain.Content
{
    [Category("ContentContract")]
    public sealed class StimYarnAuthoringContractTests
    {
        private const string EventsPath = "Assets/Dialogue/Events";
        private static readonly Regex NodeTitle = new Regex(
            @"(?m)^title:\s*(?<id>\S+)\s*$", RegexOptions.Compiled);
        private static readonly Regex ChoiceTag = new Regex(
            @"#choice:(?<id>[a-z0-9_]+)", RegexOptions.Compiled);
        private static readonly Regex ResolveCommand = new Regex(
            @"<<stim_resolve_choice\s+""(?<event>[a-z0-9_]+)""\s+""(?<choice>[a-z0-9_]+)""\s*>>",
            RegexOptions.Compiled);

        [Test]
        public void EveryYarnNodeAndResolveEventIdIsUnique()
        {
            var nodeOwners = new Dictionary<string, string>(StringComparer.Ordinal);
            var eventOwners = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var node in ReadNodes())
            {
                AssertUnique(nodeOwners, node.nodeId, node.owner, "Yarn node title");
                AssertUnique(eventOwners, node.eventId, node.owner, "resolved event id");
            }
        }

        [Test]
        public void EveryTaggedChoiceResolvesExactlyOnceWithinItsNode()
        {
            foreach (var node in ReadNodes())
            {
                CollectionAssert.AreEquivalent(node.choiceTags, node.resolvedChoices,
                    $"{node.owner} must keep #choice tags and stim_resolve_choice commands in exact parity.");
                Assert.That(node.choiceTags, Has.Count.GreaterThanOrEqualTo(2),
                    $"{node.owner} must offer at least two authored choices.");
                Assert.That(node.choiceTags.Distinct(StringComparer.Ordinal).Count(),
                    Is.EqualTo(node.choiceTags.Count), $"{node.owner} contains a duplicate #choice id.");
            }
        }

        [Test]
        public void EveryNodeResolvesOneLocalizationSafeEventId()
        {
            foreach (var node in ReadNodes())
            {
                Assert.That(node.eventId, Does.Match("^[a-z0-9_]+$"),
                    $"{node.owner} resolves an event id that is not localization-key safe.");
                Assert.That(node.resolvedEventIds, Has.Count.EqualTo(1),
                    $"{node.owner} must not resolve choices against multiple event definitions.");
            }
        }

        private static IReadOnlyList<YarnNodeContract> ReadNodes()
        {
            Assert.That(Directory.Exists(EventsPath), Is.True,
                $"Yarn event directory was not found at {EventsPath}.");

            var nodes = new List<YarnNodeContract>();
            foreach (var path in Directory.GetFiles(EventsPath, "*.yarn", SearchOption.AllDirectories)
                         .OrderBy(value => value, StringComparer.Ordinal))
            {
                var source = File.ReadAllText(path);
                var blocks = Regex.Split(source, @"(?m)^===\s*$");
                for (var index = 0; index < blocks.Length; index++)
                {
                    var block = blocks[index];
                    if (string.IsNullOrWhiteSpace(block)) continue;
                    var title = NodeTitle.Match(block);
                    Assert.That(title.Success, Is.True, $"{path} block {index + 1} has no title.");

                    var commands = ResolveCommand.Matches(block).Cast<Match>().ToList();
                    Assert.That(commands, Is.Not.Empty,
                        $"{path} node {title.Groups["id"].Value} has no stim_resolve_choice command.");
                    var eventIds = commands.Select(match => match.Groups["event"].Value)
                        .Distinct(StringComparer.Ordinal).ToList();
                    nodes.Add(new YarnNodeContract(
                        $"{path}::{title.Groups["id"].Value}",
                        title.Groups["id"].Value,
                        eventIds,
                        ChoiceTag.Matches(block).Cast<Match>()
                            .Select(match => match.Groups["id"].Value).ToList(),
                        commands.Select(match => match.Groups["choice"].Value).ToList()));
                }
            }

            Assert.That(nodes, Is.Not.Empty, "No authored Yarn event nodes were discovered.");
            return nodes;
        }

        private static void AssertUnique(
            IDictionary<string, string> owners,
            string id,
            string owner,
            string label)
        {
            Assert.That(owners.TryGetValue(id, out var existingOwner), Is.False,
                $"{label} '{id}' is owned by both {existingOwner} and {owner}.");
            owners.Add(id, owner);
        }

        private sealed class YarnNodeContract
        {
            public YarnNodeContract(
                string owner,
                string nodeId,
                IReadOnlyList<string> resolvedEventIds,
                IReadOnlyList<string> choiceTags,
                IReadOnlyList<string> resolvedChoices)
            {
                this.owner = owner;
                this.nodeId = nodeId;
                this.resolvedEventIds = resolvedEventIds;
                this.choiceTags = choiceTags;
                this.resolvedChoices = resolvedChoices;
            }

            public string owner { get; }
            public string nodeId { get; }
            public string eventId => resolvedEventIds[0];
            public IReadOnlyList<string> resolvedEventIds { get; }
            public IReadOnlyList<string> choiceTags { get; }
            public IReadOnlyList<string> resolvedChoices { get; }
        }
    }
}
