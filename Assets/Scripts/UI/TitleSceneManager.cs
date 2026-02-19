using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FruitMixer.Managers;

namespace FruitMixer.UI
{
    /// <summary>
    /// タイトルシーン管理
    /// ゲーム開始・音量設定・終了を担当
    /// </summary>
    public class TitleSceneManager : MonoBehaviour
    {
        [Header("ボタン")]
        [Tooltip("ゲーム開始ボタン")]
        [SerializeField] private Button gameStartButton;

        [Tooltip("音量設定ボタン")]
        [SerializeField] private Button volumeButton;

        [Tooltip("ゲーム終了ボタン")]
        [SerializeField] private Button quitButton;

        [Header("音量パネル")]
        [Tooltip("音量設定パネル (GameObject)")]
        [SerializeField] private GameObject volumePanel;

        [Tooltip("マスターボリュームスライダー")]
        [SerializeField] private Slider volumeSlider;

        [Tooltip("音量数値テキスト (任意)")]
        [SerializeField] private TextMeshProUGUI volumeValueText;

        [Tooltip("音量パネルを閉じるボタン")]
        [SerializeField] private Button volumeCloseButton;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        // 音量パネルの表示状態
        private bool isVolumePanelOpen = false;

        void Start()
        {
            // ボタンイベント登録
            if (gameStartButton != null)
                gameStartButton.onClick.AddListener(OnGameStartClicked);

            if (volumeButton != null)
                volumeButton.onClick.AddListener(OnVolumeClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (volumeCloseButton != null)
                volumeCloseButton.onClick.AddListener(CloseVolumePanel);

            // ボリュームスライダー初期設定
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

                // AudioManagerの現在値を反映
                float currentVolume = AudioManager.Instance != null
                    ? AudioManager.Instance.GetBGMVolume()
                    : 0.5f;
                volumeSlider.value = currentVolume;
                UpdateVolumeText(currentVolume);
            }

            // 音量パネルは非表示で開始
            if (volumePanel != null)
                volumePanel.SetActive(false);

            if (showDebugLog)
                Debug.Log("[TitleSceneManager] タイトルシーン初期化完了");
        }

        void OnDestroy()
        {
            // イベント解除
            if (gameStartButton != null)
                gameStartButton.onClick.RemoveListener(OnGameStartClicked);

            if (volumeButton != null)
                volumeButton.onClick.RemoveListener(OnVolumeClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);

            if (volumeCloseButton != null)
                volumeCloseButton.onClick.RemoveListener(CloseVolumePanel);

            if (volumeSlider != null)
                volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        // ==================== ボタンイベント ====================

        /// <summary>
        /// ゲーム開始ボタン
        /// </summary>
        private void OnGameStartClicked()
        {
            if (showDebugLog)
                Debug.Log("[TitleSceneManager] ゲーム開始");

            // GameManagerのゲーム状態リセット
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PrepareSceneTransition();
                GameManager.Instance.ResetGame();
            }

            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// 音量設定ボタン
        /// </summary>
        private void OnVolumeClicked()
        {
            if (showDebugLog)
                Debug.Log("[TitleSceneManager] 音量パネル切替");

            if (isVolumePanelOpen)
                CloseVolumePanel();
            else
                OpenVolumePanel();
        }

        /// <summary>
        /// ゲーム終了ボタン
        /// </summary>
        private void OnQuitClicked()
        {
            if (showDebugLog)
                Debug.Log("[TitleSceneManager] ゲーム終了");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ==================== 音量パネル ====================

        /// <summary>
        /// 音量パネルを開く
        /// </summary>
        private void OpenVolumePanel()
        {
            if (volumePanel == null) return;

            volumePanel.SetActive(true);
            isVolumePanelOpen = true;

            if (showDebugLog)
                Debug.Log("[TitleSceneManager] 音量パネル表示");
        }

        /// <summary>
        /// 音量パネルを閉じる
        /// </summary>
        public void CloseVolumePanel()
        {
            if (volumePanel == null) return;

            volumePanel.SetActive(false);
            isVolumePanelOpen = false;

            if (showDebugLog)
                Debug.Log("[TitleSceneManager] 音量パネル非表示");
        }

        /// <summary>
        /// ボリューム値変更時
        /// </summary>
        private void OnVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetBGMVolume(value);

            UpdateVolumeText(value);

            if (showDebugLog)
                Debug.Log($"[TitleSceneManager] 音量変更: {value:F2}");
        }

        /// <summary>
        /// 音量テキスト更新
        /// </summary>
        private void UpdateVolumeText(float value)
        {
            if (volumeValueText != null)
                volumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
}