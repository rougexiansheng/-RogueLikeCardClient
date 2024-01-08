using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace SDKProtocol
{
    public struct ProtoRes<T>
    {
        public T resData;
        /// <summary>0 沒問題</summary>
        public int code;
        /// <summary>錯誤訊息</summary>
        public string msg;
    }

    public struct ProtoReq<T>
    {
        public T reqData;
        public string token;
    }

    /// <summary>
    /// 關卡資訊 怪物 掉落物 
    /// </summary>
    public class DungeonLevelData
    {
        public int dungeonId;
        public int mapNodeEnum;
        public MapNodeEnum nodeEnum { get { return (MapNodeEnum)mapNodeEnum; } }
        /// <summary>EX: [1,10,3]</summary>
        public List<int> monsterPosAndId;
        /// <summary>關卡獲得的道具(打完怪物、Chest、Store、Antique)，順序跟怪物一樣，最後一項為關卡獎勵</summary>
        public List<List<ItemData>> acquisitionItems = new List<List<ItemData>>();
        public List<int> passives = new List<int>();
        public int threeSelectCoin;
        public List<int> threeSelectSkillIds;
    }

    public class DungeonCacheData
    {
        public int dungeonGroupId;
        public int dungeonDataIndex;
        /// <summary>
        /// 玩家現在的位置
        /// </summary>
        public int fightDungeonId;
        public List<List<DungeonLevelData>> lastCache = new List<List<DungeonLevelData>>();
        public bool isDone = true;
        public bool isWin;
        public ActorProfessionEnum professionEnum = ActorProfessionEnum.Witch;

        /// <summary>
        /// 清除此Cache
        /// </summary>
        public void Clear()
        {
            dungeonGroupId = 0;
            dungeonDataIndex = 0;
            fightDungeonId = 0;
            lastCache.Clear();
            isDone = true;
            isWin = false;
            professionEnum = ActorProfessionEnum.Witch;
        }
    }

    public class ProfessionDetailCacheData
    {
        public int id;
        public int skillId;
        public int passives;
    }

    public class NextDungeonData
    {
        public List<DungeonLevelData> dungeonDatas;
        public bool isDone;
        /// <summary>下一關的id</summary>
        public int nextDungeonId;
    }

    public class SelectDungeonData
    {
        public int dungeonGroupId;
        public int professionId;
        public int currectHp;
    }

    public class BattleEndData
    {
        public int stageProgress;
        public int spendTime;
        public ActorProfessionEnum profession;
        public string playerName;
        public bool isWin;
    }

    public class BattleResultData
    {
        public DungeonRankingData ranking = new DungeonRankingData();

        // TODO 這裡可以添加其他結算接口回傳的資料
        // ...
    }


    public class SkillGroup
    {
        public List<int> Skills = new List<int>();
        public string Name;
    }


    public class ProfileData
    {
        public string userName;
        public string uid;
        public string deviceId;
        /// <summary>寶石</summary>
        public int gem;
        /// <summary>道具(含金幣、碎片等)</summary>
        public Dictionary<int, int> items = new Dictionary<int, int>();
        /// <summary>解鎖的腳色id</summary>
        public List<int> unlockProfessionIDs = new List<int>();
        /// <summary>解鎖的時裝id</summary>
        public List<int> unlockSkinIds = new List<int>();
        /// <summary>透過職業ID存玩家腳色資料</summary>
        public Dictionary<ActorProfessionEnum, PlayerProfessionData> playerProfessionDataDic = new Dictionary<ActorProfessionEnum, PlayerProfessionData>();

        //商品購買次數
        //當前關卡 後三關
    }

    public class PlayerProfessionData
    {
        /// <summary>解鎖的腳色外觀id</summary>
        public List<int> UnlockCharacterSkinIds;
        /// <summary>技能解鎖</summary>
        public List<int> UnlockSkillIDs = new List<int>();

        /// <summary>自己設定的技能組</summary>
        public List<SkillGroup> SkillGroups = new List<SkillGroup>();

        /// <summary>狀態解鎖(暫代)</summary>
        public List<int> unlockPassiveGourpIds;
        /// <summary>上一次玩家選擇的腳色資料</summary>
        public SelectProfessionData DefalutProfessionDatas = new SelectProfessionData();
    }
    public class SelectProfessionData
    {
        /// <summary> 選擇腳色外觀id </summary>
        public int CharacterSkinId;
        /// <summary> 選擇的腳色技能組Index </summary>
        public int SkillGroupsIndex;
    }

    public class PlayerServerData
    {
        public ProfileData profile = new ProfileData();
        public DungeonCacheData dungeonCache = new DungeonCacheData();
        public ActorCacheData actorCache = new ActorCacheData();
        public List<ProfessionDetailCacheData> professionDetailCache = new List<ProfessionDetailCacheData>();
        public List<string> logs = new List<string>();
    }

    public class RankingServerData
    {
        public DungeonRankingData dungeonData = new DungeonRankingData();
    }

    public class DungeonRankingData
    {
        public List<DungeonRankingDataItem> fullRankings = new List<DungeonRankingDataItem>();
        public DungeonRankingDataItem lastRanking = new DungeonRankingDataItem();
    }

    public class DungeonRankingDataItem
    {
        public int ranking;
        public int stageProgress;
        public int spendTime;
        public ActorProfessionEnum profession;
        public string professionName;
        public string playerName;
    }

    /// <summary>
    /// 用於 server 端儲存玩家戰鬥資訊
    /// </summary>
    public class ActorCacheData
    {
        /// <summary>目前血量</summary>
        public int currentHp;
        /// <summary>戰鬥中獲得的被動(商店/寶箱/遺跡)</summary>
        public List<int> passives = new List<int>();
        /// <summary>戰鬥中獲得的技能(商店/寶箱/遺跡)</summary>
        public List<int> skillIds = new List<int>();
        /// <summary>戰鬥中獲得遺物或時裝(不能重複獲得物品)</summary>
        public List<int> itemList = new List<int>();
        /// <summary>客戶端的技能排序資料</summary>
        public Dictionary<int, ActorSkill> actorSkills = new Dictionary<int, ActorSkill>();

        /// <summary>
        /// 清除此Cache
        /// </summary>
        public void Clear()
        {
            currentHp = 0;
            passives.Clear();
            skillIds.Clear();
            itemList.Clear();
            actorSkills.Clear();
        }
    }




    public struct ItemData
    {
        public int id;
        public int count;
    }

    public class RespondError : Exception
    {

    }
}
