using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FruitMixer.Core
{
    /// <summary>
    /// すべてのフルーツ情報を管理するデータベース
    /// </summary>
    [CreateAssetMenu(fileName = "MainFruitDatabase", menuName = "FruitMixer/FruitDatabase")]
    public class FruitDatabase : ScriptableObject
    {
        [Header("フルーツ情報リスト")]
        [Tooltip("10個のフルーツ情報をティア順に登録")]
        public List<FruitInfo> allFruits = new List<FruitInfo>();

        [Header("発射設定")]
        [Tooltip("発射可能な最大ティア（例: 5 = Blueberry〜Starfruit）")]
        [Range(1, 10)]
        public int maxSpawnTier = 5;

        /// <summary>
        /// ティアでFruitInfoを検索
        /// </summary>
        public FruitInfo GetFruitByTier(int tier)
        {
            return allFruits.FirstOrDefault(f => f != null && f.tier == tier);
        }

        /// <summary>
        /// FruitTypeでFruitInfoを検索
        /// </summary>
        public FruitInfo GetFruitByType(FruitType type)
        {
            return allFruits.FirstOrDefault(f => f != null && f.fruitType == type);
        }

        /// <summary>
        /// 下位ティアのフルーツをランダム生成（発射用）
        /// </summary>
        public FruitInfo GetRandomLowTierFruit()
        {
            int randomTier = Random.Range(1, maxSpawnTier + 1);
            return GetFruitByTier(randomTier);
        }

        /// <summary>
        /// 最終フルーツか確認
        /// </summary>
        public bool IsFinalFruit(int tier)
        {
            return tier >= 10; // Durian
        }

        void OnValidate()
        {
            // Inspectorで編集時に自動検証
            if (allFruits.Count != 10)
            {
                Debug.LogWarning($"[FruitDatabase] フルーツが{allFruits.Count}個です。10個必要です。");
            }
        }
    }
}