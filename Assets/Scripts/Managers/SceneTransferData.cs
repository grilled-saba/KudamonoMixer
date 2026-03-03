namespace FruitMixer.Managers
{
    /// <summary>
    /// シーン間データ受け渡し用（staticのみ）
    /// DontDestroyOnLoadを使わずにシーン間でデータを渡す
    /// </summary>
    public static class SceneTransferData
    {
        public static UnityEngine.Sprite LastEscapedFruitSprite { get; set; }
    }
}