using UnityEngine;
using FruitMixer.AI;

namespace FruitMixer.Managers
{
    /// <summary>
    /// AI対戦モード専用のゲーム管理スクリプト
    /// シングルトンなし・DontDestroyOnLoadなし
    /// PlayerAreaのGameManagerとは独立して動作する
    /// </summary>
    public class AIGameManager : MonoBehaviour
    {
        // ==================== 状態管理 ====================
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

        [Header("対戦管理参照")]
        [Tooltip("対戦結果を通知するマネージャー（AIBattleManager）")]
        [SerializeField] private AIBattleManager battleManager;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        // ==================== ゲームオーバー ====================

        /// <summary>
        /// ゲームオーバー処理（AIエリア用）
        /// シーン遷移はせず、対戦マネージャーに通知する
        /// </summary>
        public void GameOver(Sprite escapedFruitSprite = null)
        {
            Debug.Log($"[AIGameManager] GameOver呼び出し confirmed - battleManager: {battleManager}");
            if (isGameOver) return;
            if (isTransitioning) return;

            isGameOver = true;
            isTransitioning = true;

            if (showDebugLog)
                Debug.Log("[AIGameManager] 💀 AIエリア: ゲームオーバー");

            // FruitMixerAgentにペナルティを通知
            FruitMixerAgent agent = GetComponentInChildren<FruitMixerAgent>();
            if (agent != null)
                agent.PenaltyLose();

            // 対戦マネージャーにAI負けを通知
            if (battleManager != null)
                battleManager.OnAILose();
        }

        // ==================== ドリアン管理 ====================

        /// <summary>
        /// ドリアン生成時に呼ばれる（AI対戦モード: 2個で勝利）
        /// </summary>
        public void OnDurianCreated()
        {
            durianCount++;
            Debug.Log($"[AIGameManager] OnDurianCreated confirmed - durianCount: {durianCount}, battleManager: {battleManager}");

            if (showDebugLog)
                Debug.Log($"[AIGameManager] 🏆 ドリアン生成! 現在: {durianCount}個");

            if (durianCount >= 2)
                GameWin();
        }

        /// <summary>
        /// 勝利処理（AI側が先にドリアン2個完成）
        /// </summary>
        public void GameWin()
        {
            if (showDebugLog)
                Debug.Log("[AIGameManager] 🏆🏆🏆 AI勝利!");

            FruitMixerAgent agent = GetComponentInChildren<FruitMixerAgent>();
            if (agent != null)
                agent.RewardWin();

            if (battleManager != null)
                battleManager.OnAIWin();
        }

        // ==================== 状態取得 ====================

        public bool IsGameOver() => isGameOver;
        public bool IsPaused() => isPaused;
        public bool IsTransitioning() => isTransitioning;
        public int GetDurianCount() => durianCount;
        public int GetDeleteCount() => currentDeleteCount;
        public int GetMaxDeleteCount() => maxDeleteCount;
        public int GetScore() => currentScore;

        // ==================== 削除カウント ====================

        public void AddDeleteCount()
        {
            if (currentDeleteCount < maxDeleteCount)
            {
                currentDeleteCount++;
                if (showDebugLog)
                    Debug.Log($"[AIGameManager] 🎯 削除カウント +1 → {currentDeleteCount}/{maxDeleteCount}");
            }
        }

        public bool UseDeleteCount()
        {
            if (currentDeleteCount > 0)
            {
                currentDeleteCount--;
                if (showDebugLog)
                    Debug.Log($"[AIGameManager] 🗑️ 削除カウント -1 → 残り: {currentDeleteCount}/{maxDeleteCount}");
                return true;
            }
            return false;
        }

        // ==================== スコア ====================

        public void AddScore(int score)
        {
            currentScore += score;
            if (showDebugLog)
                Debug.Log($"[AIGameManager] 💰 スコア +{score} → 合計: {currentScore}");
        }

        // ==================== 一時停止 ====================

        public void PauseGame()
        {
            if (isPaused) return;
            isPaused = true;
            if (showDebugLog)
                Debug.Log("[AIGameManager] ゲーム一時停止");
        }

        public void ResumeGame()
        {
            if (!isPaused) return;
            isPaused = false;
            if (showDebugLog)
                Debug.Log("[AIGameManager] ゲーム再開");
        }

        // ==================== リセット ====================

        public void ResetGame()
        {
            isGameOver = false;
            isPaused = false;
            isTransitioning = false;
            currentDeleteCount = 0;
            currentScore = 0;
            durianCount = 0;

            if (showDebugLog)
                Debug.Log("[AIGameManager] ゲーム状態リセット完了");
        }

        public void PrepareSceneTransition()
        {
            isTransitioning = true;
        }
    }
}