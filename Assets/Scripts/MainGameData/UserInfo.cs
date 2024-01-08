using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo
{
    public string userName;
    public int coin;
    public int gem;
    /// <summary>碎片</summary>
    public int fragment;
    /// <summary>解鎖的腳色id</summary>
    public List<int> unlockCharacterIds;
    /// <summary>解鎖的腳色外觀id</summary>
    public List<int> unlockCharacterSkinIds;
    /// <summary>狀態解鎖</summary>
    public List<int> unlockPassiveGourpIds;
    /// <summary>技能解鎖</summary>
    public List<int> unlockSkillGourpIds;
    //商品購買次數
    //當前關卡 後三關
}
