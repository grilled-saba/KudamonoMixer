using UnityEngine;
using FruitMixer.AI;

namespace FruitMixer.Managers
{
    /// <summary>
    /// ゲーム全体を管理するシングルトン
    /// DontDestroyOnLoadなし - 各シーンで独立して初期化
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ==================== ゲームモード ====================
        public enum GameMode { SinglePlay, AIBattle }
        [SerializeField] private GameMode gameMode = GameMode.SinglePlay;

        [Header("ゲーム状態")]
        private bool isGameOver = false;
        private bool isPaused = false;
        private bool isTransitioning = false;

        [Header("削除カウント管理")]
        [Tooltip("ミキサー内削除可能回数（最大値）")]
        [SerializeField] private int maxDeleteCount = 3;
        private int currentDeleteCount = 0;

        [Header("スコア管理")]
        private int currentScore = 0;

        [Header("ゲーム統計")]
        private int durianCount = 0;

        [Header("UI参照")]
        [Tooltip("ゲームクリアウィンドウ")]
        [SerializeField] private GameObject gameClearWindow;

        [Tooltip("AI対戦マネージャー（AIBattleSceneのみ使用）")]
        [SerializeField] private AIBattleManager battleManager;

        [Header("ローカルインスタンス設定")]
        [Tooltip("AIBattleSceneのPlayerAreaで使用。チェックするとシングルトンをスキップ")]
        [SerializeField] private bool isLocalInstance = false;

        void Awake()
        {
            Time.timeScale = 1f; 

            if (isLocalInstance)
            {
                Instance = this;
                return;
            }

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// ゲームオーバー処理
        /// </summary>
        public void GameOver(Sprite escapedFruitSprite = null)
        {
            if (isGameOver) return;
            if (isTransitioning) return;

            isGameOver = true;
            isTransitioning = true;

            FruitMixerAgent agent = FindFirstObjectByType<FruitMixerAgent>();
            if (agent != null)
            {
                agent.PenaltyLose();
            }

            // AI対戦モードはシーン遷移しない
            if (gameMode == GameMode.AIBattle)
            {
                Debug.Log("[GameManager] 💀 AI対戦モード: プレイヤーゲームオーバー");
                if (battleManager != null)
                    battleManager.OnPlayerLose();
                return;
            }

            // SceneTransferDataに脱出フルーツを保存
            SceneTransferData.LastEscapedFruitSprite = escapedFruitSprite;

            Time.timeScale = 0f;
            Debug.LogError("💀💀💀 GAME OVER! フルーツがミキサーから脱出しました！");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
        }

        /// <summary>
        /// 脱出したフルーツのスプライトを取得（後方互換性のため残す）
        /// </summary>
        public Sprite GetEscapedFruitSprite()
        {
            return SceneTransferData.LastEscapedFruitSprite;
        }

        public bool IsGameOver() => isGameOver;
        public bool IsPaused() => isPaused;
        public bool IsTransitioning() => isTransitioning;

        public void RestartGame()
        {
            isGameOver = false;
            Time.timeScale = 1f;
            Debug.Log("[GameManager] ゲームリスタート");
        }

        public void ResetGame()
        {
            isGameOver = false;
            isPaused = false;
            currentDeleteCount = 0;
            currentScore = 0;
            durianCount = 0;
            SceneTransferData.LastEscapedFruitSprite = null;
            Time.timeScale = 1f;
            Debug.Log("[GameManager] ゲーム状態リセット完了");
        }

        public void SetGameMode(GameMode mode)
        {
            gameMode = mode;
            Debug.Log($"[GameManager] ゲームモード設定: {mode}");
        }

        public void PrepareSceneTransition()
        {
            isTransitioning = true;
            Debug.Log("[GameManager] シーン遷移準備完了");
        }

        // ==================== ドリアン管理 ====================

        public void OnDurianCreated()
        {
            durianCount++;
            Debug.Log($"[GameManager] 🏆 ドリアン生成! 現在: {durianCount}個");

            if (gameMode == GameMode.AIBattle && durianCount >= 2)
            {
                GameWin();
            }
            else if (gameMode == GameMode.SinglePlay && durianCount == 1)
            {
                ShowClearWindow();
            }
        }

        public void GameWin()
        {
            Debug.Log("[GameManager] 🏆🏆🏆 GameWin呼び出し！");

            FruitMixerAgent agent = FindFirstObjectByType<FruitMixerAgent>();
            if (agent != null)
            {
                agent.RewardWin();
            }

            if (battleManager != null)
            {
                battleManager.OnPlayerWin();
            }
        }

        private void ShowClearWindow()
        {
            Debug.Log("[GameManager] 🏆🏆🏆 初ドリアン完成！クリアウィンドウ表示");

            if (gameClearWindow != null)
            {
                UI.GameClearWindow clearWindow = gameClearWindow.GetComponent<UI.GameClearWindow>();
                if (clearWindow != null)
                {
                    clearWindow.ShowWindow();
                }
                else
                {
                    Debug.LogError("[GameManager] GameClearWindowコンポーネントが見つかりません");
                }
            }
            else
            {
                Debug.LogError("[GameManager] gameClearWindowが未設定です");
            }
        }

        public int GetDurianCount() => durianCount;

        // ==================== 削除カウント管理 ====================

        public void AddDeleteCount()
        {
            if (currentDeleteCount < maxDeleteCount)
            {
                currentDeleteCount++;
                Debug.Log($"[GameManager] 🎯 削除カウント +1 → 現在: {currentDeleteCount}/{maxDeleteCount}");
            }
            else
            {
                Debug.Log($"[GameManager] ⚠️ 削除カウント満タン: {maxDeleteCount}/{maxDeleteCount}");
            }
        }

        public bool UseDeleteCount()
        {
            if (currentDeleteCount > 0)
            {
                currentDeleteCount--;
                Debug.Log($"[GameManager] 🗑️ 削除カウント -1 → 残り: {currentDeleteCount}/{maxDeleteCount}");
                return true;
            }
            else
            {
                Debug.Log($"[GameManager] ❌ 削除カウント不足: {currentDeleteCount}/{maxDeleteCount}");
                return false;
            }
        }

        public int GetDeleteCount() => currentDeleteCount;
        public int GetMaxDeleteCount() => maxDeleteCount;

        // ==================== スコア管理 ====================

        public void AddScore(int score)
        {
            currentScore += score;
            Debug.Log($"[GameManager] 💰 スコア +{score} → 合計: {currentScore}");
        }

        public int GetScore() => currentScore;

        public void ResetScore()
        {
            currentScore = 0;
            Debug.Log("[GameManager] スコアリセット");
        }

        // ==================== 一時停止機能 ====================

        public void PauseGame()
        {
            if (isPaused) return;
            isPaused = true;
            Time.timeScale = 0f;
            Debug.Log("[GameManager] ゲーム一時停止");
        }

        public void ResumeGame()
        {
            if (!isPaused) return;
            isPaused = false;
            Time.timeScale = 1f;
            Debug.Log("[GameManager] ゲーム再開");
        }
    }
}