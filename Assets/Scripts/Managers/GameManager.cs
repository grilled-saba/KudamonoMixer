using UnityEngine;
using FruitMixer.AI;

namespace FruitMixer.Managers
{
    /// <summary>
    /// ゲーム全体を管理するシングルトン
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

            // ==================== ゲームモード ====================
        public enum GameMode { SinglePlay, AIBattle }
        [SerializeField] private GameMode gameMode = GameMode.SinglePlay;

        [Header("ゲーム状態")]
        private bool isGameOver = false;
        private bool isPaused = false; // ✨ 一時停止状態
        private bool isTransitioning = false; // ✨ シーン遷移中フラグ

        [Header("削除カウント管理")]
        [Tooltip("ミキサー内削除可能回数（最大値）")]
        [SerializeField] private int maxDeleteCount = 3;

        private int currentDeleteCount = 0;

        [Header("スコア管理")]
        private int currentScore = 0;

        [Header("ゲーム統計")]
        private int durianCount = 0; // 生成されたドリアン個数

        [Header("ゲームオーバー関連")]
        private Sprite lastEscapedFruitSprite; // 脱出したフルーツのスプライト

        [Header("UI参照")]
        [Tooltip("ゲームクリアウィンドウ")]
        [SerializeField] private GameObject gameClearWindow;

        void Awake()
        {
            // シングルトンパターン
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // シーンロード時のイベント登録
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            // Unityエディタで停止ボタンを押した時の処理
            if (Instance == this)
            {
                Instance = null;

                // イベント解除
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        /// <summary>
        /// シーンロード完了時に呼ばれる
        /// </summary>
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // ✨ シーン遷移完了 - フラグをリセット
            isTransitioning = false;

            // GameSceneがロードされた時のみ処理
            if (scene.name == "GameScene")
            {
                Debug.Log("[GameManager] GameSceneロード完了 - 参照を再接続");
                ReconnectSceneReferences();
                ResetTestFruitSpawnerCounter();
            }
        }

        /// <summary>
              /// ゲームオーバー処理
        /// </summary>
        public void GameOver(Sprite escapedFruitSprite = null)
        {
            if (isGameOver) return;  // 重複防止
                                     // ✨ シーン遷移中なら無視
            if (isTransitioning)
            {
                Debug.Log("[GameManager] シーン遷移中のためGameOver無視");
                return;
            }
            isGameOver = true;
            FruitMixerAgent agent = FindFirstObjectByType<FruitMixerAgent>();
            if (agent != null)
            {
                agent.PenaltyLose();
            }
            // ✨ AI学習中はシーン遷移しない
            if (gameMode == GameMode.AIBattle)
            {
                Debug.Log("[GameManager] 💀 AI対戦モード: GameOver（シーン遷移なし）");
                return;
            }
            lastEscapedFruitSprite = escapedFruitSprite; // 脱出フルーツ保存
            Time.timeScale = 0f;  // ゲーム一時停止
            Debug.LogError("💀💀💀 GAME OVER! フルーツがミキサーから脱出しました！");
            // GameOverSceneへ遷移
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
        }

        /// <summary>
        /// 脱出したフルーツのスプライトを取得
        /// </summary>
        public Sprite GetEscapedFruitSprite()
        {
            return lastEscapedFruitSprite;
        }

        /// <summary>
        /// ゲームオーバー状態を取得
        /// </summary>
        public bool IsGameOver()
        {
            return isGameOver;
        }

        /// <summary>
        /// ゲームリスタート（デバッグ用）
        /// </summary>
        public void RestartGame()
        {
            isGameOver = false;
            Time.timeScale = 1f;
            Debug.Log("[GameManager] ゲームリスタート");
            // TODO: シーンリロード
        }

        /// <summary>
        /// ゲーム状態をリセット
        /// </summary>
        public void ResetGame()
        {
            isGameOver = false;
            isPaused = false;
            // isTransitioningはリセットしない（シーン遷移完了後に自動リセット）
            currentDeleteCount = 0;
            currentScore = 0;
            durianCount = 0;
            lastEscapedFruitSprite = null;
            Time.timeScale = 1f;

            Debug.Log("[GameManager] ゲーム状態リセット完了");
        }

        /// <summary>
        /// シーン遷移準備（GameOverトリガー防止）
        /// </summary>
        public void PrepareSceneTransition()
        {
            isTransitioning = true;
            Debug.Log("[GameManager] シーン遷移準備完了");
        }

        /// <summary>
        /// シーン遷移中かチェック
        /// </summary>
        public bool IsTransitioning()
        {
            return isTransitioning;
        }

        /// <summary>
        /// シーン参照を再接続
        /// </summary>
        private void ReconnectSceneReferences()
        {
            // TestFruitSpawner → FruitSpawner
            Test.TestFruitSpawner testSpawner = GetComponent<Test.TestFruitSpawner>();
            if (testSpawner != null)
            {
                // 同じGameObjectのFruitSpawnerを取得
                Gameplay.FruitSpawner spawner = GetComponent<Gameplay.FruitSpawner>();
                if (spawner != null)
                {
                    testSpawner.fruitSpawner = spawner;
                    Debug.Log("[GameManager] TestFruitSpawner.fruitSpawner を再接続");

                    // FruitSpawner.fruitQueue 再接続
                    Gameplay.FruitQueue queue = FindAnyObjectByType<Gameplay.FruitQueue>();
                    if (queue != null)
                    {
                        spawner.fruitQueue = queue;
                        Debug.Log("[GameManager] FruitSpawner.fruitQueue を再接続");
                    }

                    // FruitSpawner.leftSpawnPoint / rightSpawnPoint 再接続
                    Transform leftPoint = GameObject.Find("LeftSpawnPoint")?.transform;
                    Transform rightPoint = GameObject.Find("RightSpawnPoint")?.transform;

                    if (leftPoint != null)
                    {
                        spawner.leftSpawnPoint = leftPoint;
                        Debug.Log("[GameManager] FruitSpawner.leftSpawnPoint を再接続");
                    }

                    if (rightPoint != null)
                    {
                        spawner.rightSpawnPoint = rightPoint;
                        Debug.Log("[GameManager] FruitSpawner.rightSpawnPoint を再接続");
                    }
                }
            }

            // FruitSlicer.sliceTrail 再接続
            Gameplay.FruitSlicer slicer = GetComponent<Gameplay.FruitSlicer>();
            if (slicer != null)
            {
                TrailRenderer trail = GameObject.Find("SliceTrail")?.GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    slicer.sliceTrail = trail;
                    Debug.Log("[GameManager] FruitSlicer.sliceTrail を再接続");
                }
            }

            // GameManager.gameClearWindow 再接続
            // Canvas/Popups/GameClearWindow を検索（非アクティブでも検索）
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform popups = canvas.transform.Find("Popups");
                if (popups != null)
                {
                    Transform clearWindow = popups.Find("GameClearWindow");
                    if (clearWindow != null)
                    {
                        gameClearWindow = clearWindow.gameObject;
                        Debug.Log("[GameManager] GameManager.gameClearWindow を再接続");
                    }
                }
            }

            // ✨ FruitBasketUI.testFruitSpawner 再接続（非アクティブでも検索）
            UI.FruitBasketUI basketUI = FindAnyObjectByType<UI.FruitBasketUI>(FindObjectsInactive.Include);
            if (basketUI != null && testSpawner != null)
            {
                basketUI.testFruitSpawner = testSpawner;
                Debug.Log("[GameManager] FruitBasketUI.testFruitSpawner を再接続");
            }

            // ✨ LaunchButton.onClick 再接続
            // Canvas/BottomButtons/LaunchButton を検索
            if (canvas != null)
            {
                Transform bottomButtons = canvas.transform.Find("BottomButtons");
                if (bottomButtons != null)
                {
                    Transform launchButtonTransform = bottomButtons.Find("LaunchButton");
                    if (launchButtonTransform != null && testSpawner != null)
                    {
                        UnityEngine.UI.Button button = launchButtonTransform.GetComponent<UnityEngine.UI.Button>();
                        if (button != null)
                        {
                            // 既存のリスナーをクリア
                            button.onClick.RemoveAllListeners();
                            // 新しいリスナーを追加
                            button.onClick.AddListener(() => testSpawner.LaunchFruits());
                            Debug.Log("[GameManager] LaunchButton.onClick を再接続");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// TestFruitSpawnerのカウンターをリセット
        /// </summary>
        private void ResetTestFruitSpawnerCounter()
        {
            Test.TestFruitSpawner testSpawner = GetComponent<Test.TestFruitSpawner>();
            if (testSpawner != null)
            {
                testSpawner.ResetCounter();
            }
        }

        // ==================== ドリアン管理 ====================
        /// <summary>
        /// ドリアン生成時に呼ばれる
        /// </summary>
        public void OnDurianCreated()
        {
            durianCount++;
            Debug.Log($"[GameManager] 🏆 ドリアン生成! 現在: {durianCount}個");

            if (gameMode == GameMode.AIBattle && durianCount >= 2)
            {
                // AI対戦モード: ドリアン2個で勝利
                GameWin();
            }
            else if (gameMode == GameMode.SinglePlay && durianCount == 1)
            {
                // シングルプレイモード: 初ドリアンでクリアウィンドウ表示
                ShowClearWindow();
            }
        }
        /// <summary>
        /// 勝利処理（AI対戦モード用）
        /// </summary>
        public void GameWin()
        {
            Debug.Log("[GameManager] 🏆🏆🏆 GameWin呼び出し！");
            // TODO: FruitMixerAgentと連携予定

            FruitMixerAgent agent = FindFirstObjectByType<FruitMixerAgent>();
            if (agent != null)
            {
                agent.RewardWin();
            }
        }

        /// <summary>
        /// ゲームクリアウィンドウを表示
        /// </summary>
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

        /// <summary>
        /// 現在のドリアン個数を取得
        /// </summary>
        public int GetDurianCount()
        {
            return durianCount;
        }

        // ==================== 削除カウント管理 ====================

        /// <summary>
        /// 爆弾回収時に削除カウントを追加（最大3）
        /// </summary>
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

        /// <summary>
        /// ミキサー内削除時にカウントを消費
        /// </summary>
        /// <returns>削除可能ならtrue</returns>
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

        /// <summary>
        /// 現在の削除カウントを取得（UI表示用）
        /// </summary>
        public int GetDeleteCount()
        {
            return currentDeleteCount;
        }

        /// <summary>
        /// 最大削除カウントを取得（UI表示用）
        /// </summary>
        public int GetMaxDeleteCount()
        {
            return maxDeleteCount;
        }

        // ==================== スコア管理 ====================

        /// <summary>
        /// スコアを追加
        /// </summary>
        public void AddScore(int score)
        {
            currentScore += score;
            Debug.Log($"[GameManager] 💰 スコア +{score} → 合計: {currentScore}");
        }

        /// <summary>
        /// 現在のスコアを取得（UI表示用）
        /// </summary>
        public int GetScore()
        {
            return currentScore;
        }

        /// <summary>
        /// スコアをリセット
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            Debug.Log("[GameManager] スコアリセット");
        }

        // ========================================
        // 一時停止機能
        // ========================================

        /// <summary>
        /// ゲームを一時停止
        /// </summary>
        public void PauseGame()
        {
            if (isPaused) return; // 既に停止中なら何もしない

            isPaused = true;
            Time.timeScale = 0f;
            Debug.Log("[GameManager] ゲーム一時停止");
        }

        /// <summary>
        /// ゲームを再開
        /// </summary>
        public void ResumeGame()
        {
            if (!isPaused) return; // 停止中でなければ何もしない

            isPaused = false;
            Time.timeScale = 1f;
            Debug.Log("[GameManager] ゲーム再開");
        }

        /// <summary>
        /// 一時停止中かチェック
        /// </summary>
        public bool IsPaused()
        {
            return isPaused;
        }
    }
}