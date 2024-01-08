using UniRx;

/// <summary>
/// 這邊儲存玩家戰鬥中的技能
/// </summary>
public class NetworkSaveBattleSkillData : NetworkSaveDataBase
{
    /// <summary>
    /// 狀態ID
    /// </summary>
    public int skillId;

    /// <summary>
    /// 是否已經使用過
    /// </summary>
    public bool isUsed;

    /// <summary>
    /// 原始位置
    /// </summary>
    public int index;
    /// <summary>
    /// 當前的位置
    /// </summary>
    public int currentIdx;
    /// <summary>
    /// 此資料的Key為索引
    /// </summary>
    public override string GetKey()
    {
        return index.ToString();
    }
}