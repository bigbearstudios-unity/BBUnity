using NUnit.Framework;
using UnityEngine;

using BBUnity.Editor.Support.ComponentManagement;
using BBUnity.Statistics;

namespace BBUnity.Editor.Support.ComponentManagement.Tests {
    public class ComponentPresetTests {

        private static ComponentCategoryPresetDefinition BuiltIn {
            get { return BuiltInComponentPresets.BBUnity; }
        }

        [Test]
        public void BuiltIn_PutsPhysicsComponentsInPhysics() {
            Assert.AreEqual("Physics", BuiltIn.CategoryFor(typeof(Rigidbody2D)));
            Assert.AreEqual("Physics", BuiltIn.CategoryFor(typeof(BoxCollider2D)));
            Assert.AreEqual("Physics", BuiltIn.CategoryFor(typeof(Rigidbody)));
        }

        [Test]
        public void BuiltIn_PutsSpriteRendererAndAnimatorInRendering() {
            Assert.AreEqual("Rendering", BuiltIn.CategoryFor(typeof(SpriteRenderer)));
            Assert.AreEqual("Rendering", BuiltIn.CategoryFor(typeof(Animator)));
        }

        [Test]
        public void BuiltIn_PutsBBUnityControllersInTheirCategory() {
            Assert.AreEqual("BBUnity Controllers", BuiltIn.CategoryFor(typeof(StatisticController)));
        }

        [Test]
        public void BuiltIn_LeavesUnmatchedComponentsAlone() {
            Assert.IsNull(BuiltIn.CategoryFor(typeof(Transform)));
            Assert.IsNull(BuiltIn.CategoryFor(typeof(AudioSource)));
        }

        [Test]
        public void BuiltIn_IsMarkedBuiltIn() {
            Assert.IsTrue(BuiltIn.IsBuiltIn);
        }

        [Test]
        public void NamespaceMatch_CoversTheNamespaceAndBelowButNotSimilarNames() {
            var inside = ComponentMatch.ForNamespace("UnityEngine");
            var lookalike = ComponentMatch.ForNamespace("Unity");

            Assert.IsTrue(inside.Matches(typeof(Rigidbody2D)));
            Assert.IsFalse(lookalike.Matches(typeof(Rigidbody2D)));
        }

        [Test]
        public void TypeMatch_CoversDerivedTypes() {
            Assert.IsTrue(ComponentMatch.ForType(typeof(Collider2D)).Matches(typeof(CircleCollider2D)));
            Assert.IsFalse(ComponentMatch.ForType(typeof(CircleCollider2D)).Matches(typeof(Collider2D)));
        }

        [Test]
        public void FirstMatchingRuleWins() {
            var preset = ComponentCategoryPresetDefinition.Create("Test", new[] {
                new ComponentCategoryRule("First", ComponentMatch.ForType(typeof(Collider2D))),
                new ComponentCategoryRule("Second", ComponentMatch.ForType(typeof(BoxCollider2D)))
            });

            try {
                Assert.AreEqual("First", preset.CategoryFor(typeof(BoxCollider2D)));
            } finally {
                Object.DestroyImmediate(preset);
            }
        }

        [Test]
        public void CategoryOrder_FollowsRulesWithoutDuplicates() {
            var preset = ComponentCategoryPresetDefinition.Create("Test", new[] {
                new ComponentCategoryRule("B", ComponentMatch.ForType(typeof(Rigidbody2D))),
                new ComponentCategoryRule("A", ComponentMatch.ForType(typeof(Animator))),
                new ComponentCategoryRule("B", ComponentMatch.ForType(typeof(SpriteRenderer)))
            });

            try {
                CollectionAssert.AreEqual(new[] { "B", "A" }, preset.CategoryOrder());
            } finally {
                Object.DestroyImmediate(preset);
            }
        }

        [Test]
        public void CopyRulesFrom_MakesAnIndependentCopy() {
            var copy = ComponentCategoryPresetDefinition.Create("Copy", new ComponentCategoryRule[0]);

            try {
                copy.CopyRulesFrom(BuiltIn);

                Assert.AreEqual(BuiltIn.Rules.Count, copy.Rules.Count);
                Assert.AreEqual("Physics", copy.CategoryFor(typeof(Rigidbody2D)));
                Assert.AreNotSame(BuiltIn.Rules[0], copy.Rules[0]);
                Assert.IsFalse(copy.IsBuiltIn);
            } finally {
                Object.DestroyImmediate(copy);
            }
        }
    }
}
