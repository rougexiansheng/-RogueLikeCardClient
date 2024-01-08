using Newtonsoft.Json;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public abstract class NetworkSaveDataBase : INetworkSaveData
{
    public void Load(string json)
    {
        JsonConvert.PopulateObject(json, this);
    }
    public void Load(JsonObject jsonObject)
    {
        Load(jsonObject.ToString());
    }

    public virtual string GetKey()
    {
        return "0"; // 預設Key為0
    }

    public JsonObject ToJsonObject()
    {
        var json = JsonConvert.SerializeObject(this);
        return JsonObject.Parse(json);
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}