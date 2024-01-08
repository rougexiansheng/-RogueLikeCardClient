using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Reflection;

public static class RxEventBus
{
    class RxBusPassenger
    {
        public int instanceID;
        public Subject<object> subject;
    }
    static Dictionary<string, List<RxBusPassenger>> subjectDic = new Dictionary<string, List<RxBusPassenger>>();

    /// <summary>
    /// 建議使用此方法Enum為自訂義
    /// </summary>
    /// <param name="codeEnum"></param>
    /// <param name="data"></param>
    public static void Send(Enum codeEnum, object data = null)
    {
        var code = codeEnum.EnumToString();
        Send(code, data);
    }

    /// <summary>
    /// 不建議直接使用(Debug使用)
    /// </summary>
    /// <param name="code"></param>
    /// <param name="data"></param>
    public static void Send(string code, object data = null)
    {
        if (subjectDic.TryGetValue(code, out List<RxBusPassenger> passengers))
        {
            for (int i = 0; i < passengers.Count; i++)
            {
                passengers[i].subject.OnNext(data);
            }
        }
    }
    public static void Register(Enum codeEnum, Action action, object obj)
    {
        Register<object>(codeEnum, (o) => action(), obj);
    }
    public static void Register<T>(Enum codeEnum, Action<T> action, object obj)
    {
        var code = codeEnum.EnumToString();
        var instanceID = obj.GetHashCode();
        List<RxBusPassenger> passengers = null;
        if (!subjectDic.TryGetValue(code, out passengers))
        {
            passengers = new List<RxBusPassenger>();
            subjectDic.Add(code, passengers);
        }

        var passenger = passengers.Find(p => p.instanceID == instanceID);
        if (passenger == null)
        {
            passenger = new RxBusPassenger();
            passenger.instanceID = instanceID;
            passenger.subject = new Subject<object>();
            passenger.subject.OnErrorRetry((Exception e) => Debug.LogError("Message : " + e.Message + "\nStack : " + e.StackTrace));
            passengers.Add(passenger);
        }
        passenger.subject.Subscribe(o =>
        action((T)o)
        );
    }
    /// <summary>
    /// 取消註冊
    /// </summary>
    /// <param name="obj"></param>
    public static void UnRegister(object obj)
    {
        foreach (var item in subjectDic)
        {
            UnRegister(obj, item.Key);
        }
    }
    /// <summary>
    /// 取消註冊
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="codeEnum"></param>
    public static void UnRegister(object obj, Enum codeEnum)
    {
        var code = codeEnum.EnumToString();
        UnRegister(obj, code);
    }
    /// <summary>
    /// 取消註冊(內部)
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="code"></param>
    static void UnRegister(object obj, string code)
    {
        var instanceID = obj.GetHashCode();
        if (subjectDic.TryGetValue(code, out List<RxBusPassenger> passengers))
        {
            var passenger = passengers.Find(p => p.instanceID == instanceID);
            if (passenger != null)
            {
                passenger.subject.Dispose();
                passengers.Remove(passenger);
            }
        }
    }
}
public class EventBusEnum
{
    public enum UIBattleEnum
    {
        /// <summary>
        /// 參數int index 點擊技能按鈕
        /// </summary>
        OnClickSkillItem,
        /// <summary>參數int 技能ID 按下技能按鈕(用於顯示對應消耗的能量)</summary>
        OnPressedSkillItem,
        /// <summary>放開技能按鈕(用於顯示對應消耗的能量)</summary>
        OnPressUpSkillItem,
        /// <summary>結束玩家回合</summary>
        OnClickPlayerRoundEnd,
        /// <summary>點擊復活</summary>
        OnClickRevive,
        /// <summary>點擊結算</summary>
        OnClickReview,
        /// <summary>長按怪物</summary>
        OnLongPressMonster,
        /// <summary>查看技能詳情</summary>
        OnClickSkillInfo,
        /// <summary>長按玩家</summary>
        OnLongPressPlayer,
    }

    public enum PlayerDataEnum
    {
        /// <summary>更新選擇怪物目標 BattleActor.MonsterPositionEnum 接受參數</summary>
        UpdateSelectTarget,
    }
    /// <summary>
    /// 畫面滑動控制距離移動有超過寬版20% 才會觸發
    /// </summary>
    public enum ScreenControlEnum
    {
        Began,
        Right,
        Left,
        End
    }
}
