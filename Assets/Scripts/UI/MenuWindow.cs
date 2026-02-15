using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FruitMixer.Managers;

namespace FruitMixer.UI
{
    /// <summary>
    /// メニューウィンドウ管理
    /// ゲームプレイ中にESCまたはメニューボタンで表示
    /// </summary>
    public class MenuWindow : MonoBehaviour
    {
        [Header("UI要素")]
        [Tooltip("チュートリアルパネル")]
        [SerializeField] private GameObject tutorialPanel;

        [Tooltip("ボリューム設定パネル")]
        [SerializeField] private GameObject volumePanel;

        [Tooltip("マスターボリュームスライダー")]
        [SerializeField] private Slider volumeSlider;

        [Tooltip("ボリューム数値テキスト")]
        [SerializeField] private TextMeshProUGUI volumeValueText;

        [Header("ボタン")]
        [Tooltip("チュートリアル表示ボタン")]
        [SerializeField] private Button tutorialButton;

        [Tooltip("ボリューム設定ボタン")]
        [SerializeField] private Button volumeButton;

        [Tooltip("ゲームに戻るボタン")]
        [SerializeField] private Button resumeButton;

        [Tooltip("タイトルへ戻るボタン")]
        [SerializeField] private Button titleButton;

        [Tooltip("ゲーム終了ボタン")]
        [SerializeField] private Button quitButton;

        [Tooltip("閉じるボタン (X)")]
        [SerializeField] private Button closeButton;

        [Header("確認ダイアログ")]
        [Tooltip("タイトル確認ダイアログ")]
        [SerializeField] private GameObject titleConfirmDialog;

        [Tooltip("終了確認ダイアログ")]
        [SerializeField] private GameObject quitConfirmDialog;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        // 現在表示中のパネル
        private enum Panel { None, Tutorial, Volume }
        private Panel currentPanel = Panel.None;

        void Start()
        {
            // ボタンイベント登録
            if (tutorialButton != null)
                tutorialButton.onClick.AddListener(OnTutorialClicked);

            if (volumeButton != null)
                volumeButton.onClick.AddListener(OnVolumeClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (titleButton != null)
                titleButton.onClick.AddListener(OnTitleClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            // ボリュームスライダー
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
                // 初期値設定
                volumeSlider.value = AudioManager.Instance != null ?
                    AudioManager.Instance.GetBGMVolume() : 0.5f;
                UpdateVolumeText(volumeSlider.value);
            }

            // 初期状態: 全パネル非表示
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            if (volumePanel != null)
                volumePanel.SetActive(false);

            if (titleConfirmDialog != null)
                titleConfirmDialog.SetActive(false);

            if (quitConfirmDialog != null)
                quitConfirmDialog.SetActive(false);
        }

        void Update()
        {
            // ESCキーでメニュー開閉
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (gameObject.activeSelf)
                {
                    // メニューが開いている場合
                    if (currentPanel != Panel.None)
                    {
                        // パネルが開いている → パネルを閉じる
                        CloseCurrentPanel();
                    }
                    else
                    {
                        // パネルが開いていない → メニューを閉じる
                        CloseWindow();
                    }
                }
                else
                {
                    // メニューが閉じている → メニューを開く
                    ShowWindow();
                }
            }
        }

        void OnDestroy()
        {
            // イベント解除
            if (tutorialButton != null)
                tutorialButton.onClick.RemoveListener(OnTutorialClicked);

            if (volumeButton != null)
                volumeButton.onClick.RemoveListener(OnVolumeClicked);

            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);

            if (titleButton != null)
                titleButton.onClick.RemoveListener(OnTitleClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);

            if (volumeSlider != null)
                volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public void ShowWindow()
        {
            gameObject.SetActive(true);

            // ゲームを一時停止
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }

            // 全パネル閉じる
            CloseAllPanels();

            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] メニューウィンドウ表示");
            }
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        public void CloseWindow()
        {
            // 全パネル閉じる
            CloseAllPanels();

            gameObject.SetActive(false);

            // ゲームを再開
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }

            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] メニューウィンドウ閉じる");
            }
        }

        /// <summary>
        /// チュートリアルボタンクリック
        /// </summary>
        private void OnTutorialClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] チュートリアルボタンクリック");
            }

            CloseAllPanels();
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
                currentPanel = Panel.Tutorial;
            }
        }

        /// <summary>
        /// ボリューム設定ボタンクリック
        /// </summary>
        private void OnVolumeClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] ボリューム設定ボタンクリック");
            }

            CloseAllPanels();
            if (volumePanel != null)
            {
                volumePanel.SetActive(true);
                currentPanel = Panel.Volume;
            }
        }

        /// <summary>
        /// ゲームに戻るボタンクリック
        /// </summary>
        private void OnResumeClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] ゲームに戻るボタンクリック");
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
                Debug.Log("[MenuWindow] タイトルへボタンクリック");
            }

            // 確認ダイアログ表示
            if (titleConfirmDialog != null)
            {
                titleConfirmDialog.SetActive(true);
            }
            else
            {
                // ダイアログがない場合は直接実行
                ReturnToTitle();
            }
        }

        /// <summary>
        /// ゲーム終了ボタンクリック
        /// </summary>
        private void OnQuitClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] ゲーム終了ボタンクリック");
            }

            // 確認ダイアログ表示
            if (quitConfirmDialog != null)
            {
                quitConfirmDialog.SetActive(true);
            }
            else
            {
                // ダイアログがない場合は直接実行
                QuitGame();
            }
        }

        /// <summary>
        /// 閉じるボタン (X) クリック
        /// </summary>
        private void OnCloseClicked()
        {
            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] 閉じるボタンクリック");
            }

            CloseWindow();
        }

        /// <summary>
        /// ボリューム変更
        /// </summary>
        private void OnVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(value);
            }

            UpdateVolumeText(value);

            if (showDebugLog)
            {
                Debug.Log($"[MenuWindow] ボリューム変更: {value:F2}");
            }
        }

        /// <summary>
        /// ボリューム数値テキスト更新
        /// </summary>
        private void UpdateVolumeText(float value)
        {
            if (volumeValueText != null)
            {
                volumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        /// <summary>
        /// タイトルへ戻る（確認後）
        /// </summary>
        public void ConfirmReturnToTitle()
        {
            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] タイトルへ戻る確定");
            }

            ReturnToTitle();
        }

        /// <summary>
        /// タイトルへ戻る処理
        /// </summary>
        private void ReturnToTitle()
        {
            // シーン遷移準備
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PrepareSceneTransition();
                GameManager.Instance.ResetGame();
            }

            // TitleSceneへ遷移
            if (Application.CanStreamedLevelBeLoaded("TitleScene"))
            {
                SceneManager.LoadScene("TitleScene");
            }
            else
            {
                Debug.LogWarning("[MenuWindow] TitleSceneが見つかりません。GameSceneをリロードします。");
                SceneManager.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// ゲーム終了（確認後）
        /// </summary>
        public void ConfirmQuitGame()
        {
            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] ゲーム終了確定");
            }

            QuitGame();
        }

        /// <summary>
        /// ゲーム終了処理
        /// </summary>
        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] ゲーム終了");
            }
        }

        /// <summary>
        /// 確認ダイアログをキャンセル
        /// </summary>
        public void CancelConfirmDialog()
        {
            if (titleConfirmDialog != null)
                titleConfirmDialog.SetActive(false);

            if (quitConfirmDialog != null)
                quitConfirmDialog.SetActive(false);

            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] 確認ダイアログキャンセル");
            }
        }

        /// <summary>
        /// 全パネルを閉じる
        /// </summary>
        private void CloseAllPanels()
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            if (volumePanel != null)
                volumePanel.SetActive(false);

            currentPanel = Panel.None;
        }

        /// <summary>
        /// 現在開いているパネルを閉じる
        /// </summary>
        private void CloseCurrentPanel()
        {
            CloseAllPanels();

            if (showDebugLog)
            {
                Debug.Log("[MenuWindow] パネル閉じる");
            }
        }
    }
}