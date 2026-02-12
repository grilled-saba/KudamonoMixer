using UnityEngine;
using FruitMixer.Core;

/// <summary>
/// FruitDatabase動作テスト用の一時スクリプト
/// </summary>
public class TestFruitDatabase : MonoBehaviour
{
    [Header("テスト設定")]
    [Tooltip("MainFruitDatabaseをドラッグ")]
    public FruitDatabase database;

    void Start()  // ← 이 메서드 전체를 새 코드로 교체
    {
        if (database == null)
        {
            Debug.LogError("[TestFruitDatabase] Databaseが設定されていません！");
            return;
        }

        Debug.Log("=== FruitDatabase テスト開始 ===");

        // Tier 1~10 전부 확인
        Debug.Log("--- Tier 1〜10 検索テスト ---");
        for (int tier = 1; tier <= 10; tier++)  // ← 7을 10으로 변경
        {
            FruitInfo fruit = database.GetFruitByTier(tier);
            if (fruit != null && fruit.fruitType != FruitType.None)
            {
                Debug.Log($"✅ Tier {tier}: {fruit.fruitType} (Score: {fruit.mergeScore})");
            }
            else if (fruit != null && fruit.fruitType == FruitType.None)
            {
                Debug.LogError($"❌ Tier {tier}: Fruit Type が None です！設定してください");
            }
            else
            {
                Debug.LogError($"❌ Tier {tier}: フルーツが見つかりません");
            }
        }

        // 최종 과일 테스트 추가
        Debug.Log("--- 最終フルーツ判定テスト ---");
        Debug.Log($"Tier 9 (Coconut)は最終フルーツ？ {database.IsFinalFruit(9)}");   // False
        Debug.Log($"Tier 10 (Durian)は最終フルーツ？ {database.IsFinalFruit(10)}");  // True

        // ランダムテスト
        Debug.Log("--- ランダムフルーツ生成テスト（10回）---");
        for (int i = 0; i < 10; i++)
        {
            FruitInfo random = database.GetRandomLowTierFruit();
            if (random != null)
            {
                Debug.Log($"  {i + 1}回目: {random.fruitType} (Tier {random.tier})");
            }
        }
    }
}