using UnityEngine;
using UnityEngine.UI;

using System.Collections;

namespace BBUnity.UI {

    /// <summary>
    /// Covers the whole screen with a color that can be faded in and out — the one overlay
    /// everything that wants to fade the screen (a level transition, a death screen, ...) shares,
    /// so two of them can never stack up or fight. Lives next to SessionController and is reached
    /// through SessionController.Current.ScreenFader (added automatically if the scene doesn't
    /// have one — see SessionController.Awake).
    ///
    /// Fades use unscaled time, so a stopped or slowed Time.timeScale can't stall them. Starting a
    /// fade takes over from whichever one was running, continuing from the alpha it had reached.
    /// </summary>
    [AddComponentMenu("BBUnity/UI/Screen Fader")]
    public class ScreenFader : MonoBehaviour {

        private Image _image;
        private float _alpha;
        private int _fadeId;

        /// <summary>
        /// How opaque the overlay is right now, 0 (clear) to 1 (fully covered).
        /// </summary>
        public float Alpha {
            get { return _alpha; }
        }

        /// <summary>
        /// Fades the overlay to `toAlpha` of `color` over `milliseconds`, starting from its
        /// current alpha. Yield on it to wait for the fade to finish; a newer fade ends this one
        /// early.
        /// </summary>
        public IEnumerator Fade(Color color, float toAlpha, int milliseconds) {
            Image image = GetOrCreateImage();
            int id = ++_fadeId;

            float from = _alpha;
            float duration = milliseconds / 1000f;
            float elapsed = 0f;

            while (elapsed < duration) {
                SetAlpha(image, color, Mathf.Lerp(from, toAlpha, elapsed / duration));
                elapsed += Time.unscaledDeltaTime;
                yield return null;

                if (id != _fadeId) { yield break; }
            }

            SetAlpha(image, color, toAlpha);
        }

        /// <summary>
        /// Same as Fade, but starts it running on this component and returns straight away — for
        /// callers that aren't a coroutine themselves.
        /// </summary>
        public Coroutine FadeTo(Color color, float toAlpha, int milliseconds) {
            return StartCoroutine(Fade(color, toAlpha, milliseconds));
        }

        private void SetAlpha(Image image, Color color, float alpha) {
            _alpha = alpha;
            color.a = alpha;
            image.color = color;
            image.enabled = alpha > 0f;
        }

        // Built on first use — a screen-space overlay above everything else that never blocks input.
        private Image GetOrCreateImage() {
            if (_image != null) { return _image; }

            GameObject canvasObject = new GameObject("Screen Fader", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            GameObject imageObject = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rect = (RectTransform)imageObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _image = imageObject.GetComponent<Image>();
            _image.raycastTarget = false;
            _image.enabled = false;

            return _image;
        }
    }
}
