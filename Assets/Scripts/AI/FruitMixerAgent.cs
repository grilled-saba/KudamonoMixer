using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using FruitMixer.Managers;
using FruitMixer.Gameplay;
using FruitMixer.Core;
using FruitMixer.Test;

namespace FruitMixer.AI
{
    /// <summary>
    /// FruitMixer AI エージェント
    /// 観察(Observation) / 行動(Action) / 報酬(Reward) を定義
    /// </summary>
    public class FruitMixerAgent : Agent
    {
        [Header("ゲーム参照")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MixerManager mixerManager;
        [SerializeField] private FruitQueue fruitQueue;
        [SerializeField] private TestFruitSpawner spawner;
        [SerializeField] private FruitSlicer slicer;
        [SerializeField] private Camera aiCamera;

        [Header("行動設定")]
        [Tooltip("行動間隔（秒）- APM制限")]
        [SerializeField] private float minActionInterval = 0.2f;

        [Header("観察設定")]
        [Tooltip("観察する最大フルーツ数")]
        [SerializeField] private int maxFruitObservations = 40;
        [Tooltip("観察する最大爆弾数")]
        [SerializeField] private int maxBombObservations = 5;
        [Tooltip("観察するキューの果物数")]
        [SerializeField] private int maxQueueObservations = 3;

        // 内部状態
        private float lastActionTime = 0f;
        private float episodeStartTime = 0f;

        // ==================== 初期化 ====================

        /// <summary>
        /// エピソード開始時に呼ばれる（ゲームリセット）
        /// </summary>
        public override void OnEpisodeBegin()
        {
            episodeStartTime = Time.time;
            lastActionTime = 0f;

            if (gameManager != null)
            {
                gameManager.ResetGame();
            }

            Debug.Log("[FruitMixerAgent] エピソード開始");
        }

        // ==================== 観察 ====================

        /// <summary>
        /// AIに観察情報を渡す
        /// Observation合計サイズ: 4 + 6 + (20×4) + (5×3) = 105
        /// </summary>
        public override void CollectObservations(VectorSensor sensor)
        {
            // ========== 基本ゲーム状態 (4個) ==========
            // ドリアン個数 (0~2 → 正規化)
            sensor.AddObservation(gameManager != null ? gameManager.GetDurianCount() / 2f : 0f);
            // 爆弾個数 (0~3 → 正規化)
            sensor.AddObservation(gameManager != null ? gameManager.GetDeleteCount() / 3f : 0f);
            // 経過時間 (0~1に正規化)
            sensor.AddObservation((Time.time - episodeStartTime) % 100f / 100f);
            // ミキサー内フルーツ個数 (0~20 → 正規化)
            int mixerCount = mixerManager != null ? mixerManager.GetFruitsInMixer().Count : 0;
            sensor.AddObservation(mixerCount / 20f);

            // ========== 発射キュー情報 (6個) ==========
            // 次に発射される果物のTier情報 (最大3個)
            List<FruitInfo> queue = fruitQueue != null ? fruitQueue.GetCurrentQueue() : null;
            for (int i = 0; i < maxQueueObservations; i++)
            {
                if (queue != null && i < queue.Count && queue[i] != null)
                    sensor.AddObservation(queue[i].tier / 10f); // Tier 1~10 → 0.1~1.0
                else
                    sensor.AddObservation(0f);
            }

            // ========== ミキサー内フルーツ情報 (最大20個 × 4 = 80個) ==========
            List<FruitData> fruitsInMixer = mixerManager != null
                ? mixerManager.GetFruitsInMixer()
                : new List<FruitData>();

            // X座標でソート（観察順序を安定化）
            fruitsInMixer.Sort((a, b) =>
                a.transform.position.x.CompareTo(b.transform.position.x));

            int observedFruits = 0;
            foreach (var fruit in fruitsInMixer)
            {
                if (observedFruits >= maxFruitObservations) break;
                if (fruit == null) continue;

                // X座標 (-10~10 → -1~1)
                sensor.AddObservation(fruit.transform.position.x / 10f);
                // Y座標 (-15~5 → -1~1)
                sensor.AddObservation(fruit.transform.position.y / 10f);
                // Tier (1~10 → 0.1~1.0)
                sensor.AddObservation(fruit.currentTier / 10f);
                // ミキサー内フラグ (常に1 - ミキサー内リストなので)
                sensor.AddObservation(1f);

                observedFruits++;
            }

            // 残りをゼロパディング
            for (int i = observedFruits; i < maxFruitObservations; i++)
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }

            // ========== 画面内爆弾情報 (最大5個 × 3 = 15個) ==========
            GameObject[] allBombs = GameObject.FindGameObjectsWithTag("Bomb");

            // X座標でソート（観察順序を安定化）
            System.Array.Sort(allBombs, (a, b) =>
                a.transform.position.x.CompareTo(b.transform.position.x));

            int observedBombs = 0;
            foreach (var bombObj in allBombs)
            {
                if (observedBombs >= maxBombObservations) break;
                if (bombObj == null) continue;
                if (!IsInCameraView(bombObj)) continue;

                sensor.AddObservation(bombObj.transform.position.x / 10f);
                sensor.AddObservation(bombObj.transform.position.y / 10f);
                sensor.AddObservation(1f); // 爆弾フラグ

                observedBombs++;
            }

            // 残りをゼロパディング
            for (int i = observedBombs; i < maxBombObservations; i++)
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        // ==================== 行動 ====================

        /// <summary>
        /// AIの行動を実行する
        /// Continuous Actions [0]: マウスX (-1~1)
        /// Continuous Actions [1]: マウスY (-1~1)
        /// Discrete Actions [0]: 発射ボタン (0=押さない, 1=押す)
        /// Discrete Actions [1]: マウスアクション (0=なし, 1=左クリック, 2=右クリック)
        /// </summary>
        public override void OnActionReceived(ActionBuffers actions)
        {
            // APM制限
            if (Time.time - lastActionTime < minActionInterval) return;

            // マウス座標変換 (Viewport → World)
            float mouseX = actions.ContinuousActions[0];
            float mouseY = actions.ContinuousActions[1];

            Vector3 viewportPoint = new Vector3(
                (mouseX + 1f) / 2f,
                (mouseY + 1f) / 2f,
                10f
            );

            Camera cam = aiCamera != null ? aiCamera : Camera.main;
            Vector3 worldPos = cam.ViewportToWorldPoint(viewportPoint);
            worldPos.z = 0f;

            // 発射ボタン
            if (actions.DiscreteActions[0] == 1 && spawner != null)
            {
                spawner.LaunchFruits();
                lastActionTime = Time.time;
            }

            // マウスアクション
            int mouseAction = actions.DiscreteActions[1];

            if (mouseAction == 1) // 左クリック（スライス）
            {
                if (IsInCameraView(worldPos) && slicer != null)
                {
                    slicer.SimulateDrag(worldPos);
                    lastActionTime = Time.time;
                }
            }
            else if (mouseAction == 2) // 右クリック（収集/移動）
            {
                if (IsInCameraView(worldPos))
                {
                    SimulateRightClick(worldPos);
                    lastActionTime = Time.time;
                }
            }

            // 時間ペナルティ（長時間かかると不利）
            AddReward(-0.0001f);
        }

        // ==================== 報酬 ====================

        /// <summary>
        /// フルーツ合成成功時の報酬
        /// </summary>
        public void RewardMerge(int tier)
        {
            AddReward(0.01f * tier);
            Debug.Log($"[FruitMixerAgent] 合成報酬: +{0.01f * tier} (Tier {tier})");
        }

        /// <summary>
        /// ドリアン生成時の報酬
        /// </summary>
        public void RewardDurian()
        {
            AddReward(1.0f);
            Debug.Log("[FruitMixerAgent] ドリアン報酬: +1.0");
        }

        /// <summary>
        /// 勝利時の報酬（エピソード終了）
        /// </summary>
        public void RewardWin()
        {
            AddReward(10.0f);
            Debug.Log("[FruitMixerAgent] 勝利報酬: +10.0");
            EndEpisode();
        }

        /// <summary>
        /// 爆弾回収時の報酬
        /// </summary>
        public void RewardBombCollected()
        {
            AddReward(0.05f);
            Debug.Log("[FruitMixerAgent] 爆弾回収報酬: +0.05");
        }

        /// <summary>
        /// 爆弾がミキサーに入った時のペナルティ
        /// </summary>
        public void PenaltyBombInMixer()
        {
            AddReward(-0.2f);
            Debug.Log("[FruitMixerAgent] 爆弾進入ペナルティ: -0.2");
        }

        /// <summary>
        /// 敗北時のペナルティ（エピソード終了）
        /// </summary>
        public void PenaltyLose()
        {
            AddReward(-5.0f);
            Debug.Log("[FruitMixerAgent] 敗北ペナルティ: -5.0");
            EndEpisode();
        }

        // ==================== ユーティリティ ====================

        /// <summary>
        /// オブジェクトがカメラ内にあるか確認
        /// </summary>
        private bool IsInCameraView(GameObject obj)
        {
            if (obj == null) return false;
            Camera cam = aiCamera != null ? aiCamera : Camera.main;
            return IsInCameraView(obj.transform.position, cam);
        }

        /// <summary>
        /// ワールド座標がカメラ内にあるか確認
        /// </summary>
        private bool IsInCameraView(Vector3 worldPos)
        {
            Camera cam = aiCamera != null ? aiCamera : Camera.main;
            return IsInCameraView(worldPos, cam);
        }

        private bool IsInCameraView(Vector3 worldPos, Camera cam)
        {
            if (cam == null) return false;
            Vector3 viewportPoint = cam.WorldToViewportPoint(worldPos);
            return viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                   viewportPoint.y >= 0f && viewportPoint.y <= 1f &&
                   viewportPoint.z > 0f;
        }

        /// <summary>
        /// 右クリックシミュレーション
        /// </summary>
        private void SimulateRightClick(Vector3 worldPos)
        {
            var rightClickHandler = FindAnyObjectByType<RightClickHandler>();
            if (rightClickHandler != null)
            {
                rightClickHandler.SimulateClick(worldPos);
            }
        }
    }
}