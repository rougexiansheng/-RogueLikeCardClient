using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestActor : MonoBehaviour
{
    [Header("玩家資訊")]
    public BattleActor actor;
    [Header("技能ID")]
    public int skillId;
    [Header("技能消耗")]
    public List<SkillCostColorData> cost1;
    public List<SkillCostColorData> cost2;
    public List<SkillCostColorData> cost3;
    public List<SkillCostColorData> cost4;
    [Header("新增被動資訊")]
    public ActorPassive addPassive;
    [Header("施放者")]
    public TestActor sender;
    [Header("擁有者")]
    public TestActor owner;
    
    [Header("觸發被動時機個人")]
    public PassiveTriggerEnum triggerEnum;
    [SerializeField]
    TestSystem testSystem;
    [InspectorButton]
    public void Trigger()
    {
        var battleData = new BattleData();
        battleData.isSkill = false;
        try
        {
            testSystem.passiveManager.OnActorPassive(actor, triggerEnum, battleData);
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
    [InspectorButton("基礎數值And能量求數量")]
    void GetCurrentActorAttributeAndLogColor()
    {
        testSystem.passiveManager.GetCurrentActorAttribute(actor);
        var maxHp = actor.currentActorBaseAttribute.maxHp.GetValue();
        var def = actor.currentActorBaseAttribute.defense.GetValue();
        var atk = actor.currentActorBaseAttribute.attackPower.GetValue();
        var colorCount = actor.currentActorBaseAttribute.currentColorCount;
        Debug.Log("-------------------------------------------------------------");
        Debug.Log($"MaxHp:{maxHp} Def:{def} Atk:{atk} ColorCount:{colorCount} currentMove:{actor.currentActorBaseAttribute.currentMove}");
        foreach (var item in actor.colors)
        {
            Debug.Log($"顏色:{item.Key} 數量:{item.Value}");
        }
        Debug.Log("-------------------------------------------------------------");
    }

    [InspectorButton]
    void AddPassive()
    {
        var battleData = new BattleData();
        battleData.isSkill = false;
        battleData.modifyActorPassive = testSystem.passiveManager.GainActorPassive(addPassive.passiveId, addPassive.currentStack);
        battleData.modifyActorPassive.sender = sender == null? actor : sender.actor;
        battleData.modifyActorPassive.owner = owner == null? actor : owner.actor;
        try
        {
            if (addPassive.isAdd)
                testSystem.passiveManager.AddPassive(battleData);
            else
                testSystem.passiveManager.ClearPassive(battleData);
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    [InspectorButton]
    void Attack()
    {
        testSystem.Attack(this);
    }
}
