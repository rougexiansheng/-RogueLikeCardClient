using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class NumericalManager : IInitializable
{

    [Inject]
    NetworkSaveManager saveManager;
    private float healBaseValue = .2f;
    private float coinBaseValue = 1f;
    public void Initialize()
    {

    }

    /// <summary>
    /// 營火事件治療
    /// </summary>
    /// <param name="currentHP"></param>
    /// <returns></returns>
    public int GetRestHPValue(int currentHP)
    {
        var antiqueList = saveManager.GetContainer<NetworkSaveBattleItemContainer>().GetDatas(ItemTpyeEnum.Antique);
        return (int)(currentHP * healBaseValue);
    }

    /// <summary>
    /// 獎勵三選一治療
    /// </summary>
    /// <param name="currentHP"></param>
    /// <returns></returns>
    public int GetHealHPValue(int currentHP)
    {
        return (int)(currentHP * healBaseValue);
    }

    /// <summary>
    /// 取得實際增加的金額
    /// </summary>
    /// <param name="targetValue"></param>
    /// <returns></returns>
    public int GetCoinValue(int targetValue)
    {
        return (int)(targetValue * coinBaseValue);
    }


}
