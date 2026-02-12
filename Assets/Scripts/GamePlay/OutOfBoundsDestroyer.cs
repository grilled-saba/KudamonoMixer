using UnityEngine;

namespace FruitMixer.Gameplay
{
    /// <summary>
    /// 画面外に落下したオブジェクトを自動削除（Scene Manager方式）
    /// Sceneの空のGameObjectに1つだけアタッチ
    /// フルーツ/爆弾レイヤーのオブジェクトを監視して削除
    /// </summary>
    public class OutOfBoundsDestroyer : MonoBehaviour
    {
        [Header("削除設定")]
        [Tooltip("この Y座標以下で削除 (デフォルト: -20)")]
        [SerializeField] private float destroyThresholdY = -20f;

        [Tooltip("監視対象レイヤー（Fruit, Bomb）")]
        [SerializeField] private LayerMask targetLayers = -1;

        [Header("デバッグ")]
        [Tooltip("削除時にログ表示")]
        [SerializeField] private bool showDebugLog = false;

        void Update()
        {
            CheckAndDestroyOutOfBoundsObjects();
        }

        /// <summary>
        /// 画面外のオブジェクトをチェックして削除
        /// </summary>
        private void CheckAndDestroyOutOfBoundsObjects()
        {
            // Fruit レイヤーのオブジェクトをすべて取得
            GameObject[] fruits = GetAllObjectsInLayer(6); // Layer 6 = Fruit
            foreach (GameObject fruit in fruits)
            {
                if (fruit != null && fruit.transform.position.y < destroyThresholdY)
                {
                    if (showDebugLog)
                    {
                        Debug.Log($"[OutOfBoundsDestroyer] 🗑️ {fruit.name} を削除 (Y: {fruit.transform.position.y:F2} < {destroyThresholdY})");
                    }
                    Destroy(fruit);
                }
            }

            // Bomb レイヤーのオブジェクトをすべて取得
            GameObject[] bombs = GetAllObjectsInLayer(7); // Layer 7 = Bomb
            foreach (GameObject bomb in bombs)
            {
                if (bomb != null && bomb.transform.position.y < destroyThresholdY)
                {
                    if (showDebugLog)
                    {
                        Debug.Log($"[OutOfBoundsDestroyer] 💣 {bomb.name} を削除 (Y: {bomb.transform.position.y:F2} < {destroyThresholdY})");
                    }
                    Destroy(bomb);
                }
            }
        }

        /// <summary>
        /// 指定レイヤーのすべてのGameObjectを取得
        /// </summary>
        private GameObject[] GetAllObjectsInLayer(int layer)
        {
            // Scene内のすべてのGameObjectを取得（非効率だが確実）
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            // 指定レイヤーのオブジェクトのみをフィルタリング
            System.Collections.Generic.List<GameObject> objectsInLayer = new System.Collections.Generic.List<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == layer)
                {
                    objectsInLayer.Add(obj);
                }
            }

            return objectsInLayer.ToArray();
        }

#if UNITY_EDITOR
        // エディタでしきい値を視覚化
        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            float screenWidth = 40f; // 適当な幅
            Vector3 lineStart = new Vector3(-screenWidth, destroyThresholdY, 0f);
            Vector3 lineEnd = new Vector3(screenWidth, destroyThresholdY, 0f);
            Gizmos.DrawLine(lineStart, lineEnd);
        }
#endif
    }
}