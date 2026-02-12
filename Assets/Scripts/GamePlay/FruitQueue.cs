using UnityEngine;
using System.Collections.Generic;
using FruitMixer.Core;

namespace FruitMixer.Gameplay
{
    /// <summary>
    /// 発射予定のフルーツキュー管理
    /// 6個のフルーツを表示し、再生成機能を提供
    /// </summary>
    public class FruitQueue : MonoBehaviour
    {
        [Header("参照")]
        [Tooltip("FruitDatabaseへの参照")]
        public FruitDatabase database;

        [Header("キュー設定")]
        [Tooltip("キューに表示するフルーツの数")]
        [SerializeField] private int queueSize = 6;

        [Tooltip("再生成可能回数")]
        [SerializeField] private int maxRefreshCount = 3;

        // 現在のキュー
        private List<FruitInfo> currentQueue = new List<FruitInfo>();

        // 残り再生成回数
        private int remainingRefreshCount;

        // イベント（UI更新用）
        public System.Action OnQueueUpdated;
        public System.Action<int> OnRefreshCountChanged;

        void Start()
        {
            remainingRefreshCount = maxRefreshCount;
            GenerateNewQueue();
        }

        /// <summary>
        /// 新しいキューを生成
        /// </summary>
        public void GenerateNewQueue()
        {
            currentQueue.Clear();

            for (int i = 0; i < queueSize; i++)
            {
                FruitInfo randomFruit = database.GetRandomLowTierFruit();

                if (randomFruit != null)
                {
                    currentQueue.Add(randomFruit);
                }
                else
                {
                    Debug.LogError("[FruitQueue] ランダムフルーツ取得失敗");
                }
            }

            OnQueueUpdated?.Invoke();

            Debug.Log($"[FruitQueue] 新しいキュー生成: {currentQueue.Count}個");
        }

        /// <summary>
        /// キュー再生成（回数制限あり）
        /// </summary>
        public bool RefreshQueue()
        {
            if (remainingRefreshCount <= 0)
            {
                Debug.LogWarning("[FruitQueue] 再生成回数上限");
                return false;
            }

            remainingRefreshCount--;
            GenerateNewQueue();

            OnRefreshCountChanged?.Invoke(remainingRefreshCount);

            Debug.Log($"[FruitQueue] キュー再生成 (残り{remainingRefreshCount}回)");
            return true;
        }

        /// <summary>
        /// 次に発射するフルーツを取得（キューから削除）
        /// </summary>
        public FruitInfo GetNextFruit()
        {
            if (currentQueue.Count == 0)
            {
                Debug.LogWarning("[FruitQueue] キューが空です");
                return null;
            }

            FruitInfo nextFruit = currentQueue[0];
            currentQueue.RemoveAt(0);

            // キューが空になったら新しいキュー生成
            if (currentQueue.Count == 0)
            {
                remainingRefreshCount = maxRefreshCount; // リセット
                GenerateNewQueue();
                OnRefreshCountChanged?.Invoke(remainingRefreshCount);
            }
            else
            {
                OnQueueUpdated?.Invoke();
            }

            Debug.Log($"[FruitQueue] 次のフルーツ: {nextFruit.fruitType} (残り{currentQueue.Count}個)");
            return nextFruit;
        }

        /// <summary>
        /// 現在のキューを取得（表示用）
        /// </summary>
        public List<FruitInfo> GetCurrentQueue()
        {
            return new List<FruitInfo>(currentQueue); // コピーを返す
        }

        /// <summary>
        /// 残り再生成回数を取得
        /// </summary>
        public int GetRemainingRefreshCount()
        {
            return remainingRefreshCount;
        }

        /// <summary>
        /// キューが空か確認
        /// </summary>
        public bool IsQueueEmpty()
        {
            return currentQueue.Count == 0;
        }

        /// <summary>
        /// キューサイズを変更して新しいキューを生成
        /// </summary>
        public void SetQueueSize(int newSize)
        {
            queueSize = Mathf.Clamp(newSize, 2, 5);
            Debug.Log($"[FruitQueue] キューサイズ設定: {queueSize}個 (再生成なし)");
        }

        /// <summary>
        /// キューサイズを取得
        /// </summary>
        public int GetQueueSize()
        {
            return queueSize;
        }

        /// <summary>
        /// キューサイズを変更して再生成（回数制限あり）
        /// </summary>
        public bool SetQueueSizeAndRefresh(int newSize)
        {
            if (remainingRefreshCount <= 0)
            {
                Debug.LogWarning("[FruitQueue] 再生成回数不足 - サイズ変更不可");
                return false;
            }

            queueSize = Mathf.Clamp(newSize, 2, 5);
            remainingRefreshCount--;
            GenerateNewQueue();

            OnRefreshCountChanged?.Invoke(remainingRefreshCount);

            Debug.Log($"[FruitQueue] サイズ変更+再生成: {queueSize}個 (残り{remainingRefreshCount}回)");
            return true;
        }

        /// <summary>
        /// キューを再生成して回数リセット（発射後の自動再生成用）
        /// </summary>
        public void GenerateNewQueueWithReset()
        {
            remainingRefreshCount = maxRefreshCount;
            GenerateNewQueue();
            OnRefreshCountChanged?.Invoke(remainingRefreshCount);

            Debug.Log($"[FruitQueue] キュー再生成+回数リセット (残り{remainingRefreshCount}回)");
        }
    }
}