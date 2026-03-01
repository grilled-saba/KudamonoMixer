using FruitMixer.Core;
using FruitMixer.Gameplay;
using UnityEngine;

namespace FruitMixer.Managers
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MixerManager : MonoBehaviour
    {
        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        [Header("ゲームオーバー判定")]
        [Tooltip("ミキサー内滞在時間（秒）この時間以上いたフルーツが脱出するとゲームオーバー")]
        [SerializeField] private float minimumStayTime = 0.3f;

        [Header("AI対戦モード設定")]
        [Tooltip("AIエリアで使用する場合にAIGameManagerを設定（設定するとAIGameManagerのGameOverを呼び出す）")]
        [SerializeField] private AIGameManager aiGameManager = null;

        private BoxCollider2D mixerBoundary;

        // フルーツの滞在時間を記録
        private System.Collections.Generic.Dictionary<FruitData, float> fruitStayTimes
            = new System.Collections.Generic.Dictionary<FruitData, float>();

        void Awake()
        {
            mixerBoundary = GetComponent<BoxCollider2D>();
            mixerBoundary.isTrigger = true;
        }

        void Update()
        {
            // 滞在時間を更新
            var fruitsToUpdate = new System.Collections.Generic.List<FruitData>(fruitStayTimes.Keys);

            foreach (var fruit in fruitsToUpdate)
            {
                if (fruit != null && fruit.isInMixer)
                {
                    fruitStayTimes[fruit] += Time.deltaTime;

                    // 一定時間以上滞在したら記録
                    if (!fruit.hasEverEnteredMixer && fruitStayTimes[fruit] >= minimumStayTime)
                    {
                        fruit.hasEverEnteredMixer = true;

                        if (showDebugLog)
                        {
                            Debug.Log($"[MixerManager] ✅ {fruit.GetFruitType()} がミキサー定着（{minimumStayTime}秒経過）");
                        }
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            FruitData fruit = other.GetComponent<FruitData>();
            if (fruit != null)
            {
                fruit.isInMixer = true;

                // 滞在時間カウント開始
                if (!fruitStayTimes.ContainsKey(fruit))
                {
                    fruitStayTimes[fruit] = 0f;
                }

                if (showDebugLog)
                {
                    Debug.Log($"[MixerManager] 🍎 {fruit.GetFruitType()} がミキサー進入");
                }
                return;
            }

            BombData bomb = other.GetComponent<BombData>();
            if (bomb != null)
            {
                bomb.isInMixer = true;

                if (showDebugLog)
                {
                    Debug.Log($"[MixerManager] 💣 爆弾がミキサー進入");
                }
                return;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Unityエディタで停止中は無視
            if (!Application.isPlaying) return;

            // AIエリアの場合はAIGameManagerを使用
            if (aiGameManager != null)
            {
                if (aiGameManager.IsGameOver()) return;
                if (aiGameManager.IsTransitioning()) return;

                FruitData fruit = other.GetComponent<FruitData>();
                if (fruit != null)
                {
                    fruit.isInMixer = false;

                    if (fruitStayTimes.ContainsKey(fruit))
                        fruitStayTimes.Remove(fruit);

                    if (fruit.hasEverEnteredMixer)
                    {
                        Debug.LogError($"[MixerManager] 💀 {fruit.GetFruitType()} がAIミキサーから脱出! → GAME OVER");
                        aiGameManager.GameOver(fruit.GetCurrentSprite());
                    }
                    return;
                }

                BombData bomb = other.GetComponent<BombData>();
                if (bomb != null)
                {
                    bomb.isInMixer = false;
                    if (showDebugLog)
                        Debug.Log($"[MixerManager] 💣 爆弾がAIミキサー脱出");
                    return;
                }
                return;
            }

            // プレイヤーエリアの場合はGameManager.Instanceを使用
            // GameManagerが破壊されている場合は無視（シーン終了時）
            if (GameManager.Instance == null) return;

            // ✨ 既にゲームオーバー状態なら無視（シーン遷移中の誤検出防止）
            if (GameManager.Instance.IsGameOver()) return;

            // ✨ シーン遷移中なら無視（誤GameOver防止）
            if (GameManager.Instance.IsTransitioning()) return;

            FruitData playerFruit = other.GetComponent<FruitData>();
            if (playerFruit != null)
            {
                playerFruit.isInMixer = false;

                if (fruitStayTimes.ContainsKey(playerFruit))
                {
                    fruitStayTimes.Remove(playerFruit);
                }

                if (playerFruit.hasEverEnteredMixer)
                {
                    Debug.LogError($"[MixerManager] 💀 {playerFruit.GetFruitType()} がミキサーから脱出! → GAME OVER");
                    Sprite escapedSprite = playerFruit.GetCurrentSprite();
                    GameManager.Instance.GameOver(escapedSprite);
                }
                return;
            }

            BombData playerBomb = other.GetComponent<BombData>();
            if (playerBomb != null)
            {
                playerBomb.isInMixer = false;

                if (showDebugLog)
                {
                    Debug.Log($"[MixerManager] 💣 爆弾がミキサー脱出");
                }
                return;
            }
        }

        /// <summary>
        /// ミキサー内のフルーツリストを取得（Agent観察用）
        /// </summary>
        public System.Collections.Generic.List<FruitData> GetFruitsInMixer()
        {
            return new System.Collections.Generic.List<FruitData>(fruitStayTimes.Keys);
        }
    }
}