using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POnHealData : PerformanceData
{
    public bool isPlayer;
    public int currentHp;
    public int maxHp;
    public int heal;
    public BattleActor.MonsterPositionEnum monsterPos;
    public int monsterId;
    public void Init(BattleActor actor,int heal)
    {
        isPlayer = actor.isPlayer;
        if (!isPlayer)
        {
            monsterPos = actor.monsterPos;
            monsterId = actor.monsterId;
        }
        currentHp = actor.currentHp;
        maxHp = actor.currentActorBaseAttribute.maxHp.GetValue();
        this.heal = heal;
    }
}
