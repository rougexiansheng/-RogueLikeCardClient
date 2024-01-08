using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPassiveData : PerformanceData
{
    public bool isPlayer;
    public BattleActor.MonsterPositionEnum monsterPosition;
    public int passiveId;
    public bool isRemove = false;
    public void Init(BattleActor actor)
    {
        isPlayer = actor.isPlayer;
        monsterPosition = actor.monsterPos;
    }
}