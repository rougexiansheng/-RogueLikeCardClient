using System;
using System.Collections.Generic;

public interface INetworkSaveContainer
{
    string GetName();
    Type GetDataType();
    List<INetworkSaveData> GetDatas();

    void OnInit(INetworkSaveData data);
    void OnInit(List<INetworkSaveData> datas);

    void OnUpdate(INetworkSaveData data);
    void OnUpdate(List<INetworkSaveData> datas);

    void OnRemove(string id);
    void OnRemove(List<string> ids);
}