using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

public class DataTableManager
{
    [Inject]
    PreloadManager preloadManager;

    #region 資料結構
    Dictionary<int, SkillDataDefine> skillDefineDic = new Dictionary<int, SkillDataDefine>();
    Dictionary<int, List<SkillDataDefine>> skillGroupDefineDic = new Dictionary<int, List<SkillDataDefine>>();

    Dictionary<int, PassiveDataDefine> passiveDefineDic = new Dictionary<int, PassiveDataDefine>();
    Dictionary<int, List<PassiveDataDefine>> passiveGroupDefineDic = new Dictionary<int, List<PassiveDataDefine>>();

    Dictionary<int, MonsterDataDefine> monsterDataDic = new Dictionary<int, MonsterDataDefine>();

    Dictionary<int, MonsterAIDefine> monsterAIDic = new Dictionary<int, MonsterAIDefine>();
    Dictionary<int, MonsterGroupDataDefine> monsterGroupDic = new Dictionary<int, MonsterGroupDataDefine>();

    Dictionary<int, DungeonListDefine> dungeonListDic = new Dictionary<int, DungeonListDefine>();

    Dictionary<int, DungeonDataDefine> dungeonDataDic = new Dictionary<int, DungeonDataDefine>();
    Dictionary<int, List<List<DungeonDataDefine>>> dungeonGroupDataDic = new Dictionary<int, List<List<DungeonDataDefine>>>();

    Dictionary<int, SceneDataDefine> SceneDataDic = new Dictionary<int, SceneDataDefine>();

    Dictionary<int, List<DropItemOriginDataDefine>> dropGroupDataDic = new Dictionary<int, List<DropItemOriginDataDefine>>();

    Dictionary<int, ItemDataDefine> itemDataDic = new Dictionary<int, ItemDataDefine>();

    Dictionary<int, ItemEffectDataDefine> itemEffectDataDic = new Dictionary<int, ItemEffectDataDefine>();

    Dictionary<int, ProfessionDataDefine> professionDataDic = new Dictionary<int, ProfessionDataDefine>();

    Dictionary<int, ProfessionSkinDataDefine> professionSkinDataDefineDic = new Dictionary<int, ProfessionSkinDataDefine>();
    #endregion

    int m_intKey;
    int m_intKey2;

    #region 讀取資料表
    /// <summary> 讀取資料</summary>
    public async UniTask LoadMainTable()
    {
        /// <summary>
        /// 共用讀取資料有 2 種使用方法 (先在 xxxOriginDefine 繼承 OriginDefineBase<xxxDefine>，並複寫 ParseData)
        /// 
        /// GetTableDictionary - 正常Define (setOriginl:選填, 可儲存 Origin 資料)
        /// GetGroupTableDictionary - 包含 Define 跟 GroupDefine (setOriginl:選填, 可儲存 Origin 資料)
        /// </summary>
        var ls = new UniTask[]
        {
            //LoadServerDefineData<SkillOriginDefine>("1Di3DQWobQzLu2jT_TbhfJa6eYnVTwI4b6H4BSZArYnU", "SkillData", // 技能
            LoadServerDefineData<SkillOriginDefine>("1rweUPAs7W2Y7msmbONWBn0nFeV-cJXJwzK60BTIEa-s", "SkillData", // 技能
            (dalaList)=>{ GetGroupTableDictionary(dalaList, out skillDefineDic, out skillGroupDefineDic, preloadManager.SetSkillOriginlDefineDic); }),
            //LoadServerDefineData<PassiveOriginDefine>("1vDOa2zw-x0Ji0JBXpeQ2rFK79p8oTaH6mVPUX-e_TrA", "PassiveData", //被動
            LoadServerDefineData<PassiveOriginDefine>("1trU-VVq7Q4EPC2gWdq_Z4a98Rh92CJIQbQjPSBgibqI", "PassiveData", //被動
            (dalaList)=>{ GetGroupTableDictionary(dalaList, out passiveDefineDic, out passiveGroupDefineDic, preloadManager.SetPassiveOriginlDefineDic); }),
            //LoadServerDefineData<MonsterOriginDefine>("1qJBfcU93LaKrBC7hq9Cs2xFvAzj_snDV1SCeUUrcJro", "MonsterData", // 怪物
            LoadServerDefineData<MonsterOriginDefine>("1sgi7O_O2bM0JORT5NCl_ZHYd7Hb9q20RHakm3ZMC70A", "MonsterData", // 怪物
            (dalaList)=>{ GetTableDictionary(dalaList, out monsterDataDic, preloadManager.SetMonsterOriginlDefineDic); }),
            //LoadServerDefineData<MonsterAIOriginDefine>("1rnzNNZCFeE9QEzK3aCkXcUAR8tG7aX8UAynAEU0jEkI", "MonsterAIData", // 怪物AI
            LoadServerDefineData<MonsterAIOriginDefine>("1678qhd7-rfO9zKwvhxjCv2_tuBC1_Do58sFcizE15dI", "MonsterAIData", // 怪物AI
            (dalaList)=>{ GetTableDictionary(dalaList, out monsterAIDic); }),
            //LoadServerDefineData<MonsterGroupOriginDataDefine>("1eK3BILCBe9BnpJvgnUsbMX0GheBhbaWYWnzvT2WX-g4", "MonsterGroupData", //怪物隨機群組
            LoadServerDefineData<MonsterGroupOriginDataDefine>("1ESzW8srCPQLgMcD98CjW7a3UW-KmGWrzV_E3RUHKE0s", "MonsterGroupData", //怪物隨機群組
            (dalaList)=>{ GetTableDictionary(dalaList, out monsterGroupDic); }),
            //LoadServerDefineData<DungeonOriginListDefine>("1WpB2z9Ez5I-Xsxw4-pEeU8o1ohu6E1y38Imf-fy9sQ4","DungeonList", //地圖表
            LoadServerDefineData<DungeonOriginListDefine>("1ud_ciPiBx5C_xQiqqRLey_ffL8JEacW__oZdkM3gLwQ","DungeonList", //地圖表
            (dalaList)=>{ GetTableDictionary(dalaList, out dungeonListDic, preloadManager.SetDungeonOriginlListDefineDic); }),
            //LoadServerDefineData<DungeonOriginDataDefine>("1K3Py-BGP8sg4_8hA6yEd-2UsYb927Fpn_UO_FtjHWPU","DungeonData", //地圖
            LoadServerDefineData<DungeonOriginDataDefine>("1q0MTpxB3Qn200bc0NNAGu5DBcqkUH0akh7FdNMKWQLA","DungeonData", //地圖
            (dalaList)=>{
                GetTableDictionary(dalaList, out dungeonDataDic, preloadManager.SetDungeonOriginlDefineDic);
                GetDungeonGroupDictionary(dungeonDataDic, out dungeonGroupDataDic);
            }),
            //LoadServerDefineData<SceneDataOriginDefine>("1g4B1Iqq3b4gAP4Q3FY-wCvqmbFyZDZzKJTYEeGd8kpQ","SceneData", //場景定義
            LoadServerDefineData<SceneDataOriginDefine>("1NjnDxJU4EEQayHi0D21cVmD30D-M1KGg9okT6vh_Rig","SceneData", //場景定義
            (dalaList)=>{ GetTableDictionary(dalaList, out SceneDataDic, preloadManager.SetSceneOriginlDefineDic); }),
            //LoadServerDefineData<DropItemOriginDataDefine>("16OMY5UL1NJ7xdwIm5r2tvqQdvZpbACPSjQrOhKJcigM","DropData", //掉落群組
            LoadServerDefineData<DropItemOriginDataDefine>("1FNhzgY55JQX8y0OlzG0F6GdnxcfvwKfg-rDb9D6givk","DropData", //掉落群組
            (dalaList)=>{ GetDropItemGroupDictionary(dalaList, out dropGroupDataDic); }),
            //LoadServerDefineData<ItemOriginDataDefine>("1H9rbyvisQ9PqvRm172FiLLjpwpCEalSpCxtfIIQpe8U","ItemData", //道具
            LoadServerDefineData<ItemOriginDataDefine>("1_055jVAta4JzQ87FeuiIeNm4Bt3lwCgsNeURA9KRMiA","ItemData", //道具
            (dalaList)=>{ GetTableDictionary(dalaList, out itemDataDic, preloadManager.SetItemOriginlDefineDic); }),
            //LoadServerDefineData<ProfessionOriginDataDefine>("1DXe6Plj5yMqw0j-tG6V0zxf9sAp_PUWUf1Me1IX33CA","ProfessionData", //角色
			LoadServerDefineData<ProfessionOriginDataDefine>("1Gd6G86V2F1-YgNHjBnAigA91sWtvMlHdaYZF6c0W_ic","ProfessionData", //角色
			(dalaList)=>{ GetTableDictionary(dalaList, out professionDataDic, preloadManager.SetProfessionOriginlDefineDic); }),

            LoadServerDefineData<ItemEffectOriginalDefine>("1Z3shgeSoPK_JHzbXpXSub7bOTXiTs4pmnjdJ2lOJbBY","ItemEffectData", //道具效果
            (dalaList)=>{ GetTableDictionary(dalaList, out itemEffectDataDic, preloadManager.SetItemEffectOriginlDefineDic); }),
            //LoadServerDefineData<ProfessionSkinOriginDataDefine>("1BPND46fFC1J2nAkDEmhlc98i5lJjgJki1LeBP3tn5Ds","ProfessionSkinData", //角色造型
            LoadServerDefineData<ProfessionSkinOriginDataDefine>("1B7rjMjwZCchDXa_IWr6NObG_-LzqjSrx1EwzGKjsJ5c","ProfessionSkinData", //角色造型
            (dalaList)=>{ GetTableDictionary(dalaList, out professionSkinDataDefineDic, preloadManager.SetProfessionSkinOriginlDefineDic); }),
        };

        await UniTask.WhenAll(ls);
    }

    /// <summary>
    /// 開發測試使用
    /// 技能/被動 重新載入
    /// </summary>
    public async UniTask Reload()
    {
        var ls = new UniTask[]
        {
            LoadServerDefineData<SkillOriginDefine>("1Di3DQWobQzLu2jT_TbhfJa6eYnVTwI4b6H4BSZArYnU", "SkillData", // 技能
            (dalaList)=>{ GetGroupTableDictionary(dalaList, out skillDefineDic, out skillGroupDefineDic, preloadManager.SetSkillOriginlDefineDic); }),
            LoadServerDefineData<PassiveOriginDefine>("1vDOa2zw-x0Ji0JBXpeQ2rFK79p8oTaH6mVPUX-e_TrA", "PassiveData", //被動
            (dalaList)=>{ GetGroupTableDictionary(dalaList, out passiveDefineDic, out passiveGroupDefineDic, preloadManager.SetPassiveOriginlDefineDic); }),
        };

        await UniTask.WhenAll(ls);
    }
    #endregion

    #region Parse Google 資料相關

    /// <summary>
    /// 解析從 GoogleSheet Paser 出來的 Origin 資料，並回存 Dictionary 結構
    /// </summary>
    /// <typeparam name="TOrigin"> 資料的原始 OriginDefine class </typeparam>
    /// <typeparam name="TTable"> 資料解析後的 Define class </typeparam>
    /// <param name="source"> OriginDefine List </param>
    /// <param name="originlDic"> 存放 OriginDefine 的 Dic </param>
    /// <param name="defineDic"> 存放 Define 的 Dic </param>
    public void GetTableDictionary<TOrigin, TTable>(List<TOrigin> source,
        out Dictionary<int, TTable> defineDic, Action<Dictionary<int, TOrigin>> setOriginl = null)
        where TTable : class where TOrigin : OriginDefineBase<TTable>
    {
        if (source == null)
        {
            setOriginl?.Invoke(new Dictionary<int, TOrigin>());
            defineDic = new Dictionary<int, TTable>();
            return;
        }

        int iMax = source.Count;
        var originlDic = new Dictionary<int, TOrigin>(iMax); // ok
        defineDic = new Dictionary<int, TTable>(iMax); // ok
        for (int i = 0; i < iMax; i++)
        {
            m_intKey = source[i].id;
            originlDic[m_intKey] = source[i];
            defineDic[m_intKey] = source[i].ParseData();
        }
        setOriginl?.Invoke(originlDic);
    }

    /// <summary>
    /// 解析從 GoogleSheet Paser 出來的 Origin 資料，並回存 Dictionary 結構，且包含 Group Dictionary
    /// </summary>
    /// <typeparam name="TOrigin"> 資料的原始 OriginDefine class </typeparam>
    /// <typeparam name="TTable"> 資料解析後的 Define class </typeparam>
    /// <param name="source"> OriginDefine List </param>
    /// <param name="originlDic"> 存放 OriginDefine 的 Dic </param>
    /// <param name="defineDic"> 存放 Define 的 Dic </param>
    /// <param name="groupDefineDic">存放 Group Define 的 Dic </param>
    public void GetGroupTableDictionary<TOrigin, TTable>(List<TOrigin> source,
        out Dictionary<int, TTable> defineDic,
        out Dictionary<int, List<TTable>> groupDefineDic,
        Action<Dictionary<int, TOrigin>> setOriginl = null)
        where TTable : class where TOrigin : OriginDefineBase<TTable>
    {
        if (source == null || source.Count == 0)
        {
            setOriginl(new Dictionary<int, TOrigin>());
            defineDic = new Dictionary<int, TTable>();
            groupDefineDic = new Dictionary<int, List<TTable>>();
            setOriginl?.Invoke(new Dictionary<int, TOrigin>());
            return;
        }

        int iMax = source.Count;
        var originlDic = new Dictionary<int, TOrigin>(iMax); // ok
        defineDic = new Dictionary<int, TTable>(iMax); // ok
        groupDefineDic = new Dictionary<int, List<TTable>>(); // ok
        for (int i = 0; i < iMax; i++)
        {
            m_intKey = source[i].id;
            m_intKey2 = source[i].GetGroupId();
            originlDic[m_intKey] = source[i];
            var table = source[i].ParseData();
            defineDic[m_intKey] = table;

            if (m_intKey2 != default)
            {
                if (groupDefineDic.ContainsKey(m_intKey2))
                {
                    groupDefineDic[m_intKey2].Add(table);
                }
                else
                {
                    var list = new List<TTable>();
                    list.Add(table);
                    groupDefineDic.Add(m_intKey2, list);
                }
            }
        }

        setOriginl?.Invoke(originlDic);
    }

    async UniTask LoadServerDefineData<T>(string excelKey, string fileName, Action<List<T>> saveAction) where T : new()
    {
        var d = await GetFromGoogleSheetDatas(excelKey, fileName);
        Debug.Log($"{typeof(T)} start paser");
        var ls = SetDatas<T>(d);
        saveAction(ls);
        Debug.Log($"{typeof(T)} end paser");
    }

    /// <summary>
    /// 從google讀取表單
    /// 本地端測試使用
    /// </summary>
    /// <param name="excelKey"></param>
    /// <param name="fileName"></param>
    /// <param name="forceUpdate"></param>
    /// <returns></returns>
    async UniTask<List<List<string>>> GetFromGoogleSheetDatas(string excelKey, string fileName, bool forceUpdate = false)
    {
        forceUpdate = true;
        var path = Application.persistentDataPath + "/" + fileName + ".txt";
        if (forceUpdate && File.Exists(path))
        {
            File.Delete(path);
        }
        if (!File.Exists(path))
        {
            //var request = UnityWebRequest.Get("https://script.google.com/macros/s/AKfycbyVXK0Vk38rNsPVjpMjdkclqJvZUAjTN6bWNGaA9JaaQLO6QfimGICiRNczSpUD5OD8Kg/exec?name=" + excelKey);
            var request = UnityWebRequest.Get("https://script.google.com/macros/s/AKfycbyJHstbNthV2XVEL2Y_L7Hry55zT2zDJbQyHzrvq2CZlYAM_vTtcDmkJWQD1NHbgh-MKg/exec?name=" + excelKey);

            var r = await request.SendWebRequest();
            var file = File.OpenWrite(path);
            file.Write(r.downloadHandler.data);
            file.Close();
        }
        var str = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<List<List<string>>>(str);
    }

    /// <summary>
    /// 將資料轉換
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="datas"></param>
    /// <returns></returns>
    public List<T> SetDatas<T>(List<List<string>> datas) where T : new()
    {
        var ls = new List<T>();
        var t = typeof(T);
        var typeLs = new List<Func<string, object>>();
        var nameLs = new List<string>();
        var idIsNull = false;
        var maxFieldName = 999;
        for (int i = 1; i < datas.Count; i++)
        {
            var data = datas[i];
            var d = new T();
            for (int j = 0; j < data.Count; j++)
            {
                if (string.IsNullOrEmpty(data[0]))
                {
                    idIsNull = true;
                    break;
                }
                else if (i == 1 && !string.IsNullOrEmpty(data[j])) //解析字串Func
                {
                    typeLs.Add(StringGetConvert(data[j]));
                }
                else if (i == 2) //對應類別內的Field名稱
                {
                    if (!string.IsNullOrEmpty(data[j])) nameLs.Add(data[j]);
                    else
                    {
                        maxFieldName = j;
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(data[j]))
                {
                    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                    if (j >= nameLs.Count || j > maxFieldName)
                    {
                        continue;
                    }
                    var fieldInfo = t.GetField(nameLs[j], flags);
                    if (fieldInfo != null)
                    {
                        try
                        {
                            var v = typeLs[j](data[j]);
                            fieldInfo.SetValue(d, v);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(data[0] + " " + nameLs[j] + "Field set value :" + data[j] + " failed");
                            Debug.LogWarning(e);
                            continue;
                            throw e;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(t.ToString() + " dont have Field : " + nameLs[j]);
                        continue;
                    }
                }
            }
            if (idIsNull) break;
            if (i != 1 && i != 2 && !string.IsNullOrEmpty(data[0])) ls.Add(d);
        }
        return ls;
    }

    Func<string, object> StringGetConvert(string typeString)
    {
        switch (typeString)
        {
            case "int32":
                return s =>
                {
                    if (string.IsNullOrEmpty(s)) return 0;
                    return Convert.ToInt32(s);
                };
            case "bool":
                return s =>
                {
                    if (s == "T" || s == "1")
                        return true;
                    else if (s == "F" || s == "0" || string.IsNullOrEmpty(s))
                        return false;
                    else
                        return Convert.ToBoolean(s);
                };
            case "float32":
                return s => (float)Convert.ToDouble(s);
            case "string":
                return Convert.ToString;
            default:
                Debug.LogWarning($"{typeString} cant convert");
                return Convert.ToString;

        }
    }
    #endregion

    #region 自己另外定義的結構儲存
    private void GetDungeonGroupDictionary(Dictionary<int, DungeonDataDefine> sourecs,
        out Dictionary<int, List<List<DungeonDataDefine>>> groupDefineDic)
    {
        groupDefineDic = new Dictionary<int, List<List<DungeonDataDefine>>>();
        foreach (var data in sourecs.Values)
        {
            m_intKey = data.groupId;
            m_intKey2 = data.mapLayer;

            if (groupDefineDic.ContainsKey(m_intKey))
            {
                if (groupDefineDic[m_intKey].Count >= m_intKey2)
                {
                    groupDefineDic[m_intKey][m_intKey2 - 1].Add(data);
                }
                else
                {
                    groupDefineDic[m_intKey].Add(new List<DungeonDataDefine>() { data });
                }
            }
            else
            {
                var list = new List<List<DungeonDataDefine>>()
                {
                    new List<DungeonDataDefine>(){ data }
                };
                groupDefineDic[m_intKey] = list;
            }
        }
    }

    private void GetDropItemGroupDictionary(List<DropItemOriginDataDefine> sourecs,
        out Dictionary<int, List<DropItemOriginDataDefine>> groupDefineDic)
    {
        groupDefineDic = new Dictionary<int, List<DropItemOriginDataDefine>>();
        for (int i = 0; i < sourecs.Count; i++)
        {
            var data = sourecs[i];
            m_intKey = data.groupId;
            if (groupDefineDic.ContainsKey(m_intKey))
            {
                groupDefineDic[m_intKey].Add(data);
            }
            else
            {
                groupDefineDic[m_intKey] = new List<DropItemOriginDataDefine>() { data };
            }
        }
    }

    #endregion

    #region 取用資料
    public SkillDataDefine GetSkillDefine(int skillId)
    {
        var have = skillDefineDic.TryGetValue(skillId, out SkillDataDefine skillDataDefine);
        if (have)
            return skillDataDefine;
        else
            Debug.LogWarning($"Skill id:{skillId} Dont have Define");
        return new SkillDataDefine();
    }

    public List<SkillDataDefine> GetSkillGroupDefine(int groupId)
    {
        var have = skillGroupDefineDic.TryGetValue(groupId, out var groupDefineDic);
        if (have)
            return groupDefineDic;
        else
            Debug.LogWarning($"Skill id:{groupId} Dont have Define");
        return new List<SkillDataDefine>();
    }

    public Dictionary<int, PassiveDataDefine> GetPassiveDefineAll()
    {
        return passiveDefineDic;
    }

    public PassiveDataDefine GetPassiveDefine(int id)
    {
        var have = passiveDefineDic.TryGetValue(id, out PassiveDataDefine data);
        if (!have)
            Debug.LogWarning($"GetPassiveDefine Dont have id:{id}");
        return data;
    }

    public MonsterDataDefine GetMonsterDefine(int id)
    {
        var have = monsterDataDic.TryGetValue(id, out MonsterDataDefine data);
        if (!have)
            Debug.LogWarning($"GetMonsterDefine Dont have id:{id}");
        return data;
    }

    public MonsterAIDefine GetMonsterAIDefine(int id)
    {
        var have = monsterAIDic.TryGetValue(id, out MonsterAIDefine data);
        if (!have)
            Debug.LogWarning($"GetMonsterAIDefine Dont have id:{id}");
        return data;
    }

    public MonsterGroupDataDefine GetMonsterGroupDefine(int groupId)
    {
        var have = monsterGroupDic.TryGetValue(groupId, out MonsterGroupDataDefine data);
        if (!have)
            Debug.LogWarning($"GetMonsterGroupDefine Dont have id:{groupId}");
        return data;
    }

    public DungeonListDefine GetDungeonListDefine(int groupId)
    {
        var have = dungeonListDic.TryGetValue(groupId, out DungeonListDefine datas);
        if (have)
            return datas;
        else
            Debug.LogWarning($"GetDungeonListDefine id:{groupId} Dont have Define");
        return null;
    }

    public List<List<DungeonDataDefine>> GetDungeonDataDefines(int groupId)
    {
        var have = dungeonGroupDataDic.TryGetValue(groupId, out List<List<DungeonDataDefine>> datas);
        if (have)
            return datas;
        else
            Debug.LogWarning($"GetDungeonDataDefines id:{groupId} Dont have Define");
        return null;
    }

    public DungeonDataDefine GetDungeonDataDefine(int id)
    {
        var have = dungeonDataDic.TryGetValue(id, out DungeonDataDefine data);
        if (have)
            return data;
        else
            Debug.LogWarning($"GetDungeonDataDefine id:{id} Dont have Define");
        return null;
    }

    public SceneDataDefine GetSceneDataDefine(int id)
    {
        var have = SceneDataDic.TryGetValue(id, out SceneDataDefine data);
        if (have)
            return data;
        else
            Debug.LogWarning($"GetSceneDataDefine id:{id} Dont have Define");
        return null;
    }

    public List<DropItemOriginDataDefine> GetDropGroupDataDefine(int groupId)
    {
        var have = dropGroupDataDic.TryGetValue(groupId, out List<DropItemOriginDataDefine> data);
        if (!have)
            Debug.LogWarning($"GetDropGroupDataDefine Dont have groupId:{groupId}");
        return data;
    }

    public ItemDataDefine GetItemDataDefine(int id)
    {
        var have = itemDataDic.TryGetValue(id, out ItemDataDefine data);
        if (!have)
            Debug.LogWarning($"GetItemDataDefine Dont have id:{id}");
        return data;
    }



    public ItemEffectDataDefine GetItemEffectDataDefine(int id)
    {
        var have = itemEffectDataDic.TryGetValue(id, out ItemEffectDataDefine data);
        if (!have)
            Debug.LogWarning($"GetItemEffectDataDefine Dont have id:{id}");
        return data;
    }

    public ProfessionDataDefine GetProfessionDataDefine(ActorProfessionEnum e)
    {
        var have = professionDataDic.TryGetValue((int)e, out ProfessionDataDefine data);
        if (!have)
            Debug.LogWarning($"GetProfessionDataDefine Dont have ActorProfessionEnum:{e} id:{(int)e}");
        return data;
    }

    public ProfessionSkinDataDefine GetProfessionSkinDataDefine(int id)
    {
        var have = professionSkinDataDefineDic.TryGetValue((int)id, out ProfessionSkinDataDefine data);
        if (!have)
            Debug.LogWarning($"GetProfessionSkinDataDefine Dont have id:{id}");
        return data;
    }
    #endregion
}
