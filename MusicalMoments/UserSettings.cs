public class UserSettings
{
    public int VBAudioEquipmentInputIndex { get; set; }
    public int AudioEquipmentInputIndex { get; set; }
    public int VBAudioEquipmentOutputIndex { get; set; }
    public int AudioEquipmentOutputIndex { get; set; }
    public string ToggleStreamKey { get; set; }
    public string PlayAudioKey { get; set; }
    public bool AudioEquipmentPlay { get; set; }
    public bool SwitchStreamTips { get; set; }
    public string SameAudioPressBehavior { get; set; }
    public string DifferentAudioInterruptBehavior { get; set; }
    public bool? RestoreDefaultsAfterInstall { get; set; }
    public float VBVolume { get; set; }
    public float Volume { get; set; }
    public float TipsVolume { get; set; }
    public int CloseCount { get; set; }
    public int PlayedCount { get; set; }
    public int ChangedCount { get; set; }
    public string FirstStart { get; set; }
}
