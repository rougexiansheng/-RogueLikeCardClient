using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKProtocol
{
    /// <summary>
    /// 與服務器溝通
    /// </summary>
    public interface IProtocolBridge
    {
        #region 登入
        UniTask Login(string token);
        UniTask Logout();
        UniTask<bool> Regist();
        #endregion
        UniTask<PlayerServerData> BattleStart(SelectDungeonData selectDungeonData, SelectProfessionData selectProfessionData);
        UniTask<NextDungeonData> BattleLevelRewards(List<string> logs);

        UniTask<int> BattleNextLevel(int dungeonId);

        UniTask<bool> BattleGetMonsterAcquistion();
        UniTask<bool> BattleGetEventAcquistion();

        UniTask<bool> BattleGiveUp();

        #region ServerActorData 相關
        UniTask<ActorCacheData> BattleGainHeroAttr(int hp);
        UniTask<bool> BattleSelectItem(int itemId);
        #endregion

        #region  Skill 相關
        UniTask<bool> BattleReplaceSkill(int index, ActorSkill skill);
        UniTask<bool> UnlockSkill(ActorProfessionEnum professionEnum, int skillID);
        #endregion

        UniTask<int> BattleLosesItem(int itemId, int count);
        UniTask<bool> BattleGainItem(int itemId, int count);

        UniTask BattleLevelEnd(bool isWin, List<ActorSkill> skills);
        UniTask<BattleResultData> BattleResult(BattleEndData battleResultData);

        void SaveFakeServerData();
        void LoadFakeServerData();
        void DeleteFakeServerData();
    }
}


