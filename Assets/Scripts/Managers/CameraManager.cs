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
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name == "AIBattleScene")
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
    }
}