using UnityEngine;
using UnityEngine.UI;

using BBUnity.UI.Utilities;

namespace BBUnity.UI.Components {

    /// <summary>
    /// A UI mesh effect that applies a two-color linear gradient to a Graphic by modifying its
    /// vertex colors in ModifyMesh(). The gradient color is multiplied into the Graphic's
    /// existing vertex color (rather than replacing it) so it composes with the Graphic's own
    /// tint (Graphic.color) instead of overwriting it.
    ///
    /// Requires a Graphic on the same GameObject (Image, RawImage, Text, TextMeshProUGUI, ...) —
    /// mesh effects only run when a Graphic rebuilds its mesh and asks its IMeshModifiers to run;
    /// without one, ModifyMesh is never called. Not specific to any particular kind of UI element
    /// ("panel" or otherwise) — any Graphic works.
    /// </summary>
    [AddComponentMenu("BBUnity/UI/Components/UI Background Gradient")]
    [RequireComponent(typeof(Graphic))]
    public class UIBackgroundGradient : BaseMeshEffect {

        [SerializeField, Tooltip("The color at the top of the gradient (at Angle = 0).")]
        private Color _color1 = Color.white;

        [SerializeField, Tooltip("The color at the bottom of the gradient (at Angle = 0).")]
        private Color _color2 = Color.white;

        [SerializeField, Tooltip("The gradient's rotation in degrees. 0 is bottom-to-top.")]
        private float _angle = 0.0f;

        public Color Color1 {
            get { return _color1; }
            set { _color1 = value; RequestRebuild(); }
        }

        public Color Color2 {
            get { return _color2; }
            set { _color2 = value; RequestRebuild(); }
        }

        public float Angle {
            get { return _angle; }
            set { _angle = value; RequestRebuild(); }
        }

        public override void ModifyMesh(VertexHelper vh) {
            if(!enabled) return;

            Rect rect = graphic.rectTransform.rect;
            Vector2 dir = ColorGradient.RotationDir(_angle);
            dir = ColorGradient.CompensateAspectRatio(rect, dir);

            ColorGradient.Matrix2x3 localPositionMatrix = ColorGradient.LocalPositionMatrix(rect, dir);

            UIVertex vertex = default(UIVertex);
            for (int i = 0; i < vh.currentVertCount; i++) {
                vh.PopulateUIVertex(ref vertex, i);
                Vector2 localPosition = localPositionMatrix * vertex.position;
                vertex.color *= Color.Lerp(_color2, _color1, localPosition.y);
                vh.SetUIVertex(vertex, i);
            }
        }

        private void RequestRebuild() {
            if(graphic != null) { graphic.SetVerticesDirty(); }
        }

        private new void OnValidate() {
            RequestRebuild();
        }
    }
}
