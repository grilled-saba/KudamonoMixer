using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FruitMixer.Managers;

namespace FruitMixer.UI
{
    /// <summary>
    /// ゲームオーバーシーンUI管理
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI要素")]
        [Tooltip("タイトルテキスト (GAME OVER!)")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Tooltip("脱出フルーツ画像")]
        [SerializeField] private Image escapedFruitImage;

        [Tooltip("リトライボタン")]
        [SerializeField] private Button retryButton;

        [Tooltip("タイトルへボタン")]
        [SerializeField] private Button titleButton;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        void Start()
        {
            // タイムスケールリセット
            Time.timeScale = 1f;

            // ボタンイベント登録
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (titleButton != null)
            {
                titleButton.onClick.AddListener(OnTitleClicked);
            }

            // 脱出フルーツ画像設定
            SetEscapedFruitSprite();

            if (showDebugLog)
            {
                Debug.Log("[GameOverUI] ゲームオーバーシーン初期化完了");
            }
        }

        void OnDestroy()
        {
            // イベント解除
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(OnRetryClicked);
            }

            if (titleButton != null)
            {
                titleButton.onClick.RemoveListener(OnTitleClicked);
            }
        }

        /// <summary>
        /// 脱出フルーツのスプライトを設定
        /// </summary>
        private void SetEscapedFruitSprite()
        {
            if (escapedFruitImage == null)
            {
                Debug.LogWarning("[GameOverUI] escapedFruitImage が未設定");
                return;
            }

            if (GameManager.Instance != null)
            {
                Sprite escapedSprite = GameManager.Instance.GetEscapedFruitSprite();

                if (escapedSprite != null)
                {
                    escapedFruitImage.sprite = escapedSprite;
                    escapedFruitImage.gameObject.SetActive(true);

                    if (showDebugLog)
                    {
                        Debug.Log($"[GameOverUI] 脱出フルーツ設定: {escapedSprite.name}");
                    }
                }
                else
                {
                    // スプライトがない場合は画像非表示
                    escapedFruitImage.gameObject.SetActive(false);
                    Debug.LogWarning("[GameOverUI] 脱出フルーツスプライトがnull");
                }
            }
            else
            {
                escapedFruitImage.gameObject.SetActive(false);
                Debug.LogWarning("[GameOverUI] GameManager.Instance が null");
            }
        }

        /// <summary>
        /// リトライボタンクリック
        /// </summary>
        private void OnRetryClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[GameOverUI] リトライボタンクリック");
            }

            // GameManager 状態をリセット
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGame();
            }

            // GameSceneをリロード
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// タイトルへボタンクリック
        /// </summary>
        private void OnTitleClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[GameOverUI] タイトルへボタンクリック");
            }

            // GameManager 状態をリセット
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGame();
            }

            // TitleSceneへ遷移（まだ作成されていない場合はGameSceneへ）
            if (Application.CanStreamedLevelBeLoaded("TitleScene"))
            {
                SceneManager.LoadScene("TitleScene");
            }
            else
            {
                Debug.LogWarning("[GameOverUI] TitleSceneが見つかりません。GameSceneをロードします。");
                SceneManager.LoadScene("GameScene");
            }
        }
    }
}