using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Gameplay.Controllers;
using BBUnity.Gameplay.Entities.Controllers;
using BBUnity.Gameplay.UI.FloatingText;
using BBUnity.UI;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// The presets BBUnity ships. Add to the lists below as more components turn up that belong
    /// in a category; projects that want something different duplicate this preset in
    /// Window > BBUnity > Component Presets and edit their copy.
    /// </summary>
    public static class BuiltInComponentPresets {

        public const string BBUnityName = "BBUnity";

        private static ComponentCategoryPresetDefinition _bbUnity;

        public static ComponentCategoryPresetDefinition BBUnity {
            get {
                if (_bbUnity == null) {
                    _bbUnity = ComponentCategoryPresetDefinition.Create(BBUnityName, CreateBBUnityRules());
                    _bbUnity.MarkBuiltIn();
                }

                return _bbUnity;
            }
        }

        private static List<ComponentCategoryRule> CreateBBUnityRules() {
            return new List<ComponentCategoryRule> {
                new ComponentCategoryRule("Physics",
                    ComponentMatch.ForType(typeof(Rigidbody)),
                    ComponentMatch.ForType(typeof(Rigidbody2D)),
                    ComponentMatch.ForType(typeof(Collider)),
                    ComponentMatch.ForType(typeof(Collider2D)),
                    ComponentMatch.ForType(typeof(Joint)),
                    ComponentMatch.ForType(typeof(Joint2D)),
                    ComponentMatch.ForType(typeof(Effector2D)),
                    ComponentMatch.ForType(typeof(ConstantForce)),
                    ComponentMatch.ForType(typeof(ConstantForce2D)),
                    ComponentMatch.ForType(typeof(ArticulationBody)),
                    ComponentMatch.ForType(typeof(CharacterController))
                ),
                new ComponentCategoryRule("Rendering",
                    ComponentMatch.ForType(typeof(SpriteRenderer)),
                    ComponentMatch.ForType(typeof(Animator))
                ),
                new ComponentCategoryRule("Cinemachine",
                    ComponentMatch.ForNamespace("Unity.Cinemachine"),
                    ComponentMatch.ForNamespace("Cinemachine")
                ),
                new ComponentCategoryRule("BBUnity Controllers",
                    ComponentMatch.ForType(typeof(EntityController)),
                    ComponentMatch.ForType(typeof(PooledSoundController)),
                    ComponentMatch.ForType(typeof(SessionController)),
                    ComponentMatch.ForType(typeof(PlayerController)),
                    ComponentMatch.ForType(typeof(TimeController)),
                    ComponentMatch.ForType(typeof(BootLoaderController)),
                    ComponentMatch.ForType(typeof(LevelTransitionController)),
                    ComponentMatch.ForType(typeof(FloatingTextController)),
                    ComponentMatch.ForType(typeof(UIScreenController))
                )
            };
        }
    }
}
