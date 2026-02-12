using UnityEngine;

namespace FruitMixer.Managers
{
    /// <summary>
    /// オーディオ管理シングルトン
    /// BGMをシーン遷移後も継続再生
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM設定")]
        [Tooltip("メインBGM")]
        [SerializeField] private AudioClip mainBGM;

        [Tooltip("BGM音量")]
        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 0.5f;

        private AudioSource audioSource;

        void Awake()
        {
            // シングルトンパターン
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // AudioSource取得
                audioSource = GetComponent<AudioSource>();

                // BGM設定
                audioSource.clip = mainBGM;
                audioSource.volume = bgmVolume;
                audioSource.loop = true;
                audioSource.playOnAwake = false;

                Debug.Log("[AudioManager] AudioManager初期化完了");
            }
            else
            {
                // 既に存在する場合は削除
                Destroy(gameObject);
                Debug.Log("[AudioManager] 重複削除");
            }
        }

        void Start()
        {
            // GameSceneでBGM開始
            if (Instance == this && mainBGM != null)
            {
                PlayBGM();
            }
        }

        /// <summary>
        /// BGMを再生
        /// </summary>
        public void PlayBGM()
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("[AudioManager] BGM再生開始");
            }
        }

        /// <summary>
        /// BGMを停止
        /// </summary>
        public void StopBGM()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("[AudioManager] BGM停止");
            }
        }

        /// <summary>
        /// BGMを一時停止
        /// </summary>
        public void PauseBGM()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log("[AudioManager] BGM一時停止");
            }
        }

        /// <summary>
        /// BGMを再開
        /// </summary>
        public void ResumeBGM()
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
                Debug.Log("[AudioManager] BGM再開");
            }
        }

        /// <summary>
        /// BGM音量を設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(volume);
                bgmVolume = audioSource.volume;
                Debug.Log($"[AudioManager] BGM音量: {bgmVolume:F2}");
            }
        }

        /// <summary>
        /// 現在のBGM音量を取得
        /// </summary>
        public float GetBGMVolume()
        {
            return bgmVolume;
        }
    }
}