using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMonsterAppearData : PerformanceData
{
    public List<BattleActor.MonsterPositionEnum> positions = new List<BattleActor.MonsterPositionEnum>();
    public bool isBoss;
}
