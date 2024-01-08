using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POnDamageData : PerformanceData
{
    public bool isPlayer;
    public int currentHp;
    public int maxHp;
    public int dmg;
    public BattleActor.MonsterPositionEnum monsterPos;
    public int monsterId;
    public bool isBlock;
    public void Init(BattleActor actor,(int,bool) result)
    {
        isPlayer = actor.isPlayer;
        if (!isPlayer)
        {
            monsterPos = actor.monsterPos;
            monsterId = actor.monsterId;
        }
        dmg = result.Item1;
        isBlock = result.Item2;
        currentHp = actor.currentHp;
        maxHp = actor.currentActorBaseAttribute.maxHp.GetValue();
    }
}
