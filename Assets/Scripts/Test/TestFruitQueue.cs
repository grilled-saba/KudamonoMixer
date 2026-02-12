using UnityEngine;
using FruitMixer.Core;
using FruitMixer.Gameplay;

/// <summary>
/// FruitQueue動作テスト
/// </summary>
public class TestFruitQueue : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("FruitQueueコンポーネント")]
    public FruitQueue fruitQueue;

    [Header("テスト情報")]
    [SerializeField] private int totalLaunchedCount = 0;

    void Start()
    {
        if (fruitQueue == null)
        {
            Debug.LogError("[TestFruitQueue] FruitQueueが未設定！");
            return;
        }

        // イベント登録
        fruitQueue.OnQueueUpdated += OnQueueUpdated;
        fruitQueue.OnRefreshCountChanged += OnRefreshCountChanged;

        Debug.Log("=== FruitQueue テスト開始 ===");
        Debug.Log("Space: 次のフルーツ発射");
        Debug.Log("R: キュー再生成");
        Debug.Log("Q: 現在のキュー表示");
    }

    void Update()
    {
        // Space: 次のフルーツ取得
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchNextFruit();
        }

        // R: キュー再生成
        if (Input.GetKeyDown(KeyCode.R))
        {
            RefreshQueue();
        }

        // Q: キュー表示
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DisplayCurrentQueue();
        }
    }

    void LaunchNextFruit()
    {
        FruitInfo nextFruit = fruitQueue.GetNextFruit();

        if (nextFruit != null)
        {
            totalLaunchedCount++;
            Debug.Log($"[発射 #{totalLaunchedCount}] {nextFruit.fruitType} (Tier {nextFruit.tier})");
        }
        else
        {
            Debug.LogError("[TestFruitQueue] フルーツ取得失敗");
        }
    }

    void RefreshQueue()
    {
        bool success = fruitQueue.RefreshQueue();

        if (success)
        {
            Debug.Log("✅ キュー再生成成功");
            DisplayCurrentQueue();
        }
        else
        {
            Debug.LogWarning("❌ 再生成回数上限に達しました");
        }
    }

    void DisplayCurrentQueue()
    {
        var queue = fruitQueue.GetCurrentQueue();

        Debug.Log($"--- 現在のキュー ({queue.Count}個) ---");
        for (int i = 0; i < queue.Count; i++)
        {
            Debug.Log($"  [{i + 1}] {queue[i].fruitType} (Tier {queue[i].tier})");
        }
        Debug.Log($"残り再生成回数: {fruitQueue.GetRemainingRefreshCount()}/3");
    }

    void OnQueueUpdated()
    {
        Debug.Log("[イベント] キュー更新");
    }

    void OnRefreshCountChanged(int remaining)
    {
        Debug.Log($"[イベント] 再生成回数変更: {remaining}回");
    }

    void OnDestroy()
    {
        if (fruitQueue != null)
        {
            fruitQueue.OnQueueUpdated -= OnQueueUpdated;
            fruitQueue.OnRefreshCountChanged -= OnRefreshCountChanged;
        }
    }
}