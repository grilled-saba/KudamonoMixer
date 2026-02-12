using FruitMixer.Core;
using FruitMixer.Managers;
using UnityEngine;

namespace FruitMixer.Gameplay
{
    /// <summary>
    /// フルーツ合成システム（Suika Game風）
    /// 同じティアのフルーツが衝突 → 上位ティア生成
    /// </summary>
    [RequireComponent(typeof(FruitData))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class FruitMerger : MonoBehaviour
    {
        [Header("合成設定")]
        [Tooltip("合成時の上方向の力")]
        [SerializeField] private float mergeUpwardForce = 3f;

        [Tooltip("合成時のスコア倍率（Tier * この値）")]
        [SerializeField] private int scoreMultiplier = 10;

        [Header("サウンド")]
        [Tooltip("合成成功サウンド（オプション）")]
        [SerializeField] private AudioClip mergeSound;

        [Header("参照")]
        [Tooltip("フルーツPrefab")]
        [SerializeField] private GameObject fruitPrefab;

        [Tooltip("FruitDatabase")]
        [SerializeField] private FruitDatabase fruitDatabase;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        private FruitData fruitData;
        private bool hasMerged = false; // 1回だけ合成

        void Awake()
        {
            fruitData = GetComponent<FruitData>();

            // fruitPrefab/fruitDatabase自動取得（halfA/halfB問題の解決）
            // Unity特有: destroyed objectもnullでないのでUnityのbool演算子を使用
            if (!fruitPrefab || fruitDatabase == null)
            {
                FruitSpawner spawner = FindFirstObjectByType<FruitSpawner>();
                if (spawner != null && spawner.fruitPrefab)
                {
                    // FruitPrefabから参照を取得
                    FruitMerger prefabMerger = spawner.fruitPrefab.GetComponent<FruitMerger>();
                    if (prefabMerger != null)
                    {
                        // fruitPrefab取得
                        if (!fruitPrefab)
                        {
                            fruitPrefab = spawner.fruitPrefab;
                        }

                        // fruitDatabase取得
                        if (fruitDatabase == null && prefabMerger.fruitDatabase != null)
                        {
                            fruitDatabase = prefabMerger.fruitDatabase;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[FruitMerger] FruitSpawner が見つからないか、fruitPrefab が未設定です");
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // 既に合成済みなら無視
            if (hasMerged) return;

            // 相手がフルーツか確認
            FruitData otherFruit = collision.gameObject.GetComponent<FruitData>();
            if (otherFruit == null) return;

            // 相手も合成済みなら無視
            FruitMerger otherMerger = collision.gameObject.GetComponent<FruitMerger>();
            if (otherMerger != null && otherMerger.hasMerged) return;

            // 合成可能かチェック
            if (CanMerge(fruitData, otherFruit))
            {
                // 合成実行
                MergeFruits(fruitData, otherFruit, collision.contacts[0].point);

                // 両方とも合成済みフラグ
                hasMerged = true;
                if (otherMerger != null)
                {
                    otherMerger.hasMerged = true;
                }
            }
        }

        /// <summary>
        /// 合成可能かチェック
        /// </summary>
        private bool CanMerge(FruitData fruit1, FruitData fruit2)
        {
            // 両方ともミキサー内にいるか
            if (!fruit1.isInMixer || !fruit2.isInMixer)
            {
                if (showDebugLog)
                {
                    Debug.Log($"[FruitMerger] ❌ 合成不可: ミキサー外");
                }
                return false;
            }

            // 同じティアか
            if (fruit1.currentTier != fruit2.currentTier)
            {
                if (showDebugLog)
                {
                    Debug.Log($"[FruitMerger] ❌ 合成不可: Tier不一致 ({fruit1.currentTier} ≠ {fruit2.currentTier})");
                }
                return false;
            }

            // Tier 10（Durian）は合成不可
            if (fruit1.currentTier >= 10)
            {
                if (showDebugLog)
                {
                    Debug.Log($"[FruitMerger] ❌ 合成不可: Tier 10 は最終フルーツ");
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// フルーツを合成
        /// </summary>
        private void MergeFruits(FruitData fruit1, FruitData fruit2, Vector2 collisionPoint)
        {
            int currentTier = fruit1.currentTier;
            int newTier = currentTier + 1;

            if (showDebugLog)
            {
                Debug.Log($"[FruitMerger] 🎉 合成開始: {fruit1.GetFruitType()} (T{currentTier}) + {fruit2.GetFruitType()} (T{currentTier}) → T{newTier}");
            }

            // 新しいフルーツ情報を取得
            FruitInfo newFruitInfo = fruitDatabase.GetFruitByTier(newTier);

            if (newFruitInfo == null)
            {
                Debug.LogError($"[FruitMerger] ❌ Tier {newTier} のフルーツが見つかりません！");
                return;
            }

            // 中間位置計算
            Vector2 midPoint = (fruit1.transform.position + fruit2.transform.position) / 2f;


            // fruitPrefab検証（destroyed object対策）
            if (!fruitPrefab)
            {
                Debug.LogError("[FruitMerger] ❌ fruitPrefab が無効です（destroyed or null）！合成を中止します");
                return;
            }
            // 新しいフルーツ生成
            GameObject newFruitObj = Instantiate(fruitPrefab, midPoint, Quaternion.identity);
            FruitData newFruitData = newFruitObj.GetComponent<FruitData>();

            if (newFruitData != null)
            {
                // 初期化
                newFruitData.Initialize(newFruitInfo);

                // ⚠️ 重要: 既にミキサー内で生成されたので isInMixer = true
                newFruitData.isInMixer = true;
                newFruitData.hasEverEnteredMixer = true; // 合成で生成されたフルーツも定着扱い

                // 上方向の力を加える（生成時に"ぴょこん"）
                Rigidbody2D newRb = newFruitObj.GetComponent<Rigidbody2D>();
                if (newRb != null)
                {
                    newRb.AddForce(Vector2.up * mergeUpwardForce, ForceMode2D.Impulse);
                }

                if (showDebugLog)
                {
                    Debug.Log($"[FruitMerger] ✅ 合成成功: {newFruitInfo.fruitType} (Tier {newTier}) 生成！");
                    Debug.Log($"[FruitMerger] 📍 新フルーツ位置: {midPoint}, isInMixer: {newFruitData.isInMixer}");
                }
            }

            // スコア追加
            int mergeScore = newTier * scoreMultiplier;
            GameManager.Instance.AddScore(mergeScore);

            if (showDebugLog)
            {
                Debug.Log($"[FruitMerger] 💰 スコア +{mergeScore}");
            }

            // サウンド再生（設定されていれば）
            if (mergeSound != null)
            {
                AudioSource.PlayClipAtPoint(mergeSound, midPoint);
            }

            // Tier 10（Durian）完成チェック
            if (newTier == 10)
            {
                Debug.Log($"[FruitMerger] 🏆🏆🏆 Durian完成！おめでとうございます！");

                // GameManagerにドリアン生成を通知
                GameManager.Instance.OnDurianCreated();
            }

            // ⚠️ 重要: ゲームオーバー判定を避けるため、削除前にフラグをリセット
            fruit1.hasEverEnteredMixer = false;
            fruit2.hasEverEnteredMixer = false;

            // 元のフルーツを削除
            Destroy(fruit1.gameObject);
            Destroy(fruit2.gameObject);
        }
    }
}