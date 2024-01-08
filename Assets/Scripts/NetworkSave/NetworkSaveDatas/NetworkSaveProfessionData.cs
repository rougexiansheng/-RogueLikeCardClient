using SDKProtocol;

using System.Collections.Generic;

/// <summary>
/// 這邊儲存玩家的職業資料
/// </summary>
public class NetworkSaveProfessionData : NetworkSaveDataBase
{
    /// <summary>
    /// 職業ID
    /// </summary>
    public ActorProfessionEnum id;

    /// <summary>
    /// 解鎖的腳色外觀id
    /// </summary>
    public List<int> unlockCharacterSkinIds;

    /// <summary>
    /// 技能解鎖
    /// </summary>
    public List<int> unlockSkillIDs;

    /// <summary>
    /// 自己設定的技能組
    /// </summary>
    public List<SkillGroup> skillGroups;

    /// <summary>
    /// 狀態解鎖(暫代)
    /// </summary>
    public List<int> unlockPassiveGourpIds;

    /// <summary>
    /// 上一次玩家選擇的角色資料
    /// </summary>
    public SelectProfessionData defalutProfessionDatas = new SelectProfessionData();

    /// <summary>
    /// 此資料的Key為英雄表ID
    /// </summary>
    public override string GetKey()
    {
        return ((int)id).ToString();
    }
}
