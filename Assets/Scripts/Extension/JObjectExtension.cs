using Newtonsoft.Json.Linq;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public static class JObjectExtension
{
    /// <summary>
    /// 擴充JObject添加其他屬性時的Add方法
    /// </summary>
    public static void Add(this JsonObject jsonObject, string key, object value)
    {
        if (jsonObject == null) throw new System.NullReferenceException();
        jsonObject.Add(new JProperty(key, JToken.FromObject(value)));
    }
}
