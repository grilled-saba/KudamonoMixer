using UnityEngine;
using FruitMixer.Core;

/// <summary>
/// FruitDataとFruitPrefab動作テスト
/// </summary>
public class TestFruitSpawn : MonoBehaviour
{
    [Header("テスト設定")]
    [Tooltip("FruitPrefabをドラッグ")]
    public GameObject fruitPrefab;

    [Tooltip("MainFruitDatabaseをドラッグ")]
    public FruitDatabase database;

    [Header("生成位置")]
    public Vector2 spawnPosition = new Vector2(0, 3);

    void Update()
    {
        // スペースキーでランダムフルーツ生成
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnRandomFruit();
        }

        // 1〜5キーで特定ティア生成
        for (int i = 1; i <= 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SpawnFruitByTier(i);
            }
        }
    }

    void SpawnRandomFruit()
    {
        if (fruitPrefab == null || database == null)
        {
            Debug.LogError("[TestFruitSpawn] Prefab または Database が未設定！");
            return;
        }

        // ランダム下位フルーツ取得
        FruitInfo randomInfo = database.GetRandomLowTierFruit();

        if (randomInfo == null)
        {
            Debug.LogError("[TestFruitSpawn] ランダムフルーツ取得失敗");
            return;
        }

        // 生成
        GameObject fruitObj = Instantiate(fruitPrefab, spawnPosition, Quaternion.identity);
        FruitData fruitData = fruitObj.GetComponent<FruitData>();

        // 初期化
        fruitData.Initialize(randomInfo);

        Debug.Log($"[TestFruitSpawn] 生成: {randomInfo.fruitType}");
    }

    void SpawnFruitByTier(int tier)
    {
        if (fruitPrefab == null || database == null)
        {
            Debug.LogError("[TestFruitSpawn] Prefab または Database が未設定！");
            return;
        }

        FruitInfo info = database.GetFruitByTier(tier);

        if (info == null)
        {
            Debug.LogError($"[TestFruitSpawn] Tier {tier} が見つかりません");
            return;
        }

        GameObject fruitObj = Instantiate(fruitPrefab, spawnPosition, Quaternion.identity);
        FruitData fruitData = fruitObj.GetComponent<FruitData>();
        fruitData.Initialize(info);

        Debug.Log($"[TestFruitSpawn] Tier {tier} 生成: {info.fruitType}");
    }
}