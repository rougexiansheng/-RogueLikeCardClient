using UniRx;
using static NetworkSaveBattleHeroAttrContainer;

/// <summary>
/// 這邊儲存玩家的戰鬥中暫存的屬性值
/// </summary>
public class NetworkSaveBattleHeroAttrData : NetworkSaveDataBase
{
    /// <summary>
    /// 屬性類型
    /// </summary>
    public AttrType type;

    /// <summary>
    /// 屬性ID
    /// </summary>
    public int id;

    /// <summary>
    /// 數值
    /// </summary>
    public int value;

    /// <summary>
    /// 此資料的Key為ID
    /// </summary>
    public override string GetKey()
    {
        return $"{((int)type)}_{id}";
    }
}