using UnityEngine;

namespace FruitMixer.Gameplay
{
    /// <summary>
    /// 爆弾データと爆発処理
    /// </summary>
    public class BombData : MonoBehaviour
    {
        [Header("爆発設定")]
        [Tooltip("爆発半径")]
        [SerializeField] private float explosionRadius = 3f;

        [Tooltip("爆発力")]
        [SerializeField] private float explosionForce = 500f;

        [Header("エフェクト")]
        [Tooltip("爆発エフェクトPrefab (オプション)")]
        [SerializeField] private GameObject explosionEffectPrefab;

        [Header("状態")]
        [Tooltip("ミキサー内にいるか")]
        public bool isInMixer = false;

        /// <summary>
        /// 爆発処理（ミキサー内では弱い爆発）
        /// </summary>
        public void Explode()
        {
            Debug.Log($"[BombData] 💥 爆発! 位置: {transform.position}");

            // ミキサー内では爆発力を50%に減少
            float actualRadius = explosionRadius;
            float actualForce = explosionForce;

            if (isInMixer)
            {
                actualRadius *= 0.5f;
                actualForce *= 0.5f;
                Debug.Log($"[BombData] ⚠️ ミキサー内の弱い爆発! 半径:{actualRadius}, 力:{actualForce}");
            }

            // 爆発エフェクト生成
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            // 爆発力適用（修正された値使用）
            ApplyExplosionForce(actualRadius, actualForce);

            // 自分を破壊
            Destroy(gameObject);
        }

        /// <summary>
        /// 周囲のオブジェクトに爆発力を適用
        /// </summary>
        private void ApplyExplosionForce(float radius, float force)
        {
            // Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);  // ← 수정

            foreach (Collider2D col in colliders)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null && rb.gameObject != gameObject)
                {
                    Vector2 direction = (rb.position - (Vector2)transform.position).normalized;
                    float distance = Vector2.Distance(rb.position, transform.position);

                    // 距離に応じて力を減衰
                    // float forceMagnitude = explosionForce * (1 - distance / explosionRadius);
                    float forceMagnitude = force * (1 - distance / radius);  // ← 수정

                    rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);

                    Debug.Log($"[BombData] {col.gameObject.name}に爆発力適用: {forceMagnitude}");
                }
            }
        }

#if UNITY_EDITOR
        // エディタでギズモ表示
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
#endif
    }
}