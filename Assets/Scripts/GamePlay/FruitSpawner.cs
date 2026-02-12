using UnityEngine;
using FruitMixer.Core;

namespace FruitMixer.Gameplay
{
    /// <summary>
    /// フルーツ発射システム
    /// ボタントリガー + FruitQueue連動
    /// </summary>
    public class FruitSpawner : MonoBehaviour
    {
        [Header("参照")]
        [Tooltip("FruitQueue")]
        public FruitQueue fruitQueue;

        [Tooltip("フルーツPrefab")]
        public GameObject fruitPrefab;

        [Header("発射位置")]
        [Tooltip("左側発射位置")]
        public Transform leftSpawnPoint;

        [Tooltip("右側発射位置")]
        public Transform rightSpawnPoint;

        [Header("発射設定")]
        [Tooltip("発射力")]
        [SerializeField] private float launchForce = 15f;

        [Tooltip("発射角度（度）")]
        [Range(60f, 80f)]
        [SerializeField] private float launchAngle = 70f;

        [Tooltip("角度のランダム幅")]
        [Range(0f, 15f)]
        [SerializeField] private float angleVariation = 5f;

        [Header("発射位置ランダム化")]
        [Tooltip("Y軸のランダム範囲（重複防止）")]
        [SerializeField] private float spawnYRandomRange = 1.0f;

        [Tooltip("X軸のランダム範囲（より自然に）")]
        [SerializeField] private float spawnXRandomRange = 0.3f;

        [Header("爆弾設定")]
        [Tooltip("爆弾Prefab")]
        public GameObject bombPrefab;

        [Tooltip("爆弾確率 (0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float bombProbability = 0.15f;

        // 次の発射は左右どちら？
        private bool isLeftTurn = true;

        /// <summary>
        /// 発射ボタンから呼び出し
        /// </summary>
        public void LaunchNextFruit()
        {
            if (fruitQueue == null)
            {
                Debug.LogError("[FruitSpawner] FruitQueue が未設定");
                return;
            }

            if (fruitPrefab == null)
            {
                Debug.LogError("[FruitSpawner] FruitPrefab が未設定");
                return;
            }

            // キューから次のフルーツ取得
            FruitInfo nextFruit = fruitQueue.GetNextFruit();

            if (nextFruit == null)
            {
                Debug.LogWarning("[FruitSpawner] キューが空です");
                return;
            }

            // 発射
            SpawnFruit(nextFruit);
        }

        /// <summary>
        /// フルーツを生成して発射
        /// </summary>
        private void SpawnFruit(FruitInfo fruitInfo)
        {
            // 左右交代で発射位置 決定
            Transform spawnPoint = isLeftTurn ? leftSpawnPoint : rightSpawnPoint;
            isLeftTurn = !isLeftTurn; // 次回は反対側

            // 15% 確率で爆弾に変更
            bool isBomb = Random.value < bombProbability;

            if (isBomb && bombPrefab != null)
            {
                SpawnBomb(spawnPoint);
                return;
            }

            // ランダム位置オフセット計算
            Vector3 randomOffset = CalculateRandomOffset();
            Vector3 finalPosition = spawnPoint.position + randomOffset;

            // フルーツ生成
            GameObject fruitObj = Instantiate(fruitPrefab, finalPosition, Quaternion.identity);
            FruitData fruitData = fruitObj.GetComponent<FruitData>();

            // 初期化
            fruitData.Initialize(fruitInfo);

            // 発射方向 計算
            Vector2 launchDirection = CalculateLaunchDirection(spawnPoint);

            // 力を加える
            Rigidbody2D rb = fruitObj.GetComponent<Rigidbody2D>();
            rb.linearVelocity = launchDirection * launchForce;

            // 回転を少し追加（見た目）
            rb.angularVelocity = Random.Range(-180f, 180f);

            Debug.Log($"[FruitSpawner] 発射: {fruitInfo.fruitType} from {(isLeftTurn ? "Right" : "Left")} at offset Y:{randomOffset.y:F2}");
        }

        /// <summary>
        /// 純粋なフルーツのみを生成して発射（爆弾チェックなし）
        /// </summary>
        private void SpawnPureFruit(FruitInfo fruitInfo)
        {
            // 左右交代で発射位置 決定
            Transform spawnPoint = isLeftTurn ? leftSpawnPoint : rightSpawnPoint;
            isLeftTurn = !isLeftTurn;

            // ランダム位置オフセット計算
            Vector3 randomOffset = CalculateRandomOffset();
            Vector3 finalPosition = spawnPoint.position + randomOffset;

            // フルーツ生成
            GameObject fruitObj = Instantiate(fruitPrefab, finalPosition, Quaternion.identity);
            FruitData fruitData = fruitObj.GetComponent<FruitData>();

            // 初期化
            fruitData.Initialize(fruitInfo);

            // 発射方向 計算
            Vector2 launchDirection = CalculateLaunchDirection(spawnPoint);

            // 力を加える
            Rigidbody2D rb = fruitObj.GetComponent<Rigidbody2D>();
            rb.linearVelocity = launchDirection * launchForce;

            // 回転を少し追加（見た目）
            rb.angularVelocity = Random.Range(-180f, 180f);

            Debug.Log($"[FruitSpawner] 発射: {fruitInfo.fruitType} from {(isLeftTurn ? "Right" : "Left")} at offset Y:{randomOffset.y:F2}");
        }

        /// <summary>
        /// 爆弾を生成して発射
        /// </summary>
        private void SpawnBomb(Transform spawnPoint)
        {
            // ランダム位置オフセット計算
            Vector3 randomOffset = CalculateRandomOffset();
            Vector3 finalPosition = spawnPoint.position + randomOffset;

            // 爆弾生成
            GameObject bombObj = Instantiate(bombPrefab, finalPosition, Quaternion.identity);

            // 発射方向 計算
            Vector2 launchDirection = CalculateLaunchDirection(spawnPoint);

            // 力を加える
            Rigidbody2D rb = bombObj.GetComponent<Rigidbody2D>();
            rb.linearVelocity = launchDirection * launchForce;
            rb.angularVelocity = Random.Range(-180f, 180f);

            Debug.Log($"[FruitSpawner] 💣 爆弾発射! from {(isLeftTurn ? "Right" : "Left")} at offset Y:{randomOffset.y:F2}");
        }

        /// <summary>
        /// ランダム位置オフセット計算（重複防止）
        /// </summary>
        private Vector3 CalculateRandomOffset()
        {
            float randomX = Random.Range(-spawnXRandomRange, spawnXRandomRange);
            float randomY = Random.Range(-spawnYRandomRange, spawnYRandomRange);

            return new Vector3(randomX, randomY, 0f);
        }

        /// <summary>
        /// 発射方向を計算（対角線上向き）
        /// </summary>
        private Vector2 CalculateLaunchDirection(Transform spawnPoint)
        {
            // ランダム角度variation
            float randomAngle = launchAngle + Random.Range(-angleVariation, angleVariation);

            // 左側なら右上、右側なら左上
            float direction = (spawnPoint == leftSpawnPoint) ? 1f : -1f;

            // ラジアンに変換
            float angleRad = randomAngle * Mathf.Deg2Rad;

            // 方向ベクトル
            Vector2 launchDir = new Vector2(
                direction * Mathf.Cos(angleRad),
                Mathf.Sin(angleRad)
            );

            return launchDir.normalized;
        }

        /// <summary>
        /// 複数のフルーツを連続発射
        /// </summary>
        /// <param name="count">発射個数 (2~5)</param>
        public void LaunchMultipleFruits(int count)
        {
            // 個数制限 (2~5)
            count = Mathf.Clamp(count, 2, 5);

            if (fruitQueue == null)
            {
                Debug.LogError("[FruitSpawner] FruitQueue が未設定");
                return;
            }

            // 実際に発射可能な個数を計算（キューサイズと比較）
            int actualCount = Mathf.Min(count, fruitQueue.GetQueueSize());

            Debug.Log($"[FruitSpawner] {actualCount}個のフルーツを発射開始");

            // 指定個数のフルーツを発射（爆弾なし）
            for (int i = 0; i < actualCount; i++)
            {
                // キューから次のフルーツ取得
                FruitInfo nextFruit = fruitQueue.GetNextFruit();

                if (nextFruit == null)
                {
                    Debug.LogWarning("[FruitSpawner] キューが空です");
                    break;
                }

                // 純粋なフルーツのみ発射（爆弾チェックなし）
                SpawnPureFruit(nextFruit);
            }

            Debug.Log($"[FruitSpawner] {actualCount}個の発射完了");

            // ✨ 発射後、キューが空なら再生成+回数リセット
            if (fruitQueue.IsQueueEmpty())
            {
                fruitQueue.GenerateNewQueueWithReset();
                Debug.Log($"[FruitSpawner] キューが空だったので再生成+回数リセット");
            }

            // 追加で爆弾を発射するか確率判定（15%）
            if (Random.value < bombProbability && bombPrefab != null)
            {
                Transform spawnPoint = isLeftTurn ? leftSpawnPoint : rightSpawnPoint;
                isLeftTurn = !isLeftTurn;
                SpawnBomb(spawnPoint);
            }
        }

#if UNITY_EDITOR
        // エディタでギズモ表示
        void OnDrawGizmos()
        {
            if (leftSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(leftSpawnPoint.position, 0.5f);

                // ランダム範囲表示
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                Gizmos.DrawWireCube(leftSpawnPoint.position,
                    new Vector3(spawnXRandomRange * 2f, spawnYRandomRange * 2f, 0f));
            }

            if (rightSpawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(rightSpawnPoint.position, 0.5f);

                // ランダム範囲表示
                Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
                Gizmos.DrawWireCube(rightSpawnPoint.position,
                    new Vector3(spawnXRandomRange * 2f, spawnYRandomRange * 2f, 0f));
            }
        }
#endif
    }
}