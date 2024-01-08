using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    /// <summary>
    /// 戰鬥中直接獲得物品不用判斷金錢
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public async UniTask<bool> BattleGainItem(int itemId, int count)
    {
        // 添加道具
        var item = dataTableManager.GetItemDataDefine(itemId);

        var result = await AddGetItem(item, count);

        if (result)
        {
            // 回寫SaveContainer的資料
            var clientSave = new ClientSave(Cmd.update);

            //根據不同的類型實作
            switch (item.itemType)
            {
                case ItemTpyeEnum.Antique:
                    clientSave.Add(ConvertSaveToBattleHeroAttrData());
                    break;
                default:
                    clientSave.Add(ConvertSaveToBattleItemData(itemId));
                    break;
            }


            var clientSaveResult = new List<JsonObject>() { clientSave.ToJsonObject() };

            return await EndProtocol(result, clientSaveResult);
        }

        return await EndProtocol(false);

    }




}