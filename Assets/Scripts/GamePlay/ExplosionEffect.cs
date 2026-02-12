using UnityEngine;

namespace FruitMixer.Effects
{
    /// <summary>
    /// シンプルな爆発エフェクト
    /// 指定時間後に自動削除
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("設定")]
        [Tooltip("表示時間（秒）")]
        [SerializeField] private float duration = 0.3f;

        [Tooltip("拡大アニメーション")]
        [SerializeField] private bool useScaleAnimation = true;

        [Tooltip("最終スケール")]
        [SerializeField] private float targetScale = 1.5f;

        private float timer = 0f;
        private Vector3 initialScale;
        private SpriteRenderer spriteRenderer;

        void Start()
        {
            initialScale = transform.localScale;
            spriteRenderer = GetComponent<SpriteRenderer>();

            // 指定時間後に削除
            Destroy(gameObject, duration);
        }

        void Update()
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            if (useScaleAnimation)
            {
                // 拡大アニメーション
                transform.localScale = Vector3.Lerp(initialScale, initialScale * targetScale, progress);
            }

            // フェードアウト
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f - progress;
                spriteRenderer.color = color;
            }
        }
    }
}