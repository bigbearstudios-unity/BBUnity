using NUnit.Framework;

using UnityEngine;

using System.Collections.Generic;

using BBUnity.Combat.HitEffects;
using BBUnity.Tags;

namespace BBUnity.Combat.Tests {
    public class CombatTests {

        private GameObject _object;
        private StatisticCombatController _controller;

        [SetUp]
        public void SetUp() {
            _object = new GameObject("Combatant");
            _controller = _object.AddComponent<StatisticCombatController>();
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_object);
        }

        private static CombatActionDefinition ActionWithCooldown(int milliseconds) {
            var action = ScriptableObject.CreateInstance<CombatActionDefinition>();
            typeof(CombatActionDefinition).GetField("_cooldown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(action, milliseconds);
            return action;
        }

        // --- Hit interception ---

        [Test]
        public void ApplyHit_WithoutInterceptor_RaisesOnHit() {
            int hits = 0;
            _controller.Hit += _ => hits++;

            _controller.ApplyHit(new HitInfo(_object, new List<StatisticDamage>()));

            Assert.AreEqual(1, hits);
        }

        [Test]
        public void ApplyHit_InterceptorTakesHit_SkipsOnHitAndRaisesIntercepted() {
            int hits = 0;
            int intercepted = 0;
            _controller.Hit += _ => hits++;
            _controller.HitIntercepted += _ => intercepted++;
            _controller.HitInterceptor = _ => true;

            _controller.ApplyHit(new HitInfo(_object, new List<StatisticDamage>()));

            Assert.AreEqual(0, hits);
            Assert.AreEqual(1, intercepted);
        }

        [Test]
        public void ApplyHit_InterceptorDeclines_AppliesNormally() {
            int hits = 0;
            int intercepted = 0;
            _controller.Hit += _ => hits++;
            _controller.HitIntercepted += _ => intercepted++;
            _controller.HitInterceptor = _ => false;

            _controller.ApplyHit(new HitInfo(_object, new List<StatisticDamage>()));

            Assert.AreEqual(1, hits);
            Assert.AreEqual(0, intercepted);
        }

        // --- Tags ---

        [Test]
        public void HitInfo_HasTag_OnlyForTagsItCarries() {
            var carried = ScriptableObject.CreateInstance<CombatTagDefinition>();
            var other = ScriptableObject.CreateInstance<CombatTagDefinition>();

            var hit = new HitInfo(_object, new List<StatisticDamage>(), new List<CombatTagDefinition> { carried });

            Assert.IsTrue(hit.HasTag(carried));
            Assert.IsFalse(hit.HasTag(other));
            Assert.IsFalse(hit.HasTag(null));
        }

        [Test]
        public void HitInfo_WithoutTags_HasNone() {
            var hit = new HitInfo(_object, new List<StatisticDamage>());

            Assert.AreEqual(0, hit.Tags.Count);
        }

        // --- Combat actions ---

        [Test]
        public void CreateHit_ForAction_UsesThisControllerAsSource() {
            var action = ScriptableObject.CreateInstance<CombatActionDefinition>();

            HitInfo hit = _controller.CreateHit(action);

            Assert.AreSame(_object, hit.Source);
        }

        [Test]
        public void TryUse_NoCooldown_AlwaysSucceeds() {
            CombatActionDefinition action = ActionWithCooldown(0);

            Assert.IsTrue(_controller.TryUse(action));
            Assert.IsTrue(_controller.TryUse(action));
        }

        [Test]
        public void TryUse_DuringCooldown_Fails() {
            CombatActionDefinition action = ActionWithCooldown(60000);

            Assert.IsTrue(_controller.TryUse(action));
            Assert.IsFalse(_controller.IsReady(action));
            Assert.IsFalse(_controller.TryUse(action));
        }

        // --- Hit effect tables ---

        private static HitEffectRule Rule(List<CombatTagDefinition> attackTags, List<CombatTagDefinition> receiverTags) {
            var rule = new HitEffectRule();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(HitEffectRule).GetField("_attackTags", flags).SetValue(rule, attackTags);
            typeof(HitEffectRule).GetField("_receiverTags", flags).SetValue(rule, receiverTags);
            return rule;
        }

        private static HitEffectTable Table(params HitEffectRule[] rules) {
            var table = new HitEffectTable();
            var field = typeof(HitEffectTable).GetField("_rules", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(table, new List<HitEffectRule>(rules));
            return table;
        }

        [Test]
        public void HitEffectTable_MostSpecificMatchingRuleWins() {
            var fire = ScriptableObject.CreateInstance<CombatTagDefinition>();
            var foliage = ScriptableObject.CreateInstance<CombatTagDefinition>();

            HitEffectRule fallback = Rule(new List<CombatTagDefinition>(), new List<CombatTagDefinition>());
            HitEffectRule burning = Rule(new List<CombatTagDefinition> { fire }, new List<CombatTagDefinition>());
            HitEffectRule burningBush = Rule(new List<CombatTagDefinition> { fire }, new List<CombatTagDefinition> { foliage });
            HitEffectTable table = Table(fallback, burning, burningBush);

            Assert.AreSame(burningBush, table.Resolve(new List<CombatTagDefinition> { fire }, new List<CombatTagDefinition> { foliage }));
            Assert.AreSame(burning, table.Resolve(new List<CombatTagDefinition> { fire }, new List<CombatTagDefinition>()));
            Assert.AreSame(fallback, table.Resolve(new List<CombatTagDefinition>(), new List<CombatTagDefinition> { foliage }));
        }

        [Test]
        public void HitEffectTable_NoMatchingRule_ReturnsNull() {
            var fire = ScriptableObject.CreateInstance<CombatTagDefinition>();

            HitEffectTable table = Table(Rule(new List<CombatTagDefinition> { fire }, new List<CombatTagDefinition>()));

            Assert.IsNull(table.Resolve(new List<CombatTagDefinition>(), new List<CombatTagDefinition>()));
        }
    }
}
