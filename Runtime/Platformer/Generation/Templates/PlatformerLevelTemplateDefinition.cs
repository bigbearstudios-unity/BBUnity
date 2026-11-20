using UnityEngine;

using BBUnity.Generation.Layout;
using BBUnity.Generation.Layout.Placers;
using BBUnity.Generation.Layout.Shapes;
using BBUnity.Platformer.Generation.Layout;
using BBUnity.Generation.Rooms;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Platformer.Generation.Steps;
using BBUnity.Generation.Templates;
using BBUnity.World;

namespace BBUnity.Platformer.Generation.Templates {

    /// <summary>
    /// A generation template for side-on platformer levels. A new one starts with the standard
    /// recipe — lay out rooms, assign start and exit, add platforms, populate, validate — and its
    /// platformer parameters (movement, cell types, start/exit markers) ready to fill in.
    /// </summary>
    [CreateAssetMenu(fileName = "New Platformer Level Template", menuName = "BBUnity/Platformer/Generation/Templates/Platformer Level Template Definition")]
    public class PlatformerLevelTemplateDefinition : GenerationTemplateDefinition {

        public PlatformerGenerationParameters PlatformerParameters {
            get { return Parameters as PlatformerGenerationParameters; }
        }

        private void Reset() {
            ApplyStandardRecipe();
        }

        /// <summary>Replaces the parameters and steps with the standard platformer recipe.</summary>
        public void ApplyStandardRecipe(PlatformerGenerationParameters parameters = null, RoomTypeDefinition defaultRoomType = null) {
            var layout = new RoomLayoutStep();
            layout.Configure(new ScatterRoomPlacer(), new RectangleRoomShape(), new PlatformerCorridorConnector(), defaultRoomType);

            SetRecipe(parameters ?? new PlatformerGenerationParameters(), new GenerationStep[] {
                layout,
                new PlatformerRoomAssignmentStep(),
                new PlatformerPlatformStep(),
                new PopulationStep(),
                new PlatformerValidationStep()
            });
        }
    }
}
