using System.Collections.Generic;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public interface INetworkSaveData
{
    void Load(string json);
    void Load(JsonObject jsonObject);
    string GetKey();
    JsonObject ToJsonObject();
    string ToJson();
}