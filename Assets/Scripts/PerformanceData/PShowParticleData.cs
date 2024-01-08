using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PShowParticleData : PerformanceData
{
    [NonSerialized]
    public ParticleItem particle;
    public bool isPlayer;
    public BattleActor.MonsterPositionEnum monsterPosition;
    public EffectPosEnum position;
    public AudioClip sound;
    //以下是被動使用
    public int passiveId;
    /// <summary>是否等待特效結束</summary>
    public bool isPassive = false;
    /// <summary>如果被動 沒有執行 就不表演特效</summary>
    public bool isIgnore = false;
    /// <summary>等待特效表演完</summary>
    public bool needWaitDestroy = false;
    public void Init(SkillAbilityDataDefine abilityData, BattleActor actor)
    {
        particle = abilityData.hitEffect;
        position = abilityData.hitEffectPos;
        sound = abilityData.hitEffectSound;
        isPlayer = actor.isPlayer;
        if (!isPlayer) monsterPosition = actor.monsterPos;
        position = abilityData.hitEffectPos;
        isPassive = false;
    }

    public void Init(PassiveDataDefine passiveDataDefine, BattleActor actor)
    {
        passiveId = passiveDataDefine.id;
        particle = passiveDataDefine.effect;
        position = passiveDataDefine.effectPos;
        sound = passiveDataDefine.effectSound;
        isPlayer = actor.isPlayer;
        if (!isPlayer) monsterPosition = actor.monsterPos;
        position = passiveDataDefine.effectPos;
        isPassive = true;
    }
}
