using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public abstract class BaseControl<T, U> where T : BaseControl<T, U> where U : Enum
{
    /// <summary>
    /// 中斷使用
    /// </summary>
    CancellationTokenSource cancelSource = new CancellationTokenSource();
    Queue<U> triggerQueue = new Queue<U>();
    /// <summary>
    /// 是否正在切換中
    /// </summary>
    public bool IsSwitching { get; private set; } = false;
    /// <summary>
    /// log訊息
    /// </summary>
    public abstract Action<string> OnMessage {get;}
    /// <summary>
    /// 當前狀態
    /// </summary>
    BaseState<T, U> current;

    /// <summary>
    /// 下個狀態
    /// </summary>
    BaseState<T, U> next;

    /// <summary>
    /// 上個狀態
    /// </summary>
    BaseState<T, U> previous;

    /// <summary>
    /// 當前狀態(顯示在Unity Inspector)
    /// </summary>
    [SerializeField] U currentStateEnum;
    /// <summary>
    /// 當前狀態
    /// </summary>
    public U CurrentStateEnum { get { return currentStateEnum; } }
    /// <summary>
    /// 前一個的狀態(顯示在Unity Inspector)
    /// </summary>
    [SerializeField] U preStateEnum;
    /// <summary>
    /// 前一個的狀態
    /// </summary>
    public U PreStateEnum { get { return preStateEnum; } }
    /// <summary>
    /// 終止
    /// </summary>
    public void Stop()
    {
        if (IsSwitching)
        {
            cancelSource.Cancel();
            cancelSource = new CancellationTokenSource();
        }
        else
        {
            previous?.OnAbort();
            current?.OnAbort();
            next?.OnAbort();
        }
        previous = null;
        next = null;
        current = null;
    }

    Dictionary<U, BaseState<T, U>> transitions = new Dictionary<U, BaseState<T, U>>();
    /// <summary>
    /// 設置狀態
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="state"></param>
    protected void SetTransition(U condition, BaseState<T, U> state)
    {
        if (transitions.ContainsKey(condition) == false)
        {
            transitions.Add(condition, state);
            state.SetController((T)this);
        }
        else
        {
            var stateName = state == null ? "null" : state.GetType().Name;
            OnMessage?.Invoke("重複配置 : " + condition.ToString() + " className : " + stateName);
        }
    }
    /// <summary>
    /// 出發切換狀態
    /// </summary>
    /// <param name="condition"></param>
    async public void Trigger(U condition)
    {
        // 當在切換未完成時 連續執行 會暫時放在此容器內
        if (IsSwitching)
        {
            triggerQueue.Enqueue(condition);
            OnMessage?.Invoke($"新增: {condition} 切換中....");
            return;
        }
        IsSwitching = true;
        if (transitions.TryGetValue(condition, out BaseState<T, U> state))
        {
            if (current == state)
            {
                OnMessage?.Invoke("Same State : " + state.GetType().ToString());
            }
            else
            {
                preStateEnum = currentStateEnum;
                currentStateEnum = condition;
                previous = current;
                current = null;
                next = state;
                OnMessage?.Invoke("Start Change State : " + condition.ToString());
                if (previous != null)
                {
                    OnMessage?.Invoke(previous.GetType().ToString() + " End-Begin");
                    await previous.End().AttachExternalCancellation(cancelSource.Token);
                    OnMessage?.Invoke(previous.GetType().ToString() + " End-Finish");
                }
                next = null;
                OnMessage?.Invoke(state.GetType().ToString() + " Start-Begin");
                await state.Start().AttachExternalCancellation(cancelSource.Token);
                OnMessage?.Invoke(state.GetType().ToString() + " Start-Finish");
                current = state;
            }
        }
        else
        {
            var nextName = state == null ? "null" : state.GetType().Name;
            var currentName = current == null ? "null" : current.GetType().Name;
            var msg = string.Format("Current: {0} Next: {1} Trigger Failed", currentName, nextName);
            OnMessage?.Invoke(msg);
        }
        IsSwitching = false;
        // 切換完成後 如果容器內有東西 表示有連續切換 並在執行切換狀態
        if (triggerQueue.Count > 0) Trigger(triggerQueue.Dequeue());
    }
    /// <summary>
    /// 驅動狀態
    /// </summary>
    public void Update()
    {
        current?.Update();
    }
}
