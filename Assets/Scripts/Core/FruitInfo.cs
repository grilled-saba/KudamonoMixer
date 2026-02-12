using UnityEngine;

namespace FruitMixer.Core
{
    /// <summary>
    /// 各フルーツの情報を保存するScriptableObject
    /// Inspectorで設定可能
    /// </summary>
    [CreateAssetMenu(fileName = "FruitInfo", menuName = "FruitMixer/FruitInfo")]
    public class FruitInfo : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("フルーツの種類")]
        public FruitType fruitType = FruitType.None;

        [Tooltip("ティア（1〜10）")]
        [Range(1, 10)]
        public int tier = 1;

        [Header("ビジュアル")]
        [Tooltip("切る前の完全な果物")]
        public Sprite wholeFruitSprite;

        [Tooltip("切った後の半分（片方）※種がある側など")]
        public Sprite slicedHalfSprite_A;

        [Tooltip("切った後の半分（もう片方）※左右対称ならAと同じでOK")]
        public Sprite slicedHalfSprite_B;

        [Header("物理設定")]
        //[Tooltip("フルーツのサイズ（半径）")]
        //[Range(0.1f, 2f)]
        //public float size = 0.5f;

        [Tooltip("フルーツの質量")]
        [Range(0.1f, 5f)]
        public float mass = 1f;

        [Header("ゲームバランス")]
        [Tooltip("合成時の獲得スコア（基本: tier² × 10）")]
        public int mergeScore = 10;
    }
}