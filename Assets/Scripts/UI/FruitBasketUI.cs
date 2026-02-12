using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FruitMixer.Gameplay;
using FruitMixer.Core;
using FruitMixer.Test;
using FruitMixer.Managers; // GameManager

namespace FruitMixer.UI
{
    /// <summary>
    /// フルーツバスケットUI管理
    /// - キュー表示
    /// - 再生成ボタン
    /// - 発射個数スライダー
    /// </summary>
    public class FruitBasketUI : MonoBehaviour
    {
        [Header("参照")]
        [Tooltip("FruitQueue")]
        [SerializeField] private FruitQueue fruitQueue;

        [Tooltip("TestFruitSpawner")]
        [SerializeField] public TestFruitSpawner testFruitSpawner;

        [Header("UI要素")]
        [Tooltip("フルーツアイコン配列 (5個)")]
        [SerializeField] private Image[] fruitIcons = new Image[5];

        [Tooltip("再生成回数テキスト")]
        [SerializeField] private TextMeshProUGUI refreshCountText;

        [Tooltip("発射個数スライダー")]
        [SerializeField] private Slider launchCountSlider;

        [Tooltip("再生成ボタン")]
        [SerializeField] private Button refreshButton;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        void OnEnable()
        {
            // 開いた時に更新
            UpdateFruitIcons();
            UpdateRefreshCountText();
            UpdateLaunchCountSlider();
        }

        void Start()
        {
            // スライダーイベント登録
            if (launchCountSlider != null)
            {
                launchCountSlider.onValueChanged.AddListener(OnLaunchCountChanged);
            }

            // 再生成ボタンイベント登録
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(OnRefreshButtonClicked);
            }

            // FruitQueueイベント登録
            if (fruitQueue != null)
            {
                fruitQueue.OnQueueUpdated += UpdateFruitIcons;
                fruitQueue.OnRefreshCountChanged += OnRefreshCountChanged;
            }
        }

        void OnDestroy()
        {
            // イベント解除
            if (launchCountSlider != null)
            {
                launchCountSlider.onValueChanged.RemoveListener(OnLaunchCountChanged);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(OnRefreshButtonClicked);
            }

            if (fruitQueue != null)
            {
                fruitQueue.OnQueueUpdated -= UpdateFruitIcons;
                fruitQueue.OnRefreshCountChanged -= OnRefreshCountChanged;
            }
        }

        void Update()
        {
            // ESCキーで閉じる
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseWindow();
            }
        }

        /// <summary>
        /// フルーツアイコンを更新
        /// </summary>
        private void UpdateFruitIcons()
        {
            if (fruitQueue == null)
            {
                Debug.LogWarning("[FruitBasketUI] FruitQueue が未設定");
                return;
            }

            // 現在のキューを取得
            var currentQueue = fruitQueue.GetCurrentQueue();

            // キューサイズ取得
            int queueSize = currentQueue.Count;

            if (showDebugLog)
            {
                Debug.Log($"[FruitBasketUI] キュー更新: {queueSize}個");
            }

            // 各アイコンを更新
            for (int i = 0; i < fruitIcons.Length; i++)
            {
                if (fruitIcons[i] == null) continue;

                // キューサイズより小さいインデックス → 表示
                if (i < queueSize && i < currentQueue.Count)
                {
                    FruitInfo fruitInfo = currentQueue[i];

                    // スプライト設定
                    if (fruitInfo != null && fruitInfo.wholeFruitSprite != null)
                    {
                        fruitIcons[i].sprite = fruitInfo.wholeFruitSprite;
                        fruitIcons[i].color = Color.white; // 色をリセット

                        // ✨ スプライトの実際のサイズに基づいて比率調整
                        Rect spriteRect = fruitInfo.wholeFruitSprite.rect;
                        float spriteSize = Mathf.Max(spriteRect.width, spriteRect.height);

                        // 基準サイズ (80x80) に対する比率
                        float baseSize = 80f;
                        float scale = spriteSize / 100f; // 100ピクセルを基準とした比率

                        // RectTransformのサイズを調整
                        fruitIcons[i].rectTransform.sizeDelta = new Vector2(
                            baseSize * scale,
                            baseSize * scale
                        );

                        fruitIcons[i].gameObject.SetActive(true); // 表示
                    }
                    else
                    {
                        Debug.LogWarning($"[FruitBasketUI] FruitInfo[{i}] の wholeFruitSprite が null");
                        fruitIcons[i].gameObject.SetActive(false); // 非表示
                    }
                }
                else
                {
                    // キューサイズを超えるアイコン → 非表示
                    fruitIcons[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 再生成回数テキスト更新
        /// </summary>
        private void UpdateRefreshCountText()
        {
            if (fruitQueue == null || refreshCountText == null) return;

            int remaining = fruitQueue.GetRemainingRefreshCount();
            refreshCountText.text = $"{remaining}/3";

            if (showDebugLog)
            {
                Debug.Log($"[FruitBasketUI] 再生成回数更新: {remaining}/3");
            }
        }

        /// <summary>
        /// 発射個数スライダー初期化
        /// </summary>
        private void UpdateLaunchCountSlider()
        {
            if (launchCountSlider == null || testFruitSpawner == null) return;

            // TestFruitSpawnerの現在値を反映
            launchCountSlider.value = testFruitSpawner.launchCount;

            if (showDebugLog)
            {
                Debug.Log($"[FruitBasketUI] スライダー初期化: {testFruitSpawner.launchCount}");
            }
        }

        /// <summary>
        /// 発射個数スライダー変更時
        /// </summary>
        private void OnLaunchCountChanged(float value)
        {
            int count = Mathf.RoundToInt(value);

            if (fruitQueue == null || testFruitSpawner == null)
            {
                return;
            }

            // 現在値と同じなら何もしない
            if (count == testFruitSpawner.launchCount)
            {
                return;
            }

            // 再生成回数をチェック
            if (fruitQueue.GetRemainingRefreshCount() <= 0)
            {
                Debug.LogWarning("[FruitBasketUI] 再生成回数不足 - スライダー変更不可");

                // スライダーを前の値に戻す（イベントを一時解除）
                launchCountSlider.onValueChanged.RemoveListener(OnLaunchCountChanged);
                launchCountSlider.value = testFruitSpawner.launchCount;
                launchCountSlider.onValueChanged.AddListener(OnLaunchCountChanged);

                return;
            }

            // TestFruitSpawner更新
            testFruitSpawner.launchCount = count;

            // ✨ キューサイズ変更 + 再生成 + 回数減少
            bool success = fruitQueue.SetQueueSizeAndRefresh(count);

            if (!success)
            {
                Debug.LogWarning("[FruitBasketUI] キューサイズ変更失敗");

                // 失敗したら値を戻す
                launchCountSlider.onValueChanged.RemoveListener(OnLaunchCountChanged);
                launchCountSlider.value = testFruitSpawner.launchCount;
                launchCountSlider.onValueChanged.AddListener(OnLaunchCountChanged);
            }
            else
            {
                if (showDebugLog)
                {
                    Debug.Log($"[FruitBasketUI] 発射個数変更: {count}");
                }
            }
        }

        /// <summary>
        /// 再生成ボタンクリック時
        /// </summary>
        private void OnRefreshButtonClicked()
        {
            if (fruitQueue == null) return;

            bool success = fruitQueue.RefreshQueue();

            if (!success)
            {
                Debug.LogWarning("[FruitBasketUI] 再生成回数上限");
            }
            else
            {
                if (showDebugLog)
                {
                    Debug.Log("[FruitBasketUI] キュー再生成実行");
                }
            }
        }

        /// <summary>
        /// 再生成回数変更時（イベント）
        /// </summary>
        private void OnRefreshCountChanged(int remaining)
        {
            UpdateRefreshCountText();
        }

        /// <summary>
        /// ウィンドウを開く（外部から呼び出し）
        /// </summary>
        public void OpenWindow()
        {
            gameObject.SetActive(true);

            // ✨ ゲームを一時停止
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }

            if (showDebugLog)
            {
                Debug.Log("[FruitBasketUI] ウィンドウ開く");
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
                Debug.Log("[FruitBasketUI] ウィンドウ閉じる");
            }
        }
    }
}