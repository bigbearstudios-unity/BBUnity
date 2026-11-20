using NUnit.Framework;

using BBUnity.Gameplay.Statistics.Effectors;
using BBUnity.Gameplay.Statistics.Generic;

namespace BBUnity.Gameplay.Statistics.Tests {
    public class StatisticTests {

        private DynamicIntegerStatistic _statistic;

        [SetUp]
        public void SetUp() {
            _statistic = new DynamicIntegerStatistic();
            _statistic.SetBaseMaximum(100);
            _statistic.Reset();
        }

        // --- Value model ---

        [Test]
        public void Reset_SetsCurrentToMaximum() {
            Assert.AreEqual(100, _statistic.Current);
            Assert.AreEqual(100, _statistic.Maximum);
        }

        [Test]
        public void DecreaseBy_ClampsAtMinimum() {
            _statistic.DecreaseBy(250);

            Assert.AreEqual(0, _statistic.Current);
        }

        [Test]
        public void IncreaseBy_ClampsAtMaximum() {
            _statistic.DecreaseBy(10);
            _statistic.IncreaseBy(50);

            Assert.AreEqual(100, _statistic.Current);
        }

        [Test]
        public void TrySpend_Affordable_SpendsAndReturnsTrue() {
            Assert.IsTrue(_statistic.TrySpend(30));
            Assert.AreEqual(70, _statistic.Current);
        }

        [Test]
        public void TrySpend_Unaffordable_ChangesNothing() {
            _statistic.SetCurrent(20);

            Assert.IsFalse(_statistic.TrySpend(30));
            Assert.AreEqual(20, _statistic.Current);
        }

        [Test]
        public void TrySpend_ExactlyAffordable_Succeeds() {
            _statistic.SetCurrent(30);

            Assert.IsTrue(_statistic.TrySpend(30));
            Assert.AreEqual(0, _statistic.Current);
        }

        [Test]
        public void Changed_ReportsCurrentMinimumAndEffectiveMaximum() {
            int current = -1, minimum = -1, maximum = -1;
            _statistic.Changed += (c, mn, mx) => { current = c; minimum = mn; maximum = mx; };

            _statistic.DecreaseBy(40);

            Assert.AreEqual(60, current);
            Assert.AreEqual(0, minimum);
            Assert.AreEqual(100, maximum);
        }

        // --- Maximum modifiers ---

        [Test]
        public void FlatModifier_RaisesMaximum_AndLeavesCurrentAlone() {
            _statistic.DecreaseBy(50);

            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, 20));

            Assert.AreEqual(120, _statistic.Maximum);
            Assert.AreEqual(100, _statistic.BaseMaximum);
            Assert.AreEqual(50, _statistic.Current);
        }

        [Test]
        public void PercentModifier_IsPercentOfBase() {
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.PercentOfBase, 25));

            Assert.AreEqual(125, _statistic.Maximum);
        }

        [Test]
        public void PercentModifiers_AddTogether_RatherThanCompound() {
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.PercentOfBase, 10));
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.PercentOfBase, 10));

            Assert.AreEqual(120, _statistic.Maximum);
        }

        [Test]
        public void DecreaseModifier_PullsCurrentDownToNewMaximum() {
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, -30));

            Assert.AreEqual(70, _statistic.Maximum);
            Assert.AreEqual(70, _statistic.Current);
        }

        [Test]
        public void Maximum_NeverDropsBelowMinimum() {
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, -500));

            Assert.AreEqual(0, _statistic.Maximum);
        }

        [Test]
        public void RemovingModifier_RestoresMaximum() {
            StatisticEffector effector = _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, 20));

            Assert.IsTrue(_statistic.RemoveEffector(effector));
            Assert.AreEqual(100, _statistic.Maximum);
        }

        [Test]
        public void ModifierChange_FiresChanged() {
            int fired = 0;
            _statistic.Changed += (c, mn, mx) => fired++;

            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, 20));

            Assert.AreEqual(1, fired);
        }

        [Test]
        public void TimedModifier_ExpiresAfterDuration() {
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, 20), duration: 2000);

            _statistic.Update(1f);
            Assert.AreEqual(120, _statistic.Maximum);

            _statistic.Update(1.5f);
            Assert.AreEqual(100, _statistic.Maximum);
        }

        [Test]
        public void RemoveEffectorsFrom_RemovesOnlyThatSource() {
            var ring = new object();
            var curse = new object();
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, 20), source: ring);
            _statistic.AddEffector(new MaximumModifierEffector(MaximumModifierEffector.ModifierKind.Flat, -10), source: curse);

            int removed = _statistic.RemoveEffectorsFrom(ring);

            Assert.AreEqual(1, removed);
            Assert.AreEqual(90, _statistic.Maximum);
        }

        // --- Regeneration ---

        [Test]
        public void Regeneration_AddsAmountEveryInterval() {
            _statistic.SetCurrent(0);
            _statistic.AddEffector(new RegenerationEffector(amount: 5, interval: 1000));

            _statistic.Update(0.5f);
            Assert.AreEqual(0, _statistic.Current);

            _statistic.Update(0.5f);
            Assert.AreEqual(5, _statistic.Current);

            _statistic.Update(2f);
            Assert.AreEqual(15, _statistic.Current);
        }

        [Test]
        public void Regeneration_StopsAtMaximum() {
            _statistic.SetCurrent(98);
            _statistic.AddEffector(new RegenerationEffector(amount: 5, interval: 1000));

            _statistic.Update(1f);

            Assert.AreEqual(100, _statistic.Current);
        }

        [Test]
        public void Regeneration_WaitsForDelayAfterSpending() {
            _statistic.AddEffector(new RegenerationEffector(amount: 5, interval: 1000, delayAfterChange: 2000));
            _statistic.TrySpend(50);

            _statistic.Update(1.5f);
            Assert.AreEqual(50, _statistic.Current);

            // Delay ends at 2s; one full interval after that is the first point.
            _statistic.Update(0.5f);
            _statistic.Update(1f);
            Assert.AreEqual(55, _statistic.Current);
        }

        [Test]
        public void Regeneration_SpendingAgainRestartsTheDelay() {
            _statistic.AddEffector(new RegenerationEffector(amount: 5, interval: 1000, delayAfterChange: 2000));
            _statistic.TrySpend(50);
            _statistic.Update(1.9f);

            _statistic.TrySpend(10);
            _statistic.Update(1.9f);

            Assert.AreEqual(40, _statistic.Current);
        }

        [Test]
        public void NegativeRegeneration_DrainsToMinimum() {
            _statistic.SetCurrent(8);
            _statistic.AddEffector(new RegenerationEffector(amount: -5, interval: 1000));

            _statistic.Update(3f);

            Assert.AreEqual(0, _statistic.Current);
        }

        [Test]
        public void Regeneration_AuthoredOnStatistic_Ticks() {
            var regen = new RegenerationEffector(amount: 10, interval: 1000);
            var effectors = new StatisticEffectorList();
            effectors.Add(regen);

            var statistic = new DynamicIntegerStatistic();
            typeof(DynamicIntegerStatistic)
                .GetField("_effectors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(statistic, effectors);
            statistic.SetBaseMaximum(100);
            statistic.SetCurrent(0);

            statistic.Update(1f);

            Assert.AreEqual(10, statistic.Current);
        }

        // --- Static integer ---

        [Test]
        public void StaticInteger_ClampsToRange() {
            var strength = new StaticIntegerStatistic();

            strength.SetCurrent(500);
            Assert.AreEqual(strength.Maximum, strength.Current);

            strength.SetCurrent(-5);
            Assert.AreEqual(strength.Minimum, strength.Current);
        }
    }
}
