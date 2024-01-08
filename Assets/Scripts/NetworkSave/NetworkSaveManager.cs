using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Antlr3.Runtime;
using Zenject;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public class NetworkSaveManager : IInitializable
{
    [Inject]
    DiContainer m_diContainer;

    private Dictionary<string, INetworkSaveContainer> m_networkSaveContainers = new Dictionary<string, INetworkSaveContainer>();
    private Dictionary<Type, string> m_networkSaveContainerNames = new Dictionary<Type, string>();

    public void Initialize()
    {
        Debug.Log("NetworkSaveManager.Initialize");

        // 指定域名尋找所有INetworkSaveContainer
        var types = GetTypes(typeof(NetworkSaveContainerBase));

        // 初始化所有儲存容器
        foreach (var type in types)
        {
            Debug.Log($"NetworkSaveManager: {type.Name}");
            var instance = Activator.CreateInstance(type) as INetworkSaveContainer;
            m_networkSaveContainers.Add(instance.GetName(), instance);
            m_networkSaveContainerNames.Add(instance.GetType(), instance.GetName());
            m_diContainer.Inject(instance);
        }
    }

    public void OnServerResponse(JsonObject jsonObject)
    {
        if (jsonObject == null)
        {
            Debug.LogError("OnServerResponse jsonObject is null.");
            return;
        }
        //Debug.Log($"OnServerResponse: {jsonObject.ToString(Formatting.None)}");
        if (jsonObject.ContainsKey("init"))
        {
            var contents = jsonObject["init"].ToObject<JsonObject>();
            foreach (var content in contents)
            {
                var containerKey = content.Key;
                if (!m_networkSaveContainers.ContainsKey(containerKey))
                    continue;
                var containerValues = content.Value.ToList();
                var datas = ConvertToDatas(containerKey, containerValues);
                if (datas.Count == 1) { m_networkSaveContainers[containerKey].OnInit(datas[0]); }
                else if (datas.Count > 1) { m_networkSaveContainers[containerKey].OnInit(datas); }
                else { Debug.LogError($"OnServerResponse init no data(s) of container {containerKey}."); }
            }
        }
        else if (jsonObject.ContainsKey("update"))
        {
            var contents = jsonObject["update"].ToObject<JsonObject>();
            foreach (var content in contents)
            {
                var containerKey = content.Key;
                if (!m_networkSaveContainers.ContainsKey(containerKey))
                    continue;
                var containerValues = content.Value.ToList();
                var datas = ConvertToDatas(containerKey, containerValues);
                if (datas.Count == 1) { m_networkSaveContainers[containerKey].OnUpdate(datas[0]); }
                else if (datas.Count > 1) { m_networkSaveContainers[containerKey].OnUpdate(datas); }
                else { Debug.LogError($"OnServerResponse update no data(s) of container {containerKey}."); }
            }
        }
        else if (jsonObject.ContainsKey("remove"))
        {
            var contents = jsonObject["remove"].ToObject<JsonObject>();
            foreach (var content in contents)
            {
                var containerKey = content.Key;
                if (!m_networkSaveContainers.ContainsKey(containerKey))
                    continue;
                var removeKeys = content.Value.Values<string>().ToList();
                if (removeKeys.Count == 1) { m_networkSaveContainers[containerKey].OnRemove(removeKeys[0]); }
                else if (removeKeys.Count > 1) { m_networkSaveContainers[containerKey].OnRemove(removeKeys); }
                else { Debug.LogError($"OnServerResponse remove no key(s) of container {containerKey}."); }
            }
        }
        else
        {
            throw new Exception($"OnServerResponse exception, unknown command. {jsonObject.ToString(Formatting.None)}");
        }
    }

    private List<INetworkSaveData> ConvertToDatas(string key, List<JToken> values)
    {
        var datas = new List<INetworkSaveData>();
        var containerValues = values;
        foreach (var containerValue in containerValues)
        {
            var json = containerValue.ToString(Formatting.None);
            var type = m_networkSaveContainers[key].GetDataType();
            var obj = Activator.CreateInstance(type);
            var data = obj as INetworkSaveData;
            data.Load(json);
            datas.Add(data);
            //Debug.Log($"容器名: {key}, 存檔內容: {data.ToJson()}, type: {type.Name}");
        }
        return datas;
    }

    private Type[] GetTypes(Type interfaceType)
    {
        var result = new List<Type>();
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var assemblyType in assembly.GetTypes())
        {
            if (!interfaceType.IsAssignableFrom(assemblyType))
                continue;
            if (assemblyType == interfaceType)
                continue;
            result.Add(assemblyType);
        }
        return result.ToArray();
    }

    private Type[] GetTypes(string nameSpace, Type interfaceType)
    {
        var result = new List<Type>();
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var assemblyType in assembly.GetTypes())
        {
            if (!interfaceType.IsAssignableFrom(assemblyType))
                continue;
            if (assemblyType == interfaceType)
                continue;
            if (!string.Equals(assemblyType.Namespace, nameSpace, StringComparison.Ordinal))
                continue;
            result.Add(assemblyType);
        }
        return result.ToArray();
    }

    /// <summary>
    /// 將整個玩家存檔轉成JsonObject
    /// </summary>
    public JsonObject ToJsonObject()
    {
        var jsonObject = new JsonObject();
        foreach (var item in m_networkSaveContainers)
        {
            jsonObject.Add(item.Key, item.Value.GetDatas());
        }
        return jsonObject;
    }

    /// <summary>
    /// 將整個玩家存檔轉成Json字串
    /// </summary>
    public string ToJson()
    {
        return ToJsonObject().ToString(Formatting.None);
    }

    /// <summary>
    /// 獲取儲存存檔的容器
    /// </summary>
    public T GetContainer<T>() where T : INetworkSaveContainer
    {
        var type = typeof(T);
        if (!m_networkSaveContainerNames.ContainsKey(type))
        {
            Debug.LogError($"OnServerResponse get container failed, can't found container name by type {type.Name}.");
            return default;
        }

        var containerName = m_networkSaveContainerNames[type];
        if (!m_networkSaveContainers.ContainsKey(containerName))
        {
            Debug.LogError($"OnServerResponse get container failed, can't found container by name {containerName}.");
            return default;
        }

        return (T)m_networkSaveContainers[containerName];
    }
}