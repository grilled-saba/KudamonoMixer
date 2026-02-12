using UnityEngine;

namespace FruitMixer.Core
{
    /// <summary>
    /// ゲーム中のフルーツGameObjectにアタッチするコンポーネント
    /// 現在の状態を追跡
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class FruitData : MonoBehaviour
    {
        [Header("参照")]
        [Tooltip("このフルーツの情報（ScriptableObject）")]
        public FruitInfo fruitInfo;

        [Header("現在の状態")]
        [Tooltip("カットされたか（1回のみカット可能）")]
        public bool isSliced = false;

        [Tooltip("現在のティア（カットすると-1）")]
        public int currentTier;

        [Tooltip("ミキサー内にいるか")]
        public bool isInMixer = false;

        [Tooltip("一度でもミキサーに入ったか（脱出判定用）")]
        public bool hasEverEnteredMixer = false;

        [Header("デバッグ")]
        [Tooltip("デバッグログを表示")]
        public bool showDebugLog = false;

        // コンポーネントキャッシュ
        private Rigidbody2D rb;
        private PolygonCollider2D polygonCollider;
        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            polygonCollider = GetComponent<PolygonCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// FruitInfoから初期化
        /// </summary>
        public void Initialize(FruitInfo info)
        {
            if (info == null)
            {
                Debug.LogError("[FruitData] FruitInfo が null です！");
                return;
            }

            fruitInfo = info;
            currentTier = info.tier;

            // ビジュアル設定
            if (info.wholeFruitSprite != null)
            {
                spriteRenderer.sprite = info.wholeFruitSprite;

                // PolygonCollider2Dを自動生成（スプライトの形状に合わせる）
                UpdateColliderShape();
            }
            else
            {
                Debug.LogWarning($"[FruitData] {info.fruitType} のスプライトが設定されていません");
            }

            // 物理設定
            rb.mass = info.mass;

            // 初期状態
            isSliced = false;
            isInMixer = false;
            hasEverEnteredMixer = false;

            if (showDebugLog)
            {
                Debug.Log($"[FruitData] 初期化完了: {info.fruitType}, Tier {currentTier}");
            }
        }

        /// <summary>
        /// スプライトに合わせてColliderの形状を更新
        /// Sprite EditorのCustom Physics Shapeを適用
        /// 複数のShapeにも対応（例: パイナップル = 果肉 + 葉）
        /// </summary>
        private void UpdateColliderShape()
        {
            // PolygonCollider2DをリセットしてスプライトPhysics Shapeから再生成
            if (polygonCollider != null && spriteRenderer.sprite != null)
            {
                // 既存のパスをクリア
                polygonCollider.pathCount = 0;

                // スプライトのPhysics Shapeを適用
                int shapeCount = spriteRenderer.sprite.GetPhysicsShapeCount();

                if (shapeCount > 0)
                {
                    // Physics Shapeが設定されている場合
                    // 複数のShape対応（パイナップル: 果肉 + 葉 など）
                    for (int i = 0; i < shapeCount; i++)
                    {
                        System.Collections.Generic.List<Vector2> path =
                            new System.Collections.Generic.List<Vector2>();
                        spriteRenderer.sprite.GetPhysicsShape(i, path);

                        // 有効なパスのみ設定（3点以上必要）
                        if (path.Count >= 3)
                        {
                            polygonCollider.SetPath(i, path);
                        }
                    }

                    if (showDebugLog)
                    {
                        Debug.Log($"[FruitData] {spriteRenderer.sprite.name}: {shapeCount}個のPhysics Shape適用");
                    }
                }
                else
                {
                    // Physics Shapeがない場合 - 警告
                    Debug.LogWarning($"[FruitData] {spriteRenderer.sprite.name} にPhysics Shapeがありません！Sprite Editorで設定してください");
                }
            }
        }

        /// <summary>
        /// フルーツがカットされた時に呼び出し
        /// </summary>
        public void OnSliced()
        {
            if (isSliced)
            {
                Debug.LogWarning($"[FruitData] すでにカット済み: {fruitInfo.fruitType}");
                return;
            }

            if (isInMixer)
            {
                Debug.LogWarning($"[FruitData] ミキサー内のフルーツはカット不可: {fruitInfo.fruitType}");
                return;
            }

            isSliced = true;
            currentTier = Mathf.Max(1, currentTier - 1); // 最小 tier 1

            if (showDebugLog)
            {
                Debug.Log($"[FruitData] カット: {fruitInfo.fruitType} → Tier {currentTier}");
            }
        }

        /// <summary>
        /// ミキサー進入時に呼び出し
        /// </summary>
        public void EnterMixer()
        {
            isInMixer = true;

            if (showDebugLog)
            {
                Debug.Log($"[FruitData] ミキサー進入: {fruitInfo.fruitType}, Tier {currentTier}");
            }
        }

        /// <summary>
        /// フルーツタイプ取得
        /// </summary>
        public FruitType GetFruitType()
        {
            return fruitInfo != null ? fruitInfo.fruitType : FruitType.None;
        }

        /// <summary>
        /// フルーツ情報取得
        /// </summary>
        public FruitInfo GetFruitInfo()
        {
            return fruitInfo;
        }

        /// <summary>
        /// 現在のティア取得
        /// </summary>
        public int GetTier()
        {
            return currentTier;
        }

        /// <summary>
        /// 現在のスプライト取得（ゲームオーバー画面用）
        /// </summary>
        public Sprite GetCurrentSprite()
        {
            return spriteRenderer != null ? spriteRenderer.sprite : null;
        }
    }
}