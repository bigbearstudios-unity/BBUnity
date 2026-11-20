using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Platformer.Generation.Movement;
using BBUnity.Generation.Rooms;

namespace BBUnity.Platformer.Generation {

    /// <summary>
    /// The settings shared by every step of a platformer template: how the player moves, the cell
    /// types the level is built from, and the room types and placeables that mark where the player
    /// starts and the level ends.
    /// </summary>
    [Serializable]
    public class PlatformerGenerationParameters : GenerationParameters {

        [SerializeField]
        private MovementProfileSource _movement = new MovementProfileSource();

        [SerializeField, Tooltip("The cell type for walls and ground.")]
        private CellTypeDefinition _solid;

        [SerializeField, Tooltip("The cell type for one-way platforms the generator adds so jumps can reach new heights.")]
        private CellTypeDefinition _platform;

        [SerializeField, Min(1), Tooltip("How wide, in cells, each added platform is.")]
        private int _platformWidth = 3;

        [SerializeField]
        private RoomTypeDefinition _startRoomType;

        [SerializeField]
        private RoomTypeDefinition _exitRoomType;

        [SerializeField, Tooltip("What marks where the player starts.")]
        private PlaceableDefinition _playerSpawn;

        [SerializeField, Tooltip("What marks the way out of the level.")]
        private PlaceableDefinition _levelExit;

        public PlatformerGenerationParameters() { }

        public PlatformerGenerationParameters(PlatformerMovementProfile profile, CellTypeDefinition solid, CellTypeDefinition platform, RoomTypeDefinition startRoomType, RoomTypeDefinition exitRoomType, PlaceableDefinition playerSpawn, PlaceableDefinition levelExit) {
            _movement = new MovementProfileSource(profile);
            _solid = solid;
            _platform = platform;
            _startRoomType = startRoomType;
            _exitRoomType = exitRoomType;
            _playerSpawn = playerSpawn;
            _levelExit = levelExit;
        }

        public override CellTypeDefinition DefaultSolid {
            get { return _solid; }
        }

        public MovementProfileSource Movement {
            get { return _movement; }
        }

        public CellTypeDefinition Solid {
            get { return _solid; }
        }

        public CellTypeDefinition Platform {
            get { return _platform; }
        }

        public int PlatformWidth {
            get { return _platformWidth; }
        }

        public RoomTypeDefinition StartRoomType {
            get { return _startRoomType; }
        }

        public RoomTypeDefinition ExitRoomType {
            get { return _exitRoomType; }
        }

        public PlaceableDefinition PlayerSpawn {
            get { return _playerSpawn; }
        }

        public PlaceableDefinition LevelExit {
            get { return _levelExit; }
        }

        /// <summary>The parameters of the context, or null (after raising an error) if it isn't a platformer one.</summary>
        public static PlatformerGenerationParameters From(GenerationContext context, string stepName) {
            var parameters = context.Parameters as PlatformerGenerationParameters;
            if (parameters == null) {
                context.AddIssue(IssueSeverity.Error, $"{stepName} needs the template's parameters to be Platformer Generation Parameters.");
            }

            return parameters;
        }
    }
}
