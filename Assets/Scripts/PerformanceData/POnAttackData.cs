using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POnAttackData : PerformanceData
{
    public bool isPlayer;
    public BattleActor.MonsterPositionEnum monsterPosition;
    public int monsterId;
    public int skillId;
    public void Init(BattleActor actor,int skillId)
    {
        isPlayer = actor.isPlayer;
        monsterPosition = actor.monsterPos;
        monsterId = actor.monsterId;
        this.skillId = skillId;
    }
}
