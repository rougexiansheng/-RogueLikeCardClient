using Cysharp.Threading.Tasks;
using NanoidDotNet;
using Newtonsoft.Json;
using SDKProtocol;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using JsonObject = Newtonsoft.Json.Linq.JObject;

public partial class FakeServer
{
    public UniTask Login(string deviceId)
    {
        Debug.Log($"{TAG} SaveFakeServerData: 開始登入處理");

        // Login的時候Server會從DB撈取該玩家的存檔資料
        // 這邊模擬從fakeServerData這個結構撈取

        // 檢查是否要創建帳號
        if (string.IsNullOrEmpty(fakeServerData.player.profile.deviceId))
            CreateAccount(deviceId);

        // 清除戰鬥cache
        ResetBattleCache();

        // 建立指令為init的ClientSave
        var clientSave = new ClientSave(Cmd.init);

        // 製作客戶端玩家存檔資料
        clientSave.Add(ConvertSaveToPlayerData());

        // 製作客戶端道具存檔資料
        clientSave.Add(ConvertSaveToBattleItemData());

        // 製作客戶端的職業存檔資料
        clientSave.Add(ConvertSaveToProfessionData());

        // 製作客戶端的地城緩存存檔資料
        clientSave.Add(ConvertSaveToBattleDungeonData());

        // 製作客戶端的戰鬥角色緩存存檔資料
        clientSave.Add(ConvertSaveToBattleHeroAttrData());

        // 製作客戶端的戰鬥角色緩存存檔資料
        //clientSave.Add(ConvertSaveToBattleSkillData());

        // 將存檔資料轉換成jsonObject
        var clientSaveJsonObject = clientSave.ToJsonObject();
        var clientSaveResult = new List<JsonObject>();
        clientSaveResult.Add(clientSaveJsonObject);

        Debug.Log($"{TAG} SaveFakeServerData: 結束登入處理");

        return EndProtocol(true, clientSaveResult);
    }

    private void ResetBattleCache()
    {
        fakeServerData.player.dungeonCache.Clear();
        fakeServerData.player.actorCache.Clear();
        //SetItemCount(NetworkSaveBattleItemContainer.COIN, 0);
        SaveFakeServerData();
    }

    private void CreateAccount(string deviceId)
    {
        if (!string.IsNullOrEmpty(fakeServerData.player.profile.uid))
        {
            Debug.Log($"{TAG} CreateAccount: 已經創建過帳號 {deviceId} => {fakeServerData.player.profile.uid}");
            return;
        }

        // step1. 設定登入的帳號為裝置ID
        fakeServerData.player.profile.deviceId = deviceId;

        // step2. 帳號預設玩家名稱
        fakeServerData.player.profile.userName = (!string.IsNullOrEmpty(fakeServerData.player.profile.userName)) ?
            fakeServerData.player.profile.userName : "預設玩家";

        // step3. 帳號唯一碼
        fakeServerData.player.profile.uid = (!string.IsNullOrEmpty(fakeServerData.player.profile.uid)) ?
            fakeServerData.player.profile.uid : Nanoid.Generate(size: 16);

        // step4. 初始化職業
        DoAddCharacter((int)ActorProfessionEnum.Witch);
        DoAddCharacter((int)ActorProfessionEnum.Paladin);
        DoAddCharacter((int)ActorProfessionEnum.Fencer);
        DoAddCharacter((int)ActorProfessionEnum.Assassin);

        // step4. 撰寫其他創帳號內容

        // ...

        SaveFakeServerData();

        Debug.Log($"{TAG} CreateAccount: 創建帳號完成!! {deviceId} => {fakeServerData.player.profile.uid}");
    }
}