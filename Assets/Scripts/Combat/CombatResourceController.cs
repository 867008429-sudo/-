using System;
using UnityEngine;

namespace HuanXian.Combat
{
    public sealed class CombatResourceController : MonoBehaviour
    {
        [Header("Sanity")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float sanity = 100f;
        [SerializeField] private float sanityRegenPerSecond = 16f;
        [SerializeField] private float sanityRegenDelay = 0.7f;

        [Header("Summon Gauge")]
        [SerializeField] private float maxSummonGauge = 100f;
        [SerializeField] private float summonGauge;

        private float _sanityRegenTimer;

        public event Action<float, float> SanityChanged;
        public event Action<float, float> SummonGaugeChanged;

        public float MaxSanity => maxSanity;
        public float Sanity => sanity;
        public float MaxSummonGauge => maxSummonGauge;
        public float SummonGauge => summonGauge;
        public bool CanInvokeDescent => summonGauge >= maxSummonGauge;

        private void Awake()
        {
            sanity = Mathf.Clamp(sanity, 0f, maxSanity);
            summonGauge = Mathf.Clamp(summonGauge, 0f, maxSummonGauge);
        }

        private void Update()
        {
            if (sanity >= maxSanity || sanityRegenPerSecond <= 0f)
            {
                return;
            }

            _sanityRegenTimer -= Time.deltaTime;
            if (_sanityRegenTimer > 0f)
            {
                return;
            }

            ModifySanity(sanityRegenPerSecond * Time.deltaTime);
        }

        public float ModifySanity(float amount)
        {
            float previous = sanity;
            sanity = Mathf.Clamp(sanity + amount, 0f, maxSanity);

            if (!Mathf.Approximately(previous, sanity))
            {
                SanityChanged?.Invoke(previous, sanity);
            }

            return sanity;
        }

        public float ModifySummonGauge(float amount)
        {
            float previous = summonGauge;
            summonGauge = Mathf.Clamp(summonGauge + amount, 0f, maxSummonGauge);

            if (!Mathf.Approximately(previous, summonGauge))
            {
                SummonGaugeChanged?.Invoke(previous, summonGauge);
            }

            return summonGauge;
        }

        public bool TrySpendSanity(float amount)
        {
            if (amount < 0f)
            {
                return false;
            }

            if (sanity < amount)
            {
                return false;
            }

            ModifySanity(-amount);
            if (amount > 0f)
            {
                _sanityRegenTimer = sanityRegenDelay;
            }

            return true;
        }

        public void ResetSummonGauge()
        {
            ModifySummonGauge(-maxSummonGauge);
        }
    }
}
