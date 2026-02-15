using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FruitMixer.Managers;

namespace FruitMixer.UI
{
    /// <summary>
    /// ゲームクリアウィンドウ管理
    /// 初ドリアン生成時に表示されるポップアップ
    /// </summary>
    public class GameClearWindow : MonoBehaviour
    {
        [Header("UI要素")]
        [Tooltip("メインテキスト (GAME CLEAR!)")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("サブテキスト (初ドリアンメッセージ)")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Tooltip("続けるボタン")]
        [SerializeField] private Button continueButton;

        [Tooltip("タイトルへボタン")]
        [SerializeField] private Button titleButton;

        [Tooltip("閉じるボタン (X)")]
        [SerializeField] private Button closeButton;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        void Start()
        {
            // ボタンイベント登録
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (titleButton != null)
            {
                titleButton.onClick.AddListener(OnTitleClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            // 初期状態はUnityエディタで非表示に設定（SetActiveは使わない）
        }

        void OnDestroy()
        {
            // イベント解除
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
            }

            if (titleButton != null)
            {
                titleButton.onClick.RemoveListener(OnTitleClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public void ShowWindow()
        {
            gameObject.SetActive(true);

            // ✨ ゲームを一時停止
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }

            if (showDebugLog)
            {
                Debug.Log("[GameClearWindow] クリアウィンドウ表示");
            }
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        public void CloseWindow()
        {
            gameObject.SetActive(false);

            // ✨ ゲームを再開
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }

            if (showDebugLog)
            {
                Debug.Log("[GameClearWindow] クリアウィンドウ閉じる");
            }
        }

        /// <summary>
        /// 続けるボタンクリック
        /// </summary>
        private void OnContinueClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[GameClearWindow] 続けるボタンクリック");
            }

            CloseWindow();
        }

        /// <summary>
        /// タイトルへボタンクリック
        /// </summary>
        private void OnTitleClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[GameClearWindow] タイトルへボタンクリック");
            }

            // 警告ダイアログ表示（Phase 9.5で実装予定）
            // とりあえずそのままタイトルへ
            ReturnToTitle();
        }

        /// <summary>
        /// 閉じるボタン (X) クリック
        /// </summary>
        private void OnCloseClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[GameClearWindow] 閉じるボタンクリック");
            }

            CloseWindow();
        }

        /// <summary>
        /// タイトルへ戻る
        /// </summary>
        private void ReturnToTitle()
        {
            if (showDebugLog)
            {
                Debug.Log("[GameClearWindow] シーン遷移開始");
            }

            // ✨ シーン遷移準備（GameOver誤発火防止）
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PrepareSceneTransition();
                GameManager.Instance.ResetGame();
            }

            // TitleSceneへ遷移（まだ作成されていない場合はGameSceneへ）
            if (Application.CanStreamedLevelBeLoaded("TitleScene"))
            {
                SceneManager.LoadScene("TitleScene");
            }
            else
            {
                Debug.LogWarning("[GameClearWindow] TitleSceneが見つかりません。GameSceneをリロードします。");
                SceneManager.LoadScene("GameScene");
            }
        }
    }
}