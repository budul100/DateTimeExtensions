using NUnit.Framework;
using System;
using DateTimeExtensions;

namespace DateTimeExtensionsTests
{
    public class Tests
    {
        [Test]
        public void MultiDayCycleDates()
        {
            var cycle = new DateTime[] { new DateTime(2020, 1, 10), new DateTime(2020, 1, 16) };

            var inCycle = new DateTime(2020, 1, 12).GetCyclic(cycle);
            Assert.True(inCycle == new DateTime(2020, 1, 12));

            var atBorder = new DateTime(2020, 1, 10).GetCyclic(cycle);
            Assert.True(atBorder == new DateTime(2020, 1, 10));

            var leftAtBorder = new DateTime(2020, 1, 2).GetCyclic(cycle);
            Assert.True(leftAtBorder == new DateTime(2020, 1, 16));

            var leftToCycle = new DateTime(2020, 1, 3).GetCyclic(cycle);
            Assert.True(leftToCycle == new DateTime(2020, 1, 10));

            var rightToCycle = new DateTime(2020, 1, 30).GetCyclic(cycle);
            Assert.True(rightToCycle == new DateTime(2020, 1, 16));
        }

        [Test]
        public void SingleDayCycleDates()
        {
            var cycle = new DateTime[] { new DateTime(2020, 1, 10)};

            var outCycle = new DateTime(2020, 1, 12).GetCyclic(cycle);
            Assert.True(outCycle == new DateTime(2020, 1, 10));

            var inCycle = new DateTime(2020, 1, 10).GetCyclic(cycle);
            Assert.True(inCycle == new DateTime(2020, 1, 10));
        }
    }
}