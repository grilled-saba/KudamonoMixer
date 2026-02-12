using FruitMixer.Gameplay;
using FruitMixer.Managers;
using UnityEngine;

namespace FruitMixer.Test
{
    /// <summary>
    /// FruitSpawner テスト用スクリプト
    /// Spaceキーで複数発射トリガー
    /// </summary>
    public class TestFruitSpawner : MonoBehaviour
    {
        [Header("参照")]
        [Tooltip("テスト対象のFruitSpawner")]
        public FruitSpawner fruitSpawner;

        [Header("テスト設定")]
        [Tooltip("一度に発射する個数 (2~6)")]
        [Range(2, 6)]
        public int launchCount = 2;

        [Tooltip("自動発射モード")]
        public bool autoLaunchMode = false;

        [Tooltip("自動発射間隔(秒)")]
        public float autoLaunchInterval = 2.0f;

        // 内部
        private int totalLaunchCalls = 0;
        private float autoTimer = 0f;

        void Update()
        {
            // ゲーム一時停止中は入力無効
            if (GameManager.Instance != null && GameManager.Instance.IsPaused())
            {
                return;
            }

            // 手動発射: Spaceキー
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LaunchFruits();
            }

            // 自動発射モード
            if (autoLaunchMode)
            {
                autoTimer += Time.deltaTime;
                if (autoTimer >= autoLaunchInterval)
                {
                    autoTimer = 0f;
                    LaunchFruits();
                }
            }
        }

        /// <summary>
        /// 発射実行
        /// </summary>
        public void LaunchFruits()
        {
            if (fruitSpawner == null)
            {
                Debug.LogError("[TestFruitSpawner] FruitSpawner が未設定");
                return;
            }

            totalLaunchCalls++;
            Debug.Log($"[TestFruitSpawner] === 発射 #{totalLaunchCalls} ({launchCount}個) ===");

            fruitSpawner.LaunchMultipleFruits(launchCount);
        }

        /// <summary>
        /// カウンターをリセット
        /// </summary>
        public void ResetCounter()
        {
            totalLaunchCalls = 0;
            autoTimer = 0f;
            Debug.Log("[TestFruitSpawner] カウンターリセット");
        }
    }
}