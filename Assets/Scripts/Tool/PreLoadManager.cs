using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class PreloadManager
{
    #region const ���|�]�w
    const string DYNAMIC_ASSETS_PATH = "Assets/DynamicAssets/";
    const string SPINE_PREFABS = DYNAMIC_ASSETS_PATH + "Spine/Prefabs/";
    const string PARTICLE_PATH = DYNAMIC_ASSETS_PATH + "VFX/";
    const string SOUND_PATH = DYNAMIC_ASSETS_PATH + "Sound/";
    const string ICON_PATH = DYNAMIC_ASSETS_PATH + "UI/Icon/";
    const string SCENE_OBJ_PATH = DYNAMIC_ASSETS_PATH + "MAP/";
    const string SCENE_NAME_PATH = DYNAMIC_ASSETS_PATH + "UI/SceneNames/";
    const string SPINE_MONSTER_PATH = SPINE_PREFABS + "Monster/";
    const string CHARACTOR_PATH = SPINE_PREFABS + "Character/";
    const string CHARACTORCG_PATH = SPINE_PREFABS + "CG/";
    #endregion

    [Inject]
    DataTableManager dataTableManager;
    [Inject]
    AssetManager assetManager;

    #region ��Ƶ��c
    Dictionary<int, SkillOriginDefine> skillOriginlDefineDic = new Dictionary<int, SkillOriginDefine>();
    Dictionary<int, PassiveOriginDefine> passiveOriginlDefineDic = new Dictionary<int, PassiveOriginDefine>();
    Dictionary<int, MonsterOriginDefine> monsterOrginDic = new Dictionary<int, MonsterOriginDefine>();
    Dictionary<int, DungeonOriginListDefine> dungeonOriginListDic = new Dictionary<int, DungeonOriginListDefine>();
    Dictionary<int, DungeonOriginDataDefine> dungeonOriginDataDic = new Dictionary<int, DungeonOriginDataDefine>();
    Dictionary<int, SceneDataOriginDefine> sceneOriginDataDic = new Dictionary<int, SceneDataOriginDefine>();
    Dictionary<int, ItemOriginDataDefine> itemOriginDataDic = new Dictionary<int, ItemOriginDataDefine>();
    Dictionary<int, ItemEffectOriginalDefine> itemEffectOriginDataDic = new Dictionary<int, ItemEffectOriginalDefine>();
    Dictionary<int, ProfessionOriginDataDefine> professionOriginDataDic = new Dictionary<int, ProfessionOriginDataDefine>();
    Dictionary<int, ProfessionSkinOriginDataDefine> professionSkinOriginDataDefineDic = new Dictionary<int, ProfessionSkinOriginDataDefine>();
    #endregion

    #region ��l�Ƹ��
    public void SetSkillOriginlDefineDic(Dictionary<int, SkillOriginDefine> source)
    {
        skillOriginlDefineDic = source;
    }

    public void SetPassiveOriginlDefineDic(Dictionary<int, PassiveOriginDefine> source)
    {
        passiveOriginlDefineDic = source;
    }

    public void SetMonsterOriginlDefineDic(Dictionary<int, MonsterOriginDefine> source)
    {
        monsterOrginDic = source;
    }

    public void SetDungeonOriginlListDefineDic(Dictionary<int, DungeonOriginListDefine> source)
    {
        dungeonOriginListDic = source;
    }

    public void SetDungeonOriginlDefineDic(Dictionary<int, DungeonOriginDataDefine> source)
    {
        dungeonOriginDataDic = source;
    }

    public void SetSceneOriginlDefineDic(Dictionary<int, SceneDataOriginDefine> source)
    {
        sceneOriginDataDic = source;
    }

    public void SetItemOriginlDefineDic(Dictionary<int, ItemOriginDataDefine> source)
    {
        itemOriginDataDic = source;
    }

    public void SetItemEffectOriginlDefineDic(Dictionary<int, ItemEffectOriginalDefine> source)
    {
        itemEffectOriginDataDic = source;
    }


    public void SetProfessionOriginlDefineDic(Dictionary<int, ProfessionOriginDataDefine> source)
    {
        professionOriginDataDic = source;
    }

    public void SetProfessionSkinOriginlDefineDic(Dictionary<int, ProfessionSkinOriginDataDefine> source)
    {
        professionSkinOriginDataDefineDic = source;
    }
    #endregion

    #region �ɮ׹w��
    #region skill
    public async UniTask PreloadSkillPrefab(ActorProfessionEnum professionEnum)
    {
        var tLs = new List<UniTask>();
        foreach (var item in skillOriginlDefineDic)
        {
            var define = dataTableManager.GetSkillDefine(item.Key);
            //if (define.profession == professionEnum ||
            if (true ||
                define.profession == ActorProfessionEnum.Monster ||
                define.profession == ActorProfessionEnum.NotShow ||
                define.profession == ActorProfessionEnum.Common)
            {
                tLs.Add(PreloadSkillPrefabById(define.id));
            }
        }
        await UniTask.WhenAll(tLs);
    }

    async UniTask PreloadSkillPrefabById(int skillId)
    {
        if (!skillOriginlDefineDic.TryGetValue(skillId, out SkillOriginDefine skillOrginrDefine))
            return;
        var define = dataTableManager.GetSkillDefine(skillId);
        // �S��
        if (!string.IsNullOrEmpty(skillOrginrDefine.ability1HitEffect))
        {
            var obj = await assetManager.AcyncLoadAsset<GameObject>($"{PARTICLE_PATH}{skillOrginrDefine.ability1HitEffect}.prefab");
            if (obj && define.skillAbilities.Count >= 1)
                define.skillAbilities[0].hitEffect = obj.GetComponent<ParticleItem>();
        }

        if (!string.IsNullOrEmpty(skillOrginrDefine.ability2HitEffect))
        {
            var obj = await assetManager.AcyncLoadAsset<GameObject>($"{PARTICLE_PATH}{skillOrginrDefine.ability2HitEffect}.prefab");
            if (obj && define.skillAbilities.Count >= 2)
                define.skillAbilities[1].hitEffect = obj.GetComponent<ParticleItem>();
        }

        if (!string.IsNullOrEmpty(skillOrginrDefine.ability3HitEffect))
        {
            var obj = await assetManager.AcyncLoadAsset<GameObject>($"{PARTICLE_PATH}{skillOrginrDefine.ability3HitEffect}.prefab");
            if (obj && define.skillAbilities.Count >= 3)
                define.skillAbilities[2].hitEffect = obj.GetComponent<ParticleItem>();
        }


        if (!string.IsNullOrEmpty(skillOrginrDefine.costEffectName))
        {
            var obj = await assetManager.AcyncLoadAsset<GameObject>($"{PARTICLE_PATH}{skillOrginrDefine.costEffectName}.prefab");
            if (obj) define.costEffect = obj.GetComponent<ParticleItem>();
        }
        // ����
        if (!string.IsNullOrEmpty(skillOrginrDefine.castEffectSound))
            define.costEffectSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{skillOrginrDefine.castEffectSound}");
        if (!string.IsNullOrEmpty(skillOrginrDefine.ability1HitEffectSound) && define.skillAbilities.Count >= 1)
            define.skillAbilities[0].hitEffectSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{skillOrginrDefine.ability1HitEffectSound}");
        if (!string.IsNullOrEmpty(skillOrginrDefine.ability2HitEffectSound) && define.skillAbilities.Count >= 2)
            define.skillAbilities[1].hitEffectSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{skillOrginrDefine.ability2HitEffectSound}");
        if (!string.IsNullOrEmpty(skillOrginrDefine.ability3HitEffectSound) && define.skillAbilities.Count >= 3)
            define.skillAbilities[2].hitEffectSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{skillOrginrDefine.ability3HitEffectSound}");

        if (!string.IsNullOrEmpty(skillOrginrDefine.icon))
        {
            define.icon = await assetManager.AcyncLoadAsset<Sprite>($"{ICON_PATH}{skillOrginrDefine.icon}.png[{skillOrginrDefine.icon}].sprite");
        }
    }
    #endregion

    #region passive
    public UniTask PreloadPassiveAll()
    {
        var passiveOriginlDefineDic = dataTableManager.GetPassiveDefineAll();
        var ls = new List<UniTask>();
        foreach (var item in passiveOriginlDefineDic)
        {
            ls.Add(PreloadPassive(item.Key));
        }
        return UniTask.WhenAll(ls);
    }

    public async UniTask PreloadPassive(int passiveId)
    {
        if (passiveOriginlDefineDic.TryGetValue(passiveId, out PassiveOriginDefine passiveOrigin))
        {
            var define = dataTableManager.GetPassiveDefine(passiveId);
            if (!string.IsNullOrEmpty(passiveOrigin.effectName))
            {
                var obj = await assetManager.AcyncLoadAsset<GameObject>($"{PARTICLE_PATH}{passiveOrigin.effectName}.prefab");
                if (obj) define.effect = obj.GetComponent<ParticleItem>();

            }
            if (!string.IsNullOrEmpty(passiveOrigin.effectSound))
                define.effectSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{passiveOrigin.effectSound}");
            if (!string.IsNullOrEmpty(passiveOrigin.icon))
                define.icon = await assetManager.AcyncLoadAsset<Sprite>($"{ICON_PATH}{passiveOrigin.icon}.png[{passiveOrigin.icon}].sprite");
        }
        else
        {
            Debug.LogError($"passiveId:{passiveId} dont have passiveOriginlData");
        }
    }
    #endregion

    #region mosnter
    public async UniTask PreloadMonsterSpineAndParticle(int monsterId)
    {
        if (!monsterOrginDic.TryGetValue(monsterId, out MonsterOriginDefine monsteOrginrDefine))
            return;
        var define = dataTableManager.GetMonsterDefine(monsterId);
        var obj = await assetManager.AcyncLoadAsset<GameObject>($"{SPINE_MONSTER_PATH}{monsteOrginrDefine.prefabName}.prefab");
        if (!string.IsNullOrEmpty(monsteOrginrDefine.attackSound))
        {
            define.attackSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{monsteOrginrDefine.attackSound}");
        }
        if (!string.IsNullOrEmpty(monsteOrginrDefine.hitSound))
        {
            define.hitSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{monsteOrginrDefine.hitSound}");
        }
        if (!string.IsNullOrEmpty(monsteOrginrDefine.showSound))
        {
            define.showSound = await assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{monsteOrginrDefine.showSound}");
        }

        define.spineObj = obj;
    }
    public async UniTask PreloadAllMonster()
    {
        var ls = new List<UniTask>();
        foreach (var item in monsterOrginDic)
        {
            ls.Add(PreloadMonsterSpineAndParticle(item.Key));
        }
        await UniTask.WhenAll(ls);
    }
    #endregion

    #region Environment
    public async UniTask PreLoadSceneObject(int groupId)
    {
        var groups = dataTableManager.GetDungeonDataDefines(groupId);
        var ls = new List<DungeonDataDefine>();
        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            for (int j = 0; j < g.Count; j++)
            {
                ls.Add(g[j]);
            }
        }
        var sceneIds = ls.GroupBy(g => g.sceneId).Select(y => y.Key).ToList();

        for (int i = 0; i < sceneIds.Count; i++)
        {
            var tLs = new List<UniTask<GameObject>>();
            var sceneId = sceneIds[i];
            var sceneDefine = dataTableManager.GetSceneDataDefine(sceneId);
            if (!sceneOriginDataDic.TryGetValue(sceneId, out var originDefine)) // �קK null�A���b�@�U
            {
                Debug.LogWarning($"GetSceneOriginDataDefine id:{sceneId} Dont have Define");
                originDefine = new SceneDataOriginDefine();
            }

            for (int j = 0; j < sceneDefine.sceneNames.Count; j++)
            {
                var t = assetManager.AcyncLoadAsset<GameObject>($"{SCENE_OBJ_PATH}{originDefine.prefix}/{sceneDefine.sceneNames[j]}.prefab");
                tLs.Add(t);
            }
            if (!string.IsNullOrEmpty(originDefine.textureName))
            {
                sceneDefine.sceneNameSprite = await assetManager.AcyncLoadAsset<Sprite>($"{SCENE_NAME_PATH}{originDefine.textureName}.png[{originDefine.textureName}].sprite");
            }
            sceneDefine.sceneObj = (await UniTask.WhenAll(tLs)).ToList();
            sceneDefine.sceneBoss = await assetManager.AcyncLoadAsset<GameObject>($"{SCENE_OBJ_PATH}{originDefine.prefix}/{originDefine.bossSceneName}.prefab");
            sceneDefine.sceneSetting = await assetManager.AcyncLoadAsset<MAY.SceneSettings>($"{SCENE_OBJ_PATH}{originDefine.prefix}/{originDefine.prefix}_SceneSettings.asset");
        }
    }
    #endregion

    #region Profession
    public async UniTask PrelaodProfessionData(ActorProfessionEnum professionEnum)
    {
        if (professionOriginDataDic.TryGetValue((int)professionEnum, out ProfessionOriginDataDefine originDataDefine))
        {
            var data = dataTableManager.GetProfessionDataDefine(professionEnum);
            var ls = await UniTask.WhenAll(
                assetManager.AcyncLoadAsset<GameObject>($"{CHARACTOR_PATH}{originDataDefine.prefabName}.prefab"),
                assetManager.AcyncLoadAsset<GameObject>($"{CHARACTORCG_PATH}{originDataDefine.cgSpine1}.prefab"),
                assetManager.AcyncLoadAsset<GameObject>($"{CHARACTORCG_PATH}{originDataDefine.cgSpine2}.prefab"),
                assetManager.AcyncLoadAsset<GameObject>($"{CHARACTORCG_PATH}{originDataDefine.cgSpine3}.prefab"),
                assetManager.AcyncLoadAsset<GameObject>($"{CHARACTORCG_PATH}{originDataDefine.cgSpine4}.prefab"),
                assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{originDataDefine.attackSound}"),
                assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{originDataDefine.hitSound}"),
                assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{originDataDefine.clickSound}"),
                assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{originDataDefine.damageSound}"),
                assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{originDataDefine.ultimateSound}"),
                assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{originDataDefine.cgSound1}"),
                assetManager.AcyncLoadAsset<AudioClip>($"{SOUND_PATH}{originDataDefine.cgSound2}")
                );
            var obj = ls.Item1;
            data.spineCharacter = obj.GetComponent<SpineCharacterCtrl>();
            data.cgSpine1 = ls.Item2.GetComponentInChildren<Spine.Unity.SkeletonGraphic>();
            data.cgSpine2 = ls.Item3.GetComponentInChildren<Spine.Unity.SkeletonGraphic>();
            data.cgSpine3 = ls.Item4.GetComponentInChildren<Spine.Unity.SkeletonGraphic>();
            data.cgSpine4 = ls.Item5.GetComponentInChildren<Spine.Unity.SkeletonGraphic>();
            data.attackSound = ls.Item6;
            data.hitSound = ls.Item7;
            data.clickSound = ls.Item8;
            data.damageSound = ls.Item9;
            data.ultimateSound = ls.Item10;
            data.cgSound1 = ls.Item11;
            data.cgSound2 = ls.Item12;
        }
    }
    #endregion

    #region ItemData
    public async UniTask PreloadItemData(int itemId)
    {
        if (itemOriginDataDic.TryGetValue(itemId, out ItemOriginDataDefine itemDataOrigin))
        {
            var define = dataTableManager.GetItemDataDefine(itemId);
            if (!string.IsNullOrEmpty(itemDataOrigin.icon))
                define.icon = await assetManager.AcyncLoadAsset<Sprite>($"{ICON_PATH}{itemDataOrigin.icon}.png[{itemDataOrigin.icon}].sprite");
        }
        else
        {
            Debug.LogError($"passiveId:{itemId} dont have passiveOriginlData");
        }
    }

    public List<UniTask> PreLoadAllItemData()
    {
        var tLs = new List<UniTask>();
        foreach (var itemOrigin in itemOriginDataDic)
        {
            tLs.Add(PreloadItemData(itemOrigin.Key));
        }
        return tLs;
    }
    #endregion
    #endregion
}
