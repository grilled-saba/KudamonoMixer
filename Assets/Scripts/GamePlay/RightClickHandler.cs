using FruitMixer.Core;
using FruitMixer.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FruitMixer.Gameplay
{
    /// <summary>
       /// 右クリック処理
       /// - ミキサー外 フルーツ: 上に押し上げ
    /// - ミキサー外 爆弾: 回収（削除カウント +1）
    /// - ミキサー内 フルーツ/爆弾: 削除（カウント消費）
    /// </summary>
    public class RightClickHandler : MonoBehaviour
    {
        [Header("設定")]
        [Tooltip("右クリック判定レイヤー")]
        [SerializeField] private LayerMask clickLayerMask = -1;

        [Tooltip("右クリック判定半径（範囲を広げる）")]
        [SerializeField] private float clickRadius = 0.5f;

        [Tooltip("フルーツを押し上げる力（ミキサー外）")]
        [SerializeField] private float fruitUpwardForce = 3f;

        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;

        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            // ゲーム一時停止中は入力無効
            if (GameManager.Instance != null && GameManager.Instance.IsPaused())
            {
                return;
            }

            HandleRightClick();
        }

        /// <summary>
              /// 右クリック入力処理
        /// </summary>
        private void HandleRightClick()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            // 右クリック（マウス1番）
            if (mouse.rightButton.wasPressedThisFrame)
            {
                Vector2 mouseWorldPos = GetMouseWorldPosition(mouse.position.ReadValue());
                CheckRightClick(mouseWorldPos);
            }
        }

        /// <summary>
        /// 右クリック判定
        /// </summary>
        private void CheckRightClick(Vector2 worldPos)
        {
            // 範囲 内のすべてのオブジェクトを取得
            RaycastHit2D[] allHits = Physics2D.CircleCastAll(worldPos, clickRadius, Vector2.zero, 0f, clickLayerMask);

            if (allHits.Length == 0)
            {
                if (showDebugLog)
                {
                    Debug.Log($"[RightClick] ❌ 何も検出されず at {worldPos} (半径: {clickRadius})");
                }
                return;
            }

            // デバッグ: 検出されたオブジェクト一覧
            if (showDebugLog)
            {
                Debug.Log($"[RightClick] 右クリック位置で検出: {allHits.Length}個 (半径: {clickRadius})");
                foreach (var h in allHits)
                {
                    float distance = Vector2.Distance(worldPos, h.point);
                    Debug.Log($"  - {h.collider.gameObject.name} (距離: {distance:F2})");
                }
            }

            // クリック位置から最も近いオブジェクトを選択
            RaycastHit2D closestHit = allHits[0];
            float closestDistance = Vector2.Distance(worldPos, allHits[0].point);

            for (int i = 1; i < allHits.Length; i++)
            {
                float distance = Vector2.Distance(worldPos, allHits[i].point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHit = allHits[i];
                }
            }

            // 最も近いオブジェクトを処理
            if (showDebugLog)
            {
                Debug.Log($"[RightClick] 右クリック対象: {closestHit.collider.gameObject.name} (距離: {closestDistance:F2})");
            }

            HandleRightClickedObject(closestHit.collider.gameObject);
        }

        /// <summary>
              /// 右クリックされたオブジェクト処理
        /// </summary>
        private void HandleRightClickedObject(GameObject obj)
        {
            // 爆弾チェック
            BombData bomb = obj.GetComponent<BombData>();
            if (bomb != null)
            {
                if (bomb.isInMixer)
                {
                    // ミキサー内: 削除（カウント消費 + 弱い爆発）
                    if (!GameManager.Instance.UseDeleteCount())
                    {
                        if (showDebugLog)
                        {
                            Debug.Log($"[RightClick] ❌ 削除カウント不足（爆弾）");
                        }
                        return;
                    }

                    if (showDebugLog)
                    {
                        Debug.Log($"[RightClick] 💥 ミキサー内の爆弾を削除 → 弱い爆発");
                    }

                    // ⚠️ 중요: 폭발 효과 발생!
                    bomb.Explode();
                }
                else
                {
                    // ミキサー外: 回収（削除カウント +1）
                    if (showDebugLog)
                    {
                        Debug.Log($"[RightClick] 🎯 爆弾を回収（ミキサー外）");
                    }
                    GameManager.Instance.AddDeleteCount();
                    Destroy(obj);
                }
                return;
            }

            // フルーツチェック
            FruitData fruit = obj.GetComponent<FruitData>();
            if (fruit != null)
            {
                if (fruit.isInMixer)
                {
                    // ミキサー内: 削除（カウント消費）
                    if (!GameManager.Instance.UseDeleteCount())
                    {
                        if (showDebugLog)
                        {
                            Debug.Log($"[RightClick] ❌ 削除カウント不足（フルーツ）");
                        }
                        return;
                    }

                    if (showDebugLog)
                    {
                        Debug.Log($"[RightClick] 🗑️ {fruit.GetFruitType()} を削除（ミキサー内・カウント消費）");
                    }

                    // ⚠️ 重要: ゲームオーバー判定を避けるため、フラグをリセット
                    fruit.hasEverEnteredMixer = false;

                    Destroy(obj);
                }
                else
                {
                    // ミキサー外: 上に押し上げ
                    Rigidbody2D rb = fruit.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.AddForce(Vector2.up * fruitUpwardForce, ForceMode2D.Impulse);

                        if (showDebugLog)
                        {
                            Debug.Log($"[RightClick] ⬆️ {fruit.GetFruitType()} を押し上げ（力: {fruitUpwardForce}）");
                        }
                    }
                }
                return;
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