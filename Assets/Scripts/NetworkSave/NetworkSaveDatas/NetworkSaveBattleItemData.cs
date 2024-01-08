using UniRx;

/// <summary>
/// 這邊儲存玩家的所有道具、貨幣、碎片等
/// </summary>
public class NetworkSaveBattleItemData : NetworkSaveDataBase
{
    /// <summary>
    /// 道具ID
    /// </summary>
    public int id;

    /// <summary>
    /// 道具數量
    /// </summary>
    public int count;

    /// <summary>
    /// 此資料的Key為ID
    /// </summary>
    public override string GetKey()
    {
        return id.ToString();
    }
}