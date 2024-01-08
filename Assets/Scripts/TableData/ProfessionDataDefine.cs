using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfessionDataDefine
{
    public int id;
    public string name;
    public List<int> ultGroupIds = new List<int>();
    public ProfessionLockEnum lockType;
    public int baseHp;
    public int lockArg;
    public int skillRange;
    public int colorCount;
    public List<int> prepareSkills = new List<int>();
    public List<int> baseSkills = new List<int>();
    public List<List<int>> selectSkills = new List<List<int>>();

    public SpineCharacterCtrl spineCharacter;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip ultimateSound;
    public AudioClip damageSound;
    public AudioClip clickSound;
}

public class ProfessionOriginDataDefine : OriginDefineBase<ProfessionDataDefine>
{
    public string name;
    public string prefabName;
    public string ultGroupIds;
    public int lockType;
    public int lockArg;
    public int skillRange;
    public int colorCount;
    public string prepareSkills;
    public string baseSkills;
    public string selectSkill1;
    public string selectSkill2;
    public string selectSkill3;
    public string selectSkill4;
    public int baseHp;

    public string attackSound;
    public string hitSound;
    public string ultimateSound;
    public string damageSound;
    public string clickSound;
    public override ProfessionDataDefine ParseData()
    {
        var d = new ProfessionDataDefine();
        d.id = id;
        d.name = name;
        d.baseHp = baseHp;
        if (!string.IsNullOrEmpty(ultGroupIds))
            d.ultGroupIds = ultGroupIds.Split(',').ToList().ConvertAll(s => int.Parse(s));
        d.lockType = (ProfessionLockEnum)lockType;
        d.lockArg = lockArg;
        d.skillRange = skillRange;
        d.colorCount = colorCount;
        if (!string.IsNullOrEmpty(prepareSkills))
            d.prepareSkills = prepareSkills.Split(',').ToList().ConvertAll(s => int.Parse(s));
        if (!string.IsNullOrEmpty(baseSkills))
            d.baseSkills = baseSkills.Split(',').ToList().ConvertAll(s => int.Parse(s));

        if (!string.IsNullOrEmpty(selectSkill1))
            d.selectSkills.Add(selectSkill1.Split(',').ToList().ConvertAll(s => int.Parse(s)));
        if (!string.IsNullOrEmpty(selectSkill2))
            d.selectSkills.Add(selectSkill2.Split(',').ToList().ConvertAll(s => int.Parse(s)));
        if (!string.IsNullOrEmpty(selectSkill3))
            d.selectSkills.Add(selectSkill3.Split(',').ToList().ConvertAll(s => int.Parse(s)));
        if (!string.IsNullOrEmpty(selectSkill4))
            d.selectSkills.Add(selectSkill4.Split(',').ToList().ConvertAll(s => int.Parse(s)));
        return d;
    }
}