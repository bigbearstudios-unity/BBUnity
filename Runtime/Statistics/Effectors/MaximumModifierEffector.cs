using UnityEngine;

namespace BBUnity.Statistics.Effectors {

    /// <summary>
    /// Raises (positive Amount) or lowers (negative Amount) the statistic's maximum — a +20 mana
    /// ring, a curse that cuts health by 10%. Percentages are of the statistic's authored base
    /// maximum, not of the already-modified one, and every modifier's bonus is added together, so
    /// two +10% effectors give +20% rather than compounding.
    ///
    /// Lowering the maximum below the current value pulls the current value down with it;
    /// raising it leaves the current value where it is.
    /// </summary>
    [System.Serializable]
    public class MaximumModifierEffector : StatisticEffector {

        public enum ModifierKind {
            Flat = 0,
            PercentOfBase = 100
        }

        [SerializeField]
        private ModifierKind _kind = ModifierKind.Flat;

        [SerializeField, Tooltip("Flat points, or percent of the base maximum. Negative decreases the maximum.")]
        private int _amount = 10;

        public ModifierKind Kind { get { return _kind; } }
        public int Amount { get { return _amount; } }

        public MaximumModifierEffector() { }

        public MaximumModifierEffector(ModifierKind kind, int amount) {
            _kind = kind;
            _amount = amount;
        }

        public override int MaximumBonus(int baseMaximum) {
            if (_kind == ModifierKind.PercentOfBase) {
                return Mathf.RoundToInt(baseMaximum * _amount / 100f);
            }

            return _amount;
        }
    }
}
