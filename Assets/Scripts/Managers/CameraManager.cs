using UnityEngine;

namespace FruitMixer.Managers
{
    /// <summary>
    /// カメラ管理
    /// シーン遷移時のMain Camera重複を防止
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        void Awake()
        {
            // シングルトンパターン
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[CameraManager] メインカメラ初期化完了");
            }
            else
            {
                // 既に存在する場合は削除
                Debug.Log("[CameraManager] 重複カメラ削除");
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}