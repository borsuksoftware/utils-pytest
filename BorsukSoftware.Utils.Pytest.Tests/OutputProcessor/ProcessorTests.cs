using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace BorsukSoftware.Utils.Pytest.OutputProcessor
{
    public class ProcessorTests
    {

        public static IEnumerable<object[]> ProcessLogOutputTest_Data
        {
            get
            {
                var assembly = typeof(ProcessorTests).Assembly;

                var regex = new System.Text.RegularExpressions.Regex("OutputProcessor\\.Resources\\.(?<label>[a-z]+)\\.Input\\.txt", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var allResourceNames = assembly.GetManifestResourceNames();

                var resourceNames = assembly.GetManifestResourceNames().
                    Select(name => new { name, match = regex.Match(name) }).
                    Where(tuple => tuple.match.Success).
                    Select(tuple => {
                        var expectedResultsResourceName = allResourceNames.Single(n => n.EndsWith($"OutputProcessor.Resources.{tuple.match.Groups["label"]}.expected.json"));

                        TestResult[] expectedResults;
                        using (var s = assembly.GetManifestResourceStream(expectedResultsResourceName))
                        {
                            expectedResults = System.Text.Json.JsonSerializer.Deserialize<TestResult[]>(s);
                        }

                        return new { tuple.match, tuple.name, expectedResults };
                    });

                foreach (var tuple in resourceNames)
                {
                    yield return new object[]
                    {
                        tuple.match.Groups["label"].Value,
                        assembly.GetManifestResourceStream(tuple.name),
                        tuple.expectedResults
                    };
                }
            }
        }

        [MemberData(nameof(ProcessLogOutputTest_Data))]
        [Theory]
        public void ProcessLogOutputTest(string label, System.IO.Stream sourceStream, IReadOnlyCollection<TestResult> expectedResults)
        {
            using (var streamReader = new System.IO.StreamReader(sourceStream))
            {
                var processor = new Processor();
                var output = processor.ProcessLogOutput(streamReader);

                output.Should().BeEquivalentTo(expectedResults);
            }
        }
    }
}
