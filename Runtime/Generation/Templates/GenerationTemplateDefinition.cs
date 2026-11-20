using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Chunks;
using BBUnity.Generation.Grid;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Templates {

    public enum SeedMode {
        Random,
        Fixed
    }

    /// <summary>What can vary between runs of one template without editing it.</summary>
    public class GenerationOptions {
        public int? Seed;
        public ParameterOverrideSet Overrides;
    }

    /// <summary>
    /// A complete recipe for generating a level: how big it is, the steps that build it, the
    /// shared parameters those steps read, the chunks they may use and how the seed is chosen.
    /// It is built and tested in the editor and is also what a game runs at runtime — Generate
    /// needs nothing from the editor. Game families derive from it (the platformer adds its
    /// movement profile and defaults).
    /// </summary>
    [CreateAssetMenu(fileName = "New Generation Template", menuName = "BBUnity/Generation/Templates/Generation Template Definition")]
    public class GenerationTemplateDefinition : ScriptableObject {

        public const string WidthKey = "size.width";
        public const string HeightKey = "size.height";

        [SerializeField]
        private IntParameter _width = IntParameter.Fixed(100);

        [SerializeField]
        private IntParameter _height = IntParameter.Fixed(60);

        [SerializeReference]
        private GenerationParameters _parameters;

        [SerializeReference]
        private List<GenerationStep> _steps = new List<GenerationStep>();

        [SerializeField]
        private ChunkLibraryDefinition _chunkLibrary;

        [SerializeField]
        private SeedMode _seedMode = SeedMode.Random;

        [SerializeField]
        private int _fixedSeed = 12345;

        public IReadOnlyList<GenerationStep> Steps {
            get { return _steps; }
        }

        public GenerationParameters Parameters {
            get { return _parameters; }
        }

        public ChunkLibraryDefinition ChunkLibrary {
            get { return _chunkLibrary; }
        }

        /// <summary>For derived templates to fill in their parameters and steps when first created.</summary>
        protected void SetRecipe(GenerationParameters parameters, IEnumerable<GenerationStep> steps) {
            _parameters = parameters;
            _steps = new List<GenerationStep>(steps);
        }

        /// <summary>Runs the pipeline and returns the generated level (never null; check Issues).</summary>
        public GenerationResult Generate(GenerationOptions options = null) {
            int seed = options != null && options.Seed.HasValue ? options.Seed.Value : ChooseSeed();

            var result = new GenerationResult { Seed = seed };
            var context = new GenerationContext(new GenerationRandom(seed), _parameters, options?.Overrides, result, _chunkLibrary);

            result.Grid = new GenerationGrid(context.Resolve(WidthKey, _width), context.Resolve(HeightKey, _height));

            GenerationPipeline.Run(_steps, context);
            return result;
        }

        private int ChooseSeed() {
            return _seedMode == SeedMode.Fixed ? _fixedSeed : new System.Random().Next();
        }
    }
}
