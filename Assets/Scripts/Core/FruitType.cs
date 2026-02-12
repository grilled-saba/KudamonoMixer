using UnityEngine;

namespace FruitMixer.Core
{
    /// <summary>
    /// フルーツの種類（10段階）
    /// </summary>
    public enum FruitType
    {
        None = 0,           // 初期値（使用しない）
        Blueberry = 1,      // ブルーベリー
        Lychee = 2,         // ライチ
        Kiwi = 3,           // キウイ
        Lemon = 4,          // レモン
        Starfruit = 5,      // スターフルーツ
        Apple = 6,          // りんご
        Avocado = 7,        // アボカド
        Pineapple = 8,      // パイナップル
        Coconut = 9,        // ココナッツ
        Durian = 10         // ドリアン（最終）
    }
}