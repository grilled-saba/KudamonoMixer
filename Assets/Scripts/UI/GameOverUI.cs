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
            Time.timeScale = 1f;

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);

            if (titleButton != null)
                titleButton.onClick.AddListener(OnTitleClicked);

            SetEscapedFruitSprite();

            if (showDebugLog)
                Debug.Log("[GameOverUI] ゲームオーバーシーン初期化完了");
        }

        void OnDestroy()
        {
            if (retryButton != null)
                retryButton.onClick.RemoveListener(OnRetryClicked);

            if (titleButton != null)
                titleButton.onClick.RemoveListener(OnTitleClicked);
        }

        /// <summary>
        /// 脱出フルーツのスプライトを設定
        /// SceneTransferDataから直接取得（GameManager不要）
        /// </summary>
        private void SetEscapedFruitSprite()
        {
            if (escapedFruitImage == null)
            {
                Debug.LogWarning("[GameOverUI] escapedFruitImage が未設定");
                return;
            }

            Sprite escapedSprite = SceneTransferData.LastEscapedFruitSprite;

            if (escapedSprite != null)
            {
                escapedFruitImage.sprite = escapedSprite;
                escapedFruitImage.gameObject.SetActive(true);

                if (showDebugLog)
                    Debug.Log($"[GameOverUI] 脱出フルーツ設定: {escapedSprite.name}");
            }
            else
            {
                escapedFruitImage.gameObject.SetActive(false);
                Debug.LogWarning("[GameOverUI] 脱出フルーツスプライトがnull");
            }
        }

        private void OnRetryClicked()
        {
            if (showDebugLog)
                Debug.Log("[GameOverUI] リトライボタンクリック");

            SceneTransferData.LastEscapedFruitSprite = null;
            SceneManager.LoadScene("GameScene");
        }

        private void OnTitleClicked()
        {
            if (showDebugLog)
                Debug.Log("[GameOverUI] タイトルへボタンクリック");

            SceneTransferData.LastEscapedFruitSprite = null;

            if (Application.CanStreamedLevelBeLoaded("TitleScene"))
                SceneManager.LoadScene("TitleScene");
            else
            {
                Debug.LogWarning("[GameOverUI] TitleSceneが見つかりません。GameSceneをロードします。");
                SceneManager.LoadScene("GameScene");
            }
        }
    }
}