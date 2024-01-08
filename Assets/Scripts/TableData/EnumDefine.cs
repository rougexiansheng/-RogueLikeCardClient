using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 技能
[Flags]
public enum SkillTypeEnum
{
    Attack = 1 << 0,
    Heal = 1 << 1,
    State = 1 << 2,
    Resource = 1 << 3,
    Move = 1 << 4,
    /// <summary>惡意</summary>
    Malicious = 1 << 5,
    /// <summary>召喚</summary>
    Summon = 1 << 6,
}
/// <summary>
/// 技能花費顏色 flags
/// </summary>
[Flags]
public enum SkillCostColorEnum
{
    None = 0,
    Red = 1 << 0,
    Green = 1 << 1,
    Blue = 1 << 2,
}


public enum SkillUnlockEnum
{
    None,
    Coin,
    Fragment,
}
/// <summary>
/// 動畫名稱
/// </summary>
public enum SpineAnimationEnum
{
    /// <summary>沒有動作</summary>
    None,
    /// <summary>攻擊1</summary>
    Attack01,
    /// <summary>攻擊2</summary>
    Attack02,
    /// <summary>攻擊3</summary>
    Attack03,
    /// <summary>待機1</summary>
    Idle01,
    /// <summary>待機2</summary>
    Idle02,
    /// <summary>受擊 (被點擊也可使用)</summary>
    Hit,
    /// <summary>登場</summary>
    Debut,
    /// <summary>爆衣</summary>
    DressBreak,
}

/// <summary>
/// 技能效果類型
/// </summary>
public enum SkillAbilityEnum
{
    None,
    /// <summary>傷害</summary>
    Damage,
    /// <summary>獲得顏色</summary>
    GetColor,
    /// <summary>執行技能</summary>
    ActiveSkill,
    /// <summary>給予被動(狀態)</summary>
    AffrodPassive,
    /// <summary>直接位移 技能</summary>
    SkillMove,
    /// <summary>成功擊殺執行技能</summary>
    KilledActiveSkill,
    /// <summary>依照目標被動層數給予目標被動</summary>
    TargetBuffStackAffrodPassiveForTarget,
    /// <summary>依照目標被動層數給予自身被動</summary>
    TargetBuffStackAffrodPassiveForSelf,
    /// <summary>依照自身被動層數給予目標被動</summary>
    SelfBuffStackAffrodPassiveForTarget,
    /// <summary>封印技能</summary>
    BannedSkill,
    /// <summary>花費顏色多段傷害</summary>
    CostColorCountMultipleSkill,
    /// <summary>花費欄位數量多段傷害</summary>
    CostCountRowMultipleSkill,
    /// <summary>花費欄位數量給予傷害</summary>
    CostCountRowDamage,
    /// <summary>治療</summary>
    Heal,
    /// <summary>取得隨機色</summary>
    GetRandomColor,
    /// <summary>清除被動治療</summary>
    ClearPassiveHeal,
    /// <summary>清除被動取的隨機顏色</summary>
    ClearPassiveGetColor,
    /// <summary>減少血量依照血量給予傷害斬殺後執行技能</summary>
    ReduceHpActiveSkill,
    /// <summary>目標身上被動數量 治療自己/傷害目標</summary>
    TargetPassiveStackHealOrDamage,
    /// <summary>增減護盾</summary>
    ModifyShield,
    /// <summary>依照目標的屬性造成(治療/傷害)</summary>
    AttributeHealOrDamage,
    /// <summary>創造怪物</summary>
    CreateMonster,
    /// <summary>插入技能</summary>
    InserSkill,
    /// <summary>給予傷害(不觸發任何時機)可穿透防禦/護盾</summary>
    CrossDamage,
    /// <summary>依照費用花費調整被動層數(百分比)</summary>
    ModifyPassiveStackByCost,
    /// <summary>依照自身被動層數造成(治療/傷害)</summary>
    SelfPassiveStackHealOrDamage,
    /// <summary>治療自身最大血量百分比</summary>
    HealPercentage,
    /// <summary>移除技能</summary>
    RemoveSkill,
    /// <summary>增加/減少 金幣</summary>
    ModifyCoin,
    /// <summary>修改怪物IA</summary>
    SwitchMonsterAI,
}

public enum SkillAbilityConditionEnum
{
    None,
    /// <summary>顏色花費比較</summary>
    ColorCost,
    /// <summary>被動疊層比較</summary>
    BuffStack,
    /// <summary>血量百分比</summary>
    HpPercentage,
    /// <summary>全場敵方數值最低或最高</summary>
    FirstOrLast,
    /// <summary>判斷自身被動層數</summary>
    SelfPassiveStack
}

public enum SkillTargetEnum
{
    None,
    SelectTarget,
    /// <summary>所有敵人</summary>
    AllEnemy,
    /// <summary>自身</summary>
    Self,
    /// <summary>隨機不重複</summary>
    RandomNotRepeat,
    /// <summary>指定敵方外全體(不包含目標的所有敵人)</summary>
    AllExcludeSelect,
    /// <summary>指定敵方外隨機敵方(不包含目標的所有敵人不重複)</summary>
    ExcludeSelectRandom,
    /// <summary>隨機可重複</summary>
    Random,

    /// <summary>所有友軍</summary>
    AllPartner,
    /// <summary>隨機己方不重複(不含自身)</summary>
    RandomPartnerNotRepeatExcludeSelf,
    /// <summary>隨機己方不重複(含自身)</summary>
    RandomPartnerNotRepeat,
    /// <summary>全體己方隨機(含自身/重複)</summary>
    RandomPartner,
}
public enum SkillChangeStateEnum
{
    /// <summary>技能替換狀態</summary>
    Replace,
    /// <summary>技能升級狀態</summary>
    LevelUp,
}
#endregion

#region 狀態(被動性質)
/// <summary>
/// 影響執行順序
/// heal->damage->none
/// </summary>
public enum PassivePropertyEnum
{
    None,
    Damage,
    Heal,
    Promote
}
/// <summary>
/// 狀態效果類型
/// </summary>
public enum PassiveAbilityEnum
{
    None,
    /// <summary>基礎數值修改</summary>
    ActorAttribute,
    /// <summary>擊暈/直接跳過階段</summary>
    Stun,
    /// <summary>層數減傷 最大被動層數限制</summary>
    ReduceDamageMaxStack,
    /// <summary>指定技能增傷</summary>
    SkillIncreaseDamage,
    /// <summary>反擊</summary>
    FightBack,
    /// <summary>不新增 修改(增/減)被動層數</summary>
    ModifyPassiveStack,
    /// <summary>重複執行技能</summary>
    RepeatFirstSkill,
    /// <summary>執行技能</summary>
    DoSkill,
    /// <summary>取得顏色</summary>
    GetColor,
    /// <summary>取得隨機顏色</summary>
    GetRandomColor,
    /// <summary>反傷</summary>
    Retaliation,
    /// <summary>治療</summary>
    Heal,
    /// <summary>修改(增/減)增益傷害</summary>
    ModifyDamage,
    /// <summary>減傷 最高減傷上限</summary>
    ReduceDamageLimit,
    /// <summary>忽略上指定狀態</summary>
    IgnoreAffrodPassive,
    /// <summary>轉動技能</summary>
    SkillMove,
    /// <summary>給予被動層數</summary>
    AffrodPassive,
    /// <summary>給予傷害(不觸發任何時機)可穿透防禦/護盾</summary>
    CrossDamage,
    /// <summary>單次減傷 最大減傷值 並消除層數</summary>
    ReduceOnceDamageLimit,
    [Obsolete]
    /// <summary>(已廢棄)禁用技能</summary>
    BannedSkill,
    /// <summary>反擊 技能/被動</summary>
    SpecialFightBack,
    /// <summary>依照目標的屬性造成(治療/傷害)</summary>
    AttributeHealOrDamage,
    /// <summary>(增/減)護盾</summary>
    Shield,
    /// <summary>修改(增/減)增益治療</summary>
    ModifyHeal,
    /// <summary>修改(增/減)增益護盾</summary>
    ModifyShield,
    /// <summary>修改(增/減)增益資源顏色</summary>
    ModifyColorCount,
    /// <summary>指定技能目標上被動</summary>
    AffrodPassiveFromSkillTarget,
    /// <summary>機率執行技能</summary>
    RandomDoSkill,
}

public enum PassiveAbilityConditionEnum
{
    None,
    /// <summary>Hp百分比條件</summary>
    HpPercentage,
    /// <summary>傷害來源 被動/技能</summary>
    DamageSource,
    /// <summary>被動層數檢查</summary>
    StackCheck,
    /// <summary>檢查傷害數值</summary>
    CheckDamageValue,
    /// <summary>指定技能群組技能ID才會觸發</summary>
    SkillGroupIdCheck,
}

/// <summary>
/// 狀態效果觸發時機
/// </summary>
public enum PassiveTriggerEnum
{
    None,
    /// <summary>開始戰鬥後(階段)</summary>
    BeginFightAfter,
    /// <summary>玩家回合開始前(階段)</summary>
    PlayerRoundStartBefore,
    /// <summary>玩家回合開始後(階段)</summary>
    PlayerRoundStartAfter,
    /// <summary>取得顏色前(階段)</summary>
    GetColorCostBefore,
    /// <summary>取得顏色後(階段)</summary>
    GetColorCostAfter,
    /// <summary>技能轉動前(階段)</summary>
    MoveSkillStateBefore,
    /// <summary>技能轉動後(階段)</summary>
    MoveSkillStateAfter,
    /// <summary>玩家行動前(階段)</summary>
    PlayerActionBefore,
    /// <summary>玩家行動後(階段)(按下結束回合)</summary>
    PlayerActionAfter,
    /// <summary>玩家回合結束前(階段)</summary>
    PlayerRoundEndBefore,
    /// <summary>玩家回合結束後(階段)</summary>
    PlayerRoundEndAfter,
    /// <summary>怪物回合開始前(階段)</summary>
    MonsterRoundStartBefore,
    /// <summary>怪物回合開始後(階段)</summary>
    MonsterRoundStartAfter,
    /// <summary>所有怪物怪物行動前(階段)</summary>
    MonsterActionBefore,
    /// <summary>所有怪物怪物行動後(階段)</summary>
    MonsterActionAfter,
    /// <summary>怪物回合開始前(階段)</summary>
    MonsterRoundEndBefore,
    /// <summary>怪物回合開始後(階段)</summary>
    MonsterRoundEndAfter,
    /// <summary>常駐</summary>
    Permanent,
    /// <summary>當被動修改前(增加的被動資料)</summary>
    OnPassiveModifyBefore,
    /// <summary>當被動修改後</summary>
    OnPassiveModifyAfter,
    /// <summary>施法者 攻擊前</summary>
    OnAttackBefore,
    /// <summary>施法者 攻擊後</summary>
    OnAttackAfter,
    /// <summary>目標 被攻擊前</summary>
    BeAttackBefore,
    /// <summary>目標 被攻擊後</summary>
    BeAttackAfter,
    /// <summary>施法者 治療前</summary>
    OnHealBefore,
    /// <summary>施法者 治療後</summary>
    OnHealAfter,
    /// <summary>目標 被治療前</summary>
    BeHealBefore,
    /// <summary>目標 被治療後</summary>
    BeHealAfter,
    /// <summary>玩家清除顏色花費前</summary>
    PlayerClearCostColorBefore,
    /// <summary>玩家清除顏色花費後</summary>
    PlayerClearCostColorAfter,
    /// <summary>玩家獲得顏色花費前</summary>
    PlayerGetCostColorBefore,
    /// <summary>玩家獲得顏色花費後</summary>
    PlayerGetCostColorAfter,
    /// <summary>因技能轉動技能前</summary>
    MoveSkillBySkillBefore,
    /// <summary>因技能轉動技能後</summary>
    MoveSkillBySkillAfter,
    /// <summary>施法者 主動技能之前</summary>
    OnSkillBefore,
    /// <summary>施法者 主動技能之後</summary>
    OnSkillAfter,
    /// <summary>死亡後被移除之前</summary>
    OnDeadAndRemoveBefore,
    /// <summary>死亡後被移除之後</summary>
    OnDeadAndRemoveAfter,
    /// <summary>獲得金幣前</summary>
    OnGetCoinBefore,
    /// <summary>獲得金幣後</summary>
    OnGetCoinAfter,
    /// <summary>在勝負之前(遊戲戰鬥流程結束前)</summary>
    OnLoseOrWinBefore,
    /// <summary>上盾前</summary>
    OnShieldBefore,
    /// <summary>上盾後</summary>
    OnShieldAfter,
    /// <summary>被上盾前(遊戲戰鬥流程結束前)</summary>
    BeShieldBefore,
    /// <summary>被上盾後(遊戲戰鬥流程結束前)</summary>
    BeShieldAfter,
    /// <summary>執行傷害</summary>
    OnDamage,
    /// <summary>執行治療</summary>
    OnHeal,
    /// <summary>執行上盾</summary>
    OnShield,
    /// <summary>取的顏色球前</summary>
    OnGetColor,
    /// <summary>回合開始</summary>
    RoundStart,
    /// <summary>回合結束</summary>
    RoundEnd
}

public enum PassiveStackEnum
{
    None,
    Stack,
    /// <summary>相同group會直接升級 並且疊加層數</summary>
    LevelStack,
    Replace,
}

public enum PassiveTypeEnum
{
    None,
    State,
    Passive,
    Antique
}
#endregion

[Flags]
public enum MonsterAppearEnum
{
    None,
    ShakeCamera,
    SpeedLine,
}

/// <summary>
/// 怪物AI施放技能的條件
/// </summary>
public enum AIConsiderationEnum
{
    None,
    /// <summary>
    /// 每幾回合
    /// </summary>
    EveryRound,
    /// <summary>
    /// 低於血量百分比
    /// </summary>
    HpPercentageLess,
}

public enum ItemTpyeEnum
{
    /// <summary>金幣</summary>
    Coin = 1,
    /// <summary>道具(使用道具效果表). ex: 治療藥劑，購買後可以立即回復10點HP。</summary>
    Item,
    /// <summary>技能碎片</summary>
    SkillFragment,
    /// <summary>遺物(被動)</summary>
    Antique,
    /// <summary>寶箱(全掉落)</summary>
    Chest,
    /// <summary>福袋(掉落一個)</summary>
    LuckyBag = 6,
    /// <summary>獲得造型</summary>
    Skin,
    /// <summary>角色</summary>
    Character,
    /// <summary>能量</summary>
    Energy,
    /// <summary>技能</summary>
    Skill,

}

public enum ItemEffectTypeEnum
{
    /// <summary> 治療效果</summary>
    cure = 1,
    /// <summary> 技能(升、降級)效果</summary>
    skill,
}

public enum MapNodeEnum
{
    /// <summary>怪物</summary>
    Monster,
    /// <summary>菁英怪</summary>
    EliteMonster,
    /// <summary>王</summary>
    Boss,
    /// <summary>寶箱</summary>
    Chest,
    /// <summary>商店</summary>
    Store,
    /// <summary>遺物</summary>
    Antique,
    /// <summary>休息</summary>
    Rest,
}

public enum MapType
{
    /// <summary>3選1</summary>
    ThreeBranch,
    /// <summary>5選1</summary>
    FiveBranch,
    /// <summary>2選1</summary>
    TwoBranch
}


public enum ProfessionLockEnum
{
    None,
    /// <summary>指定職業過關 arg 職業ID</summary>
    DesignationPass,
    /// <summary>購買 arg道具ID</summary>
    Shop,
}

/// <summary>
/// 特效顯示位置 UI上下
/// </summary>
public enum EffectPosEnum
{
    /// <summary>無</summary>
    None,
    /// <summary>上方前方</summary>
    TopFront,
    /// <summary>中心前方</summary>
    CenterFront,
    /// <summary>底部前方</summary>
    BottomFront,
    /// <summary>武器前方</summary>
    WeaponFront,
    /// <summary>UI中心</summary>
    ViewCenter,
    /// <summary>場景中心</summary>
    SceneCenter,
    /// <summary>上方後方</summary>
    TopBack,
    /// <summary>中心後方</summary>
    CenterBack,
    /// <summary>底部後方</summary>
    BottomBack,
    /// <summary>武器後方</summary>
    WeaponBack,
    /// <summary>身體前方</summary>
    BodyFront,
    /// <summary>身體後方</summary>
    BodyBack,
}

public enum ActorProfessionEnum
{
    None,
    /// <summary>女巫</summary>
    Witch,
    /// <summary>聖騎士</summary>
    Paladin,
    /// <summary>擊劍士</summary>
    Fencer,
    /// <summary>刺客</summary>
    Assassin,
    /// <summary>傭兵</summary>
    Mercenary,
    /// <summary>學者</summary>
    Investigator,
    /// <summary>遊俠</summary>
    Ranger,
    /// <summary>園藝師</summary>
    Gardener,
    /// <summary>龍騎士</summary>
    DragonKnight,





    /// <summary>怪物</summary>
    Monster = 101,
    /// <summary>不顯示的技能</summary>
    NotShow = 501,
    /// <summary>通用</summary>
    Common = 1001,
}
#region

public enum ViewItemType
{
    ItemData,
    SkillData,
}

#endregion

#region
#endregion



