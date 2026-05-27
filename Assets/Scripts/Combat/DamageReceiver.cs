using UnityEngine;

namespace HuanXian.Combat
{
    public sealed class DamageReceiver : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float health = 100f;

        public float Health => health;
        public bool IsAlive => health > 0f;

        private void Awake()
        {
            health = Mathf.Clamp(health, 0f, maxHealth);
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            health = Mathf.Max(health - amount, 0f);
        }
    }
}
