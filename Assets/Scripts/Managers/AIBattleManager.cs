using UnityEngine;

namespace FruitMixer.Managers
{
    /// <summary>
    /// AI対戦の勝敗を統合管理するスクリプト
    /// PlayerAreaのGameManagerとAIAreaのAIGameManagerの両方から通知を受け取る
    /// </summary>
    public class AIBattleManager : MonoBehaviour
    {
        [Header("結果UI参照")]
        [Tooltip("対戦結果ポップアップ（勝敗表示用）")]
        [SerializeField] private GameObject battleResultPopup;

        [Tooltip("結果テキスト（TextMeshPro）")]
        [SerializeField] private TMPro.TextMeshProUGUI resultText;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        private bool battleEnded = false;

        // ==================== 勝敗通知 ====================

        /// <summary>
        /// プレイヤーが勝利した時（ドリアン2個完成）
        /// </summary>
        public void OnPlayerWin()
        {
            if (battleEnded) return;
            battleEnded = true;

            if (showDebugLog)
                Debug.Log("[AIBattleManager] 🏆 プレイヤー勝利!");

            ShowResult("YOU WIN!");
        }

        /// <summary>
        /// プレイヤーがゲームオーバーになった時
        /// </summary>
        public void OnPlayerLose()
        {
            if (battleEnded) return;
            battleEnded = true;

            if (showDebugLog)
                Debug.Log("[AIBattleManager] 💀 プレイヤー敗北");

            ShowResult("YOU LOSE...");
        }

        /// <summary>
        /// AIが勝利した時（ドリアン2個完成）
        /// </summary>
        public void OnAIWin()
        {
            if (battleEnded) return;
            battleEnded = true;

            if (showDebugLog)
                Debug.Log("[AIBattleManager] 🤖 AI勝利");

            ShowResult("YOU LOSE...");
        }

        /// <summary>
        /// AIがゲームオーバーになった時
        /// </summary>
        public void OnAILose()
        {
            if (battleEnded) return;
            battleEnded = true;

            if (showDebugLog)
                Debug.Log("[AIBattleManager] 🤖 AI敗北");

            ShowResult("YOU WIN!");
        }

        // ==================== 結果表示 ====================

        private void ShowResult(string message)
        {
            Time.timeScale = 0f;

            if (resultText != null)
                resultText.text = message;

            if (battleResultPopup != null)
                battleResultPopup.SetActive(true);

            if (showDebugLog)
                Debug.Log($"[AIBattleManager] 結果表示: {message}");
        }

        /// <summary>
        /// もう一度ボタン：AIBattleSceneを再ロード
        /// </summary>
        public void OnRetryClicked()
        {
            Time.timeScale = 1f;
            battleEnded = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene("AIBattleScene");
        }

        /// <summary>
        /// タイトルへボタン：TitleSceneへ遷移
        /// </summary>
        public void OnTitleClicked()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
        }
    }
}