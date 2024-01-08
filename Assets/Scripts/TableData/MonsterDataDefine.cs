using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Spine.Unity;

public class MonsterDataDefine
{
    public int id;
    public string name;
    public int maxHp;
    public int atk;
    public int def;
    public List<int> passives = new List<int>();
    public int aiId;
    public int dropGroupId;
    public int dropCount;
    public List<MonsterAppearEnum> appearEnums = new List<MonsterAppearEnum>();
    public GameObject spineObj;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip showSound;
}

/// <summary>
/// 原始的怪物資料
/// </summary>
public class MonsterOriginDefine : OriginDefineBase<MonsterDataDefine>
{
    public string name;
    public int maxHp;
    public int atk;
    public int def;
    public string passives;
    public int aiId;
    public int dropGroupId;
    public int dropCount;
    public string appear;
    public string prefabName;
    // 音效位置
    public string attackSound;
    public string hitSound;
    public string showSound;
    public override MonsterDataDefine ParseData()
    {
        var d = new MonsterDataDefine();
        d.id = id;
        d.name = name;
        d.maxHp = maxHp;
        d.atk = atk;
        d.def = def;
        if (!string.IsNullOrEmpty(passives))
            d.passives = passives.Split(',').ToList().ConvertAll(a => int.Parse(a));
        d.aiId = aiId;
        d.dropGroupId = dropGroupId;
        d.dropCount = dropCount;
        if (!string.IsNullOrEmpty(appear))
            d.appearEnums = appear.Split(',').ToList().ConvertAll(a => (MonsterAppearEnum)int.Parse(a));
        return d;
    }
}
