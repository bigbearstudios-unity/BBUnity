using UnityEngine;
using UnityEngine.UIElements;

using BBUnity.Platformer.Generation.Movement;

namespace BBUnity.Editor.Platformer.Generation {

    /// <summary>
    /// Draws what a movement profile means on a grid: the player's body, a full jump at top speed
    /// and a shorter jump, so jump heights and gap widths can be judged at a glance.
    /// </summary>
    internal class JumpArcView : VisualElement {

        private const float CellPixels = 28f;
        private const float Margin = 20f;

        private PlatformerMovementProfile _profile = PlatformerMovementProfile.Default;

        public JumpArcView() {
            style.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            style.marginTop = 4;
            generateVisualContent += Draw;
            Resize();
        }

        public void SetProfile(PlatformerMovementProfile profile) {
            _profile = profile;
            Resize();
            MarkDirtyRepaint();
        }

        private float WidthCells {
            get { return Mathf.Max(8f, Mathf.Ceil(_profile.MaxJumpDistance) + _profile.BodyWidth + 3f); }
        }

        private float HeightCells {
            get { return Mathf.Max(5f, Mathf.Ceil(_profile.MaxJumpHeight) + _profile.BodyHeight + 3f); }
        }

        private void Resize() {
            style.width = WidthCells * CellPixels + Margin * 2f;
            style.height = HeightCells * CellPixels + Margin * 2f;
        }

        // Cell coordinates (x right, y up from the ground line) to the view.
        private Vector2 ToView(float x, float y) {
            return new Vector2(Margin + x * CellPixels, Margin + (HeightCells - y) * CellPixels);
        }

        private void Draw(MeshGenerationContext context) {
            Painter2D painter = context.painter2D;

            DrawGrid(painter);
            DrawGround(painter);
            DrawBody(painter, 1f, new Color(0.4f, 0.6f, 1f, 0.5f));
            DrawArc(painter, 0.6f, new Color(1f, 0.85f, 0.3f));
            DrawArc(painter, 1f, new Color(0.4f, 1f, 0.5f));
        }

        private void DrawGrid(Painter2D painter) {
            painter.strokeColor = new Color(1f, 1f, 1f, 0.07f);
            painter.lineWidth = 1f;

            for (int x = 0; x <= Mathf.CeilToInt(WidthCells); x++) { Line(painter, ToView(x, 0), ToView(x, HeightCells)); }
            for (int y = 0; y <= Mathf.CeilToInt(HeightCells); y++) { Line(painter, ToView(0, y), ToView(WidthCells, y)); }
        }

        private void DrawGround(Painter2D painter) {
            painter.strokeColor = new Color(0.7f, 0.7f, 0.75f);
            painter.lineWidth = 2f;
            Line(painter, ToView(0, 0), ToView(WidthCells, 0));
        }

        private void DrawBody(Painter2D painter, float atX, Color color) {
            Vector2 topLeft = ToView(atX - _profile.BodyWidth / 2f, _profile.BodyHeight);
            Vector2 bottomRight = ToView(atX + _profile.BodyWidth / 2f, 0f);

            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(topLeft);
            painter.LineTo(new Vector2(bottomRight.x, topLeft.y));
            painter.LineTo(bottomRight);
            painter.LineTo(new Vector2(topLeft.x, bottomRight.y));
            painter.ClosePath();
            painter.Fill();
        }

        // The path the body's feet take for a jump of `jumpScale` x full jump speed at top running speed.
        private void DrawArc(Painter2D painter, float jumpScale, Color color) {
            float startX = 1f;
            float jumpSpeed = _profile.JumpSpeed * jumpScale;
            float flightTime = 2f * jumpSpeed / _profile.Gravity;

            painter.strokeColor = color;
            painter.lineWidth = 2f;
            painter.BeginPath();

            const int Steps = 40;
            for (int i = 0; i <= Steps; i++) {
                float t = flightTime * i / Steps;
                Vector2 point = ToView(startX + _profile.RunSpeed * t, jumpSpeed * t - 0.5f * _profile.Gravity * t * t);

                if (i == 0) { painter.MoveTo(point); } else { painter.LineTo(point); }
            }

            painter.Stroke();
        }

        private static void Line(Painter2D painter, Vector2 from, Vector2 to) {
            painter.BeginPath();
            painter.MoveTo(from);
            painter.LineTo(to);
            painter.Stroke();
        }
    }
}
