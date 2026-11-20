using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using BBUnity.Controllers;
using BBUnity.Statistics.Base;
using BBUnity.Statistics;

namespace BBUnity.UI {

    /// <summary>
    /// A HUD bar showing one of the Player's statistics (Health, Poise, Mana, ...) on a Slider.
    /// Binds at runtime through PlayerController.Current rather than a serialized reference,
    /// because the Player is a prefab spawned by SessionController and a prefab can't point at
    /// scene objects (and the HUD can't point at a prefab instance that doesn't exist yet). The
    /// flow is one-way — the Player never knows the HUD exists.
    ///
    /// Retries every frame until the Player (and the picked statistic on it) exists, so it
    /// doesn't depend on which of the HUD or the Player woke up first. Once bound it stops
    /// polling and just listens to the statistic's ValueChanged.
    ///
    /// Binds once. If the Player is ever replaced by a new instance, this needs rebinding.
    /// </summary>
    [AddComponentMenu("BBUnity/UI/Statistic Bar")]
    public class StatisticBar : MonoBehaviour {

        [SerializeField, Tooltip("The Slider to drive. Found on this GameObject if left unassigned.")]
        private Slider _slider;

        [SerializeField, Tooltip("Which of the Player's statistics this bar shows.")]
        [FormerlySerializedAs("_attribute")]
        private StatisticTypeReference _statistic;

        private EntityStatistic _boundStatistic;

        private void Awake() {
            if (_slider == null) {
                _slider = GetComponent<Slider>();
            }

            if (_slider == null) {
                Debug.LogWarning($"StatisticBar on '{gameObject.name}' has no Slider assigned or found on this GameObject.", this);
                enabled = false;
            }
        }

        private void Update() {
            if (TryBind()) {
                enabled = false;
            }
        }

        private void OnDestroy() {
            if (_boundStatistic != null) {
                _boundStatistic.ValueChanged -= OnValueChanged;
            }
        }

        private bool TryBind() {
            PlayerController player = PlayerController.Current;
            if (player == null) { return false; }

            StatisticController statistics = player.GetComponent<StatisticController>();
            if (statistics == null) { return false; }

            EntityStatistic statistic = _statistic.Resolve(statistics);
            if (statistic == null) { return false; }

            _boundStatistic = statistic;
            _boundStatistic.ValueChanged += OnValueChanged;

            // Catch up with wherever the statistic already is — it may have reset before we
            // subscribed.
            if (statistic.NumericValue.HasValue && statistic.MinimumValue.HasValue && statistic.MaximumValue.HasValue) {
                OnValueChanged(statistic.NumericValue.Value, statistic.MinimumValue.Value, statistic.MaximumValue.Value);
            }

            return true;
        }

        private void OnValueChanged(int current, int minimum, int maximum) {
            _slider.minValue = minimum;
            _slider.maxValue = maximum;
            _slider.value = current;
        }
    }
}
