using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FruitMixer.Managers;

namespace FruitMixer.UI
{
    /// <summary>
    /// 爆弾回収カウントHUD表示
    /// 画面左上に X/3 (💣) 形式で表示
    /// </summary>
    public class BombCountUI : MonoBehaviour
    {
        [Header("UI要素")]
        [Tooltip("カウントテキスト (X/3)")]
        [SerializeField] private TextMeshProUGUI countText;

        [Tooltip("爆弾アイコン画像 (オプション)")]
        [SerializeField] private Image bombIcon;

        [Header("デバッグ設定")]
        [Tooltip("デバッグログを表示")]
        [SerializeField] private bool showDebugLog = false;

        void Update()
        {
            UpdateCountDisplay();
        }

        /// <summary>
        /// カウント表示を更新
        /// </summary>
        private void UpdateCountDisplay()
        {
            if (GameManager.Instance != null && countText != null)
            {
                int current = GameManager.Instance.GetDeleteCount();
                int max = GameManager.Instance.GetMaxDeleteCount();

                // テキスト更新
                countText.text = $"{current} / {max}";
            }
        }

        /// <summary>
        /// 爆弾アイコンを設定（Inspectorから呼ぶ用）
        /// </summary>
        public void SetBombIcon(Sprite iconSprite)
        {
            if (bombIcon != null && iconSprite != null)
            {
                bombIcon.sprite = iconSprite;

                if (showDebugLog)
                {
                    Debug.Log("[BombCountUI] 爆弾アイコン設定完了");
                }
            }
        }
    }
}