﻿using DG.Sculpt.Cron.FieldInternals;
using FluentAssertions;
using Xunit;

namespace DG.Sculpt.Tests.Cron.FieldInternals
{
    public class CronRangeTests
    {
        private readonly static CronValueParser _parser = new CronValueParser("test-field", 1, 10, 2, "ONE", "TWO");

        [Theory]
        [InlineData(2, null, null, "2")]
        [InlineData(null, null, null, "*")]
        [InlineData(2, 5, null, "2-5")]
        [InlineData(2, 5, 4, "2-5/4")]
        [InlineData(null, null, 4, "*/4")]
        public void ToString_ReturnsCorrectValues(int? start, int? end, int? stepSize, string expected)
        {
            var cronRange = new CronRange(new CronValue(start), new CronValue(end), stepSize);

            string result = cronRange.ToString();

            result.Should().Be(expected);
        }

        [Fact]
        public void TryParse_Asterisk_ReturnsAny()
        {
            string s = "*";
            var result = CronRange.TryParse(s, _parser);

            result.HasResult.Should().BeTrue();
            var actual = result.GetResultOrThrow();
            actual.IsWildcard.Should().BeTrue();
        }

        [Theory]
        [InlineData("5", "5")]
        [InlineData("2-5", "2-5")]
        [InlineData("2/4", "2-10/4")] //if step value exists but range end doesn't, range end gets set to max.
        [InlineData("2-5/4", "2-5/4")]
        [InlineData("*/4", "*/4")]
        [InlineData("ONE-TWO", "ONE-TWO")]
        [InlineData("ONE", "ONE")]
        public void TryParse_Works(string input, string expected)
        {
            var result = CronRange.TryParse(input, _parser);

            result.HasResult.Should().BeTrue();
            var actual = result.GetResultOrThrow();
            actual.ToString().Should().Be(expected);
        }
        [Theory]
        [InlineData("5-")]
        [InlineData("*/")]
        [InlineData("2-5/ONE")]
        [InlineData("2-5/0")]
        [InlineData("2-5/13")]
        public void TryParse_Throws(string input)
        {
            var result = CronRange.TryParse(input, _parser);

            result.HasResult.Should().BeFalse();
        }

        [Theory]
        [InlineData("*", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
        [InlineData("3", 3)]
        [InlineData("3-5", 3, 4, 5)]
        [InlineData("9-2", 9, 10, 1, 2)]
        [InlineData("*/2", 1, 3, 5, 7, 9)]
        [InlineData("2/2", 2, 4, 6, 8, 10)]
        [InlineData("2-6/2", 2, 4, 6)]
        [InlineData("2-6/3", 2, 5)]
        [InlineData("9-3/2", 9, 1, 3)]
        public void GetAllowedValues_Works(string range, params int[] expected)
        {
            var parsedRange = CronRange.TryParse(range, _parser).GetResultOrThrow();

            var allowed = parsedRange.GetAllowedValues(_parser.Min, _parser.Max);

            allowed.Should().Equal(expected);
        }
    }
}