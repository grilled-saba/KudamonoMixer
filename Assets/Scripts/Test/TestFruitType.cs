using UnityEngine;
using FruitMixer.Core;

/// <summary>
/// FruitType動作テスト用の一時スクリプト
/// </summary>
public class TestFruitType : MonoBehaviour
{
    void Start()
    {
        // FruitType enum が使えるかテスト
        FruitType myFruit = FruitType.Apple;
        Debug.Log($"選択したフルーツ: {myFruit}"); // りんご と表示

        // int変換テスト
        int tier = (int)myFruit;
        Debug.Log($"ティア: {tier}"); // 6 と表示

        // すべてのフルーツをループ
        Debug.Log("=== 全フルーツリスト ===");
        for (int i = 1; i <= 10; i++)
        {
            FruitType fruit = (FruitType)i;
            Debug.Log($"Tier {i}: {fruit}");
        }
    }
}