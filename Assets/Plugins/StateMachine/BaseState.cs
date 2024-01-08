using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T">指定控制的BaseControl</typeparam>
public abstract class BaseState<T,U> where T : BaseControl<T,U> where U : Enum
{
    T controller;
    /// <summary>
    /// 取得controller
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetController()
    {
        return controller;
    }
    /// <summary>
    /// 配置controller
    /// BaseControl SetTransistion 會自動執行
    /// </summary>
    /// <param name="target"></param>
    public void SetController(T controller)
    {
        this.controller = controller;
    }
    /// <summary>
    /// 開始
    /// </summary>
    /// <param name="preCondition">上個狀態的觸發</param>
    /// <returns></returns>
    public abstract UniTask Start();
    /// <summary>
    /// 會持續執行
    /// </summary>
    public abstract void Update();
    /// <summary>
    /// 結束
    /// </summary>
    /// <param name="nextCondition">下個狀態的觸發</param>
    /// <returns></returns>
    public abstract UniTask End();
    /// <summary>
    /// 強制停止
    /// </summary>
    public abstract void OnAbort();
}
