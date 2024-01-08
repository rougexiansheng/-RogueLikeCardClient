using System.Collections.Generic;

/// <summary>
/// 這邊單純是玩家資料, 不儲存貨幣, 貨幣要到ItemContainer獲取
/// </summary>
public class NetworkSavePlayerData : NetworkSaveDataBase
{
    /// <summary>
    /// 玩家名稱
    /// </summary>
    public string username;

    /// <summary>
    /// 伺服器給玩家的唯一ID
    /// </summary>
    public string uid;

    /// <summary>
    /// 解鎖的角色id
    /// </summary>
    public List<int> unlockProfessionIds;

    /// <summary>
    /// 解鎖的時裝id
    /// </summary>
    public List<int> unlockSkinIds;
}