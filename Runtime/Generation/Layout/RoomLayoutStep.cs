using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Layout.Connectors;
using BBUnity.Generation.Layout.Placers;
using BBUnity.Generation.Layout.Shapes;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// The general room-and-passage layout: fills the grid with solid, asks the Room Placer where
    /// rooms go, the Room Shape to carve each one, then links them with the Room Connector along
    /// the paths the ConnectionPlanner chooses. Different level styles are different
    /// combinations of those three parts and different numbers; write a subclass of a part for a
    /// new idea, or of LayoutStep for an algorithm that doesn't work in rooms at all.
    /// </summary>
    [Serializable]
    public class RoomLayoutStep : GenerationStep {

        [SerializeField, Tooltip("What the level is filled with before rooms are carved out. Uses the template's default solid if left empty.")]
        private CellTypeDefinition _solid;

        [SerializeField, Tooltip("The type given to every room this step creates. Later steps can change individual rooms.")]
        private RoomTypeDefinition _roomType;

        [SerializeField]
        private IntParameter _roomCount = IntParameter.Range(5, 8);

        [SerializeField]
        private IntParameter _roomWidth = IntParameter.Range(8, 14);

        [SerializeField]
        private IntParameter _roomHeight = IntParameter.Range(6, 10);

        [SerializeField]
        private IntParameter _corridorWidth = IntParameter.Fixed(3);

        [SerializeField, Tooltip("Extra passages beyond the minimum needed to reach every room, which make loops.")]
        private IntParameter _extraConnections = IntParameter.Range(0, 2);

        [SerializeField, Min(1), Tooltip("Solid cells kept around the edge of the level.")]
        private int _margin = 2;

        [SerializeReference]
        private RoomPlacer _placer = new ScatterRoomPlacer();

        [SerializeReference]
        private RoomShape _shape = new RectangleRoomShape();

        [SerializeReference]
        private RoomConnector _connector = new LShapedCorridorConnector();

        public override string DisplayName {
            get { return "Room Layout"; }
        }

        /// <summary>Sets the three parts and the type given to rooms (for building a step in code).</summary>
        public void Configure(RoomPlacer placer, RoomShape shape, RoomConnector connector, RoomTypeDefinition roomType = null) {
            _placer = placer;
            _shape = shape;
            _connector = connector;
            _roomType = roomType;
        }

        public override void Execute(GenerationContext context) {
            if (_placer == null || _shape == null || _connector == null) {
                context.AddIssue(IssueSeverity.Error, "Room Layout needs a Room Placer, a Room Shape and a Room Connector.");
                return;
            }

            CellTypeDefinition solid = _solid != null ? _solid : context.Parameters?.DefaultSolid;
            if (solid == null) {
                context.AddIssue(IssueSeverity.Error, "Room Layout needs a Solid cell type to fill the level with.");
                return;
            }

            GenerationGrid grid = context.Result.Grid;
            GridLayer layer = grid.Main;
            layer.Fill(new RectInt(0, 0, grid.Width, grid.Height), solid);

            int wanted = context.Resolve("layout.roomCount", _roomCount);
            var request = new RoomPlacementRequest {
                GridWidth = grid.Width,
                GridHeight = grid.Height,
                RoomCount = wanted,
                MinRoomWidth = _roomWidth.Min,
                MaxRoomWidth = _roomWidth.Max,
                MinRoomHeight = _roomHeight.Min,
                MaxRoomHeight = _roomHeight.Max,
                Margin = _margin
            };

            List<RectInt> bounds = _placer.Place(request, context.Random.Fork("layout.place"));
            if (bounds.Count == 0) {
                context.AddIssue(IssueSeverity.Error, "Room Layout could not place any rooms — the level is too small for the room sizes.");
                return;
            }

            if (bounds.Count < wanted) {
                context.AddIssue(IssueSeverity.Warning, $"Room Layout placed {bounds.Count} of {wanted} rooms — there was no room for more.");
            }

            GenerationRandom shapeRandom = context.Random.Fork("layout.shape");
            foreach (RectInt area in bounds) {
                context.Result.Rooms.AddRoom(area, _roomType);
                _shape.Carve(layer, area, shapeRandom);
            }

            ConnectRooms(context, layer);
        }

        private void ConnectRooms(GenerationContext context, GridLayer layer) {
            RoomLinking.ConnectAll(context, layer, _connector, _corridorWidth, _extraConnections);
        }
    }
}
