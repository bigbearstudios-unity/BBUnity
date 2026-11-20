using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace BBUnity.Tiles {

    /// <summary>
    /// The thirteen tiles of the classic ground layout: a centre, four edges, four outer corners
    /// and four inner corners (where a notch is cut out of the diagonal), plus an optional tile
    /// for pieces that fit none of them (a lone block, a one-wide pillar).
    /// </summary>
    [Serializable]
    public class EdgeCornerTileSet {
        public TileBase Center;
        public TileBase Top;
        public TileBase Bottom;
        public TileBase Left;
        public TileBase Right;
        public TileBase TopLeft;
        public TileBase TopRight;
        public TileBase BottomLeft;
        public TileBase BottomRight;
        public TileBase InnerTopLeft;
        public TileBase InnerTopRight;
        public TileBase InnerBottomLeft;
        public TileBase InnerBottomRight;
        public TileBase Single;
    }

    /// <summary>
    /// Builds a rule set from a standard tile layout, so a tileset can be wired up by naming its
    /// tiles instead of drawing every rule by hand.
    /// </summary>
    public static class TileRuleGenerator {

        private const NeighborRequirement Same = NeighborRequirement.Same;
        private const NeighborRequirement Different = NeighborRequirement.Different;

        /// <summary>Rules for solid ground and walls from a 13-tile edge and corner set. Rules for missing tiles are left out.</summary>
        public static List<TileRule> FromEdgeCornerSet(EdgeCornerTileSet tiles) {
            var rules = new List<TileRule>();

            Add(rules, "Center", tiles.Center, Same, Same, Same, Same, Same, Same, Same, Same);

            // Inner corners: all four sides are the same, one diagonal is missing.
            Add(rules, "Inner Top Left", tiles.InnerTopLeft, Same, null, Same, null, Same, null, Same, Different);
            Add(rules, "Inner Top Right", tiles.InnerTopRight, Same, Different, Same, null, Same, null, Same, null);
            Add(rules, "Inner Bottom Left", tiles.InnerBottomLeft, Same, null, Same, null, Same, Different, Same, null);
            Add(rules, "Inner Bottom Right", tiles.InnerBottomRight, Same, null, Same, Different, Same, null, Same, null);

            // Edges: one side is open.
            Add(rules, "Top", tiles.Top, Different, null, Same, null, Same, null, Same, null);
            Add(rules, "Bottom", tiles.Bottom, Same, null, Same, null, Different, null, Same, null);
            Add(rules, "Left", tiles.Left, Same, null, Same, null, Same, null, Different, null);
            Add(rules, "Right", tiles.Right, Same, null, Different, null, Same, null, Same, null);

            // Outer corners: two touching sides are open.
            Add(rules, "Top Left", tiles.TopLeft, Different, null, Same, null, Same, null, Different, null);
            Add(rules, "Top Right", tiles.TopRight, Different, null, Different, null, Same, null, Same, null);
            Add(rules, "Bottom Left", tiles.BottomLeft, Same, null, Same, null, Different, null, Different, null);
            Add(rules, "Bottom Right", tiles.BottomRight, Same, null, Different, null, Different, null, Same, null);

            return rules;
        }

        /// <summary>Rules for a horizontal strip such as a one-way platform: left cap, middle, right cap and a single block.</summary>
        public static List<TileRule> FromHorizontalStrip(TileBase left, TileBase middle, TileBase right, TileBase single) {
            var rules = new List<TileRule>();

            AddStrip(rules, "Middle", middle, Same, Same);
            AddStrip(rules, "Left", left, Different, Same);
            AddStrip(rules, "Right", right, Same, Different);
            AddStrip(rules, "Single", single, Different, Different);

            return rules;
        }

        // Requirements are given in TileNeighbor order: N, NE, E, SE, S, SW, W, NW (null = any).
        private static void Add(List<TileRule> rules, string name, TileBase tile, params NeighborRequirement?[] requirements) {
            if (tile == null) { return; }

            var rule = new TileRule(name, tile);
            for (int i = 0; i < requirements.Length; i++) {
                if (requirements[i].HasValue) { rule.Require((TileNeighbor)i, requirements[i].Value); }
            }

            rules.Add(rule);
        }

        private static void AddStrip(List<TileRule> rules, string name, TileBase tile, NeighborRequirement west, NeighborRequirement east) {
            if (tile == null) { return; }

            rules.Add(new TileRule(name, tile).Require(TileNeighbor.West, west).Require(TileNeighbor.East, east));
        }
    }
}
