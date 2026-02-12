using FruitMixer.Core;
using FruitMixer.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FruitMixer.Gameplay
{
    /// <summary>
    /// フルーツと爆弾の切断システム
    /// Penetration検出方式で自然な切断タイミング
    /// </summary>
    public class FruitSlicer : MonoBehaviour
    {
        [Header("切断設定")]
        [Tooltip("切断可能なレイヤー")]
        [SerializeField] private LayerMask sliceLayer = -1;

        [Tooltip("最小切断距離（ピクセル）")]
        [SerializeField] private float minSliceDistance = 5f;

        [Header("ビジュアルフィードバック")]
        [Tooltip("切断軌跡（オプション）")]
        [SerializeField] public TrailRenderer sliceTrail;

        [Tooltip("軌跡表示時間")]
        [SerializeField] private float trailTime = 0.2f;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugRay = true;

        // 内部
        private Camera mainCamera;
        private bool isSlicing = false;
        private Vector2 lastMousePosition;

        // Penetration検出用
        private HashSet<GameObject> objectsCurrentlyInside = new HashSet<GameObject>();

        void Start()
        {
            mainCamera = Camera.main;

            // TrailRenderer設定
            if (sliceTrail != null)
            {
                sliceTrail.time = trailTime;
                sliceTrail.emitting = false;
            }
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused())
            {
                return;
            }

            HandleSlicing();
        }

        /// <summary>
        /// 切断入力処理
        /// </summary>
        private void HandleSlicing()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            // 左クリック押し始め
            if (mouse.leftButton.wasPressedThisFrame)
            {
                StartSlice();
            }

            // 左クリック中
            if (mouse.leftButton.isPressed && isSlicing)
            {
                ContinueSlice(mouse.position.ReadValue());
            }

            // 左クリック離した
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                EndSlice();
            }
        }

        /// <summary>
        /// 切断開始
        /// </summary>
        private void StartSlice()
        {
            isSlicing = true;
            objectsCurrentlyInside.Clear();

            Mouse mouse = Mouse.current;
            lastMousePosition = mouse.position.ReadValue();

            // TrailRenderer開始
            if (sliceTrail != null)
            {
                sliceTrail.Clear();
                sliceTrail.emitting = true;
                Vector3 worldPos = GetMouseWorldPosition(lastMousePosition);
                sliceTrail.transform.position = worldPos;
            }
        }

        /// <summary>
        /// 切断継続（Penetration検出）
        /// </summary>
        private void ContinueSlice(Vector2 currentScreenPos)
        {
            // 最小距離チェック
            if (Vector2.Distance(lastMousePosition, currentScreenPos) < minSliceDistance)
            {
                return;
            }

            Vector2 currentWorldPos = GetMouseWorldPosition(currentScreenPos);
            Vector2 lastWorldPos = GetMouseWorldPosition(lastMousePosition);

            // TrailRenderer更新
            if (sliceTrail != null)
            {
                sliceTrail.transform.position = currentWorldPos;
            }

            // デバッグライン
            if (showDebugRay)
            {
                Debug.DrawLine(lastWorldPos, currentWorldPos, Color.red, 0.5f);
            }

            // Penetration検出
            DetectPenetration(lastWorldPos, currentWorldPos);

            lastMousePosition = currentScreenPos;
        }

        /// <summary>
        /// Penetration検出（3Dスクリプトの核心ロジック）
        /// </summary>
        private void DetectPenetration(Vector2 startPos, Vector2 endPos)
        {
            // ライン上のすべてのオブジェクトを検出
            RaycastHit2D[] hits = Physics2D.LinecastAll(startPos, endPos, sliceLayer);

            // 今フレームで検出されたオブジェクト
            HashSet<GameObject> objectsHitThisFrame = new HashSet<GameObject>();

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    objectsHitThisFrame.Add(hit.collider.gameObject);
                }
            }

            // Penetration判定: 前フレームにあって今フレームにない = "抜けた"
            List<GameObject> objectsToSlice = new List<GameObject>();

            foreach (GameObject obj in objectsCurrentlyInside)
            {
                if (!objectsHitThisFrame.Contains(obj))
                {
                    // マウスがオブジェクトから抜けた！
                    objectsToSlice.Add(obj);
                }
            }

            // 切断実行
            foreach (GameObject obj in objectsToSlice)
            {
                Vector2 sliceDirection = (endPos - startPos).normalized;
                HandleSlicedObject(obj, sliceDirection);
            }

            // 現在の状態を更新
            objectsCurrentlyInside = objectsHitThisFrame;
        }

        /// <summary>
        /// 切断されたオブジェクト処理
        /// </summary>
        private void HandleSlicedObject(GameObject obj, Vector2 sliceDirection)
        {
            // オブジェクトが既に削除されていないかチェック
            if (obj == null) return;

            // 爆弾チェック
            BombData bomb = obj.GetComponent<BombData>();
            if (bomb != null)
            {
                // ⚠️ ミキサー内の爆弾は切断不可!
                if (bomb.isInMixer)  // ← 추가!
                {
                    Debug.Log($"[FruitSlicer] ⚠️ ミキサー内の爆弾は切断不可");
                    return;
                }

                Debug.Log($"[FruitSlicer] 💥 爆弾を切断！");
                bomb.Explode();
                return;
            }

            // フルーツチェック
            FruitData fruit = obj.GetComponent<FruitData>();
            if (fruit != null)
            {
                // 切断可能かチェック
                if (fruit.isSliced)
                {
                    Debug.Log($"[FruitSlicer] ⚠️ すでに切断済み: {fruit.GetFruitType()}");
                    return;
                }

                if (fruit.isInMixer)
                {
                    Debug.Log($"[FruitSlicer] ⚠️ ミキサー内は切断不可: {fruit.GetFruitType()}");
                    return;
                }

                // フルーツ切断実行
                SliceFruit(fruit, sliceDirection);
                return;
            }
        }


        /// <summary>
        /// フルーツを切断（2つの半分に分割）
        /// </summary>
        private void SliceFruit(FruitData fruitData, Vector2 sliceDirection)
        {
            Debug.Log($"[FruitSlicer] 🔪 フルーツを切断: {fruitData.GetFruitType()} (Tier {fruitData.currentTier})");

            // FruitDataのOnSliced()呼び出し（ティア減少）
            fruitData.OnSliced();

            FruitInfo fruitInfo = fruitData.GetFruitInfo();
            if (fruitInfo == null)
            {
                Debug.LogError("[FruitSlicer] FruitInfo が null です！");
                return;
            }

            // === 原本の情報を保存（削除前に取得） ===
            GameObject originalObj = fruitData.gameObject;
            Vector3 originalPosition = originalObj.transform.position;
            Vector2 originalVelocity = originalObj.GetComponent<Rigidbody2D>().linearVelocity;
            int originalLayer = originalObj.layer;
            Quaternion originalRotation = originalObj.transform.rotation;

            // 原本のコライダー情報も保存
            PolygonCollider2D originalCollider = originalObj.GetComponent<PolygonCollider2D>();

            // 原本のPhysics設定も保存
            Rigidbody2D originalRb = originalObj.GetComponent<Rigidbody2D>();
            float originalGravityScale = originalRb != null ? originalRb.gravityScale : 1f;
            float originalMass = originalRb != null ? originalRb.mass : 1f;

            // 原本のSprite設定も保存
            SpriteRenderer originalSprite = originalObj.GetComponent<SpriteRenderer>();
            int sortingLayerID = originalSprite != null ? originalSprite.sortingLayerID : 0;
            int sortingOrder = originalSprite != null ? originalSprite.sortingOrder : 0;

            // === Half_A: 新しいGameObjectを左半分として生成 ===
            GameObject halfA = new GameObject($"{fruitInfo.fruitType}_HalfA");
            halfA.transform.position = originalPosition + Vector3.left * 0.2f;
            halfA.transform.rotation = originalRotation;
            halfA.layer = originalLayer;

            // Half_Aスプライト設定（A優先、なければBを使用）
            SpriteRenderer spriteA = halfA.AddComponent<SpriteRenderer>();
            Sprite halfSpriteA = fruitInfo.slicedHalfSprite_A != null ?
                fruitInfo.slicedHalfSprite_A : fruitInfo.slicedHalfSprite_B;

            if (halfSpriteA != null)
            {
                spriteA.sprite = halfSpriteA;
            }
            else
            {
                Debug.LogWarning($"[FruitSlicer] {fruitInfo.fruitType} の slicedHalfSprite が設定されていません！");
            }
            spriteA.sortingLayerID = sortingLayerID;
            spriteA.sortingOrder = sortingOrder;

            // Half_A物理設定
            Rigidbody2D rbA = halfA.AddComponent<Rigidbody2D>();
            rbA.gravityScale = originalGravityScale;
            rbA.mass = originalMass;

            // Half_AコライダーをPolygonCollider2Dで追加
            if (originalCollider != null)
            {
                PolygonCollider2D colliderA = halfA.AddComponent<PolygonCollider2D>();
                // 原本のパスをコピー
                for (int i = 0; i < originalCollider.pathCount; i++)
                {
                    Vector2[] path = originalCollider.GetPath(i);
                    colliderA.SetPath(i, path);
                }
                colliderA.isTrigger = originalCollider.isTrigger;
            }

            // Half_AにFruitDataコンポーネント追加
            FruitData fruitDataA = halfA.AddComponent<FruitData>();
            fruitDataA.Initialize(fruitInfo);
            fruitDataA.isSliced = true; // すでに切断済み
            fruitDataA.currentTier = fruitData.currentTier; // 同じティア
            fruitDataA.isInMixer = false;

            // Half_AにFruitMergerコンポーネント追加
            FruitMerger mergerA = halfA.AddComponent<FruitMerger>();


            // 切断方向の反対に跳ね返る物理効果
            Vector2 bounceDirection = -sliceDirection.normalized;
            Vector2 bounceForce = new Vector2(
                bounceDirection.x * 1.5f,  // 水平反発
                Mathf.Abs(bounceDirection.y) * 2f + 1.5f  // 垂直は常に上向き
            );

            // Half_Aに物理力を適用
            rbA.linearVelocity = originalVelocity * 0.7f; // 元の速度を70%に減速
            rbA.AddForce(bounceForce, ForceMode2D.Impulse); // 反発力追加
            rbA.AddTorque(Random.Range(-50f, -20f)); // 左回転

            // Initialize後、再度スプライト設定（Initializeでwholが設定される可能性対策）
            if (halfSpriteA != null)
            {
                spriteA.sprite = halfSpriteA;
            }

            // ⚠️ 重要: 原本のGameObjectを削除
            Destroy(originalObj);

            // === Half_B: 新しいGameObjectを右半分として生成 ===
            GameObject halfB = new GameObject($"{fruitInfo.fruitType}_HalfB");
            halfB.transform.position = originalPosition + Vector3.right * 0.2f;
            halfB.transform.rotation = halfA.transform.rotation;
            halfB.layer = halfA.layer;

            // Half_Bスプライト設定（B優先、なければAを使用）
            SpriteRenderer spriteB = halfB.AddComponent<SpriteRenderer>();
            Sprite halfSpriteB = fruitInfo.slicedHalfSprite_B != null ?
                fruitInfo.slicedHalfSprite_B : fruitInfo.slicedHalfSprite_A;

            if (halfSpriteB != null)
            {
                spriteB.sprite = halfSpriteB;
            }
            spriteB.sortingLayerID = spriteA.sortingLayerID;
            spriteB.sortingOrder = spriteA.sortingOrder;

            // Half_B物理設定
            Rigidbody2D rbB = halfB.AddComponent<Rigidbody2D>();
            rbB.linearVelocity = originalVelocity * 0.7f; // 元の速度を70%に減速
            rbB.gravityScale = rbA.gravityScale;
            rbB.mass = rbA.mass;
            rbB.AddForce(bounceForce, ForceMode2D.Impulse); // Half_Aと同じ反発力
            rbB.AddTorque(Random.Range(20f, 50f)); // 右回転

            // Half_BコライダーをHalf_Aと同じように追加
            PolygonCollider2D halfACollider = halfA.GetComponent<PolygonCollider2D>();  // ← PolygonCollider2Dに合わせて変更
            if (halfACollider != null)
            {
                PolygonCollider2D colliderB = halfB.AddComponent<PolygonCollider2D>();  // ← PolygonCollider2Dに合わせて変更

                // Half_AのパスをコピーしてHalf_Bに適用
                for (int i = 0; i < halfACollider.pathCount; i++)
                {
                    Vector2[] path = halfACollider.GetPath(i);
                    colliderB.SetPath(i, path);
                }

                colliderB.isTrigger = halfACollider.isTrigger;
            }

            // Half_BにFruitDataコンポーネント追加（同じ情報共有）
            FruitData fruitDataB = halfB.AddComponent<FruitData>();
            fruitDataB.Initialize(fruitInfo);
            fruitDataB.isSliced = true; // すでに切断済み
            fruitDataB.currentTier = fruitData.currentTier; // 同じティア
            fruitDataB.isInMixer = false;

            // ⚠️ 重要: FruitMergerコンポーネントも追加（合成可能にする）
            FruitMerger mergerB = halfB.AddComponent<FruitMerger>();


            // ⚠️ 注意: FruitMergerのAwake()で自動的にfruitPrefabを取得するため
            // リフレクションによる参照コピーは不要（削除済み）

            // Initialize後、再度スプライト設定（Initializeでwholが設定される可能性対策）
            if (halfSpriteB != null)
            {
                spriteB.sprite = halfSpriteB;
            }

            Debug.Log($"[FruitSlicer] ✅ 切断完了 → 2つの半分に分割 (Tier {fruitData.currentTier})");
        }

        /// <summary>
        /// 切断終了
        /// </summary>
        private void EndSlice()
        {
            isSlicing = false;
            objectsCurrentlyInside.Clear();

            // TrailRenderer停止
            if (sliceTrail != null)
            {
                sliceTrail.emitting = false;
            }
        }

        /// <summary>
        /// マウスのワールド座標取得
        /// </summary>
        private Vector2 GetMouseWorldPosition(Vector2 screenPos)
        {
            Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPoint);
        }
    }
}