using System;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;


public abstract class NetworkSaveContainerBase : INetworkSaveContainer
{
    private string m_name;
    private Type m_dataType;


    public string GetName()
    {
        if (string.IsNullOrEmpty(m_name))
        {
            m_name = this.GetType().Name.Replace("NetworkSave", "").Replace("Container", "");
            m_name = m_name[0].ToString().ToLower() + m_name.Substring(1);
            Debug.Log($"{this.GetType().Name} GetName -> {m_name}");
        }
        return m_name;
    }

    public Type GetDataType()
    {
        if (m_dataType == null)
        {
            var typeName = $"{this.GetType().Name.Replace("Container", "")}Data";
            Debug.Log($"{this.GetType().Name} GetDataType -> {typeName}");
            m_dataType = Type.GetType(typeName);
        }
        return m_dataType;
    }

    public abstract List<INetworkSaveData> GetDatas();

    public abstract void OnInit(INetworkSaveData data);

    public abstract void OnInit(List<INetworkSaveData> datas);

    public abstract void OnRemove(string id);

    public abstract void OnRemove(List<string> ids);

    public abstract void OnUpdate(INetworkSaveData data);

    public abstract void OnUpdate(List<INetworkSaveData> datas);
}