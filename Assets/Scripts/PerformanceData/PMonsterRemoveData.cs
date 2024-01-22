using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMonsterRemoveData : PerformanceData
{
    public List<BattleActor.MonsterPositionEnum> positions = new List<BattleActor.MonsterPositionEnum>();
    public List<SDKProtocol.ItemData> acquisitionList;
    public int monsterID;
}
