using Newtonsoft.Json;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    private enum Cmd
    {
        init = 1,
        update = 2,
        remove = 3
    }

    private enum SaveContainer
    {
        player = 1,
        item = 2,
        profession = 3,
        rank = 4,
        battleItem = 5,
        battleDungeon = 6,
        battleHeroAttr = 7,
        battleSkill = 8
    }

    private class ClientSave
    {
        public class Data
        {
            public SaveContainer targetSaveContainer;
            public List<INetworkSaveData> datas;
        }

        private Cmd m_cmd;
        private List<Data> m_saveDatas;

        public ClientSave(Cmd cmd)
        {
            m_cmd = cmd;
            m_saveDatas = new List<Data>();
        }

        public void Add(Data saveData)
        {
            m_saveDatas.Add(saveData);
        }

        public JsonObject ToJsonObject()
        {
            var containerJsonObject = new JsonObject(); // 目標container的JsonObject
            foreach (var saveData in m_saveDatas)
            {
                containerJsonObject.Add(saveData.targetSaveContainer.ToString(), saveData.datas);
            }
            var responseJsonObject = new JsonObject(); // 最終回傳的JsonObject
            responseJsonObject.Add(m_cmd.ToString(), containerJsonObject);
            Debug.Log($"ClientSave.ToJsonObject: {responseJsonObject.ToString(Formatting.None)}");
            return responseJsonObject;
        }
    }
}