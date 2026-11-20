using UnityEngine;

using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;
using BBUnity.World.Elements;

namespace BBUnity.World.Generation {

    /// <summary>
    /// Turns a GenerationResult into objects: a GeneratedLevelRoot holding a LevelRoom for each
    /// room and an instance of each placed element's prefab at its position, and, when given an
    /// ILevelLayerBuilder, the layer it builds (the 2D assembly supplies one that paints
    /// tilemaps). It uses no editor APIs, so the same code builds a level in the editor and at
    /// runtime.
    /// </summary>
    public static class LevelBuilder {

        public static GeneratedLevelRoot Build(GenerationResult result, GridToWorld mapping, string templateName, Transform parent = null, IElementInstantiator instantiator = null, ILevelLayerBuilder layerBuilder = null) {
            instantiator = instantiator ?? new RuntimeElementInstantiator();

            GameObject rootObject = instantiator.CreateEmpty("Generated Level", parent);
            var root = rootObject.AddComponent<GeneratedLevelRoot>();
            root.Initialize(result.Seed, templateName);

            BuildRooms(result, mapping, instantiator, instantiator.CreateEmpty(GeneratedLevelRoot.RoomsContainer, rootObject.transform));
            BuildElements(result, mapping, instantiator, instantiator.CreateEmpty(GeneratedLevelRoot.ElementsContainer, rootObject.transform));

            if (layerBuilder != null) {
                GameObject layer = instantiator.CreateEmpty(layerBuilder.ContainerName, rootObject.transform);
                layerBuilder.Build(result, mapping, layer.transform);
            }

            return root;
        }

        private static void BuildRooms(GenerationResult result, GridToWorld mapping, IElementInstantiator instantiator, GameObject container) {
            foreach (GridRoom room in result.Rooms.Rooms) {
                string type = room.Type != null ? room.Type.DisplayName : "Room";
                GameObject roomObject = instantiator.CreateEmpty($"Room {room.Id} ({type})", container.transform);

                Rect bounds = mapping.ToWorld(room.Bounds);
                roomObject.transform.position = bounds.center;
                roomObject.AddComponent<LevelRoom>().Initialize(room.Id, type, bounds);
            }
        }

        private static void BuildElements(GenerationResult result, GridToWorld mapping, IElementInstantiator instantiator, GameObject container) {
            foreach (PlacementRecord placement in result.Placements) {
                var element = placement.Placeable as LevelElementDefinition;
                if (element == null || element.Prefab == null) {
                    Debug.LogWarning($"LevelBuilder skipped '{placement.Placeable?.DisplayName}' — it is not a Level Element with a prefab.");
                    continue;
                }

                Vector2 position = mapping.FootprintBottomCenter(placement.Footprint);
                GameObject instance = instantiator.Instantiate(element.Prefab, position, container.transform);
                instance.name = element.Prefab.name;

                LevelElement marker = instance.GetComponent<LevelElement>();
                if (marker == null) { marker = instance.AddComponent<LevelElement>(); }
                marker.Initialize(element, placement.RoomId);
            }
        }
    }
}
