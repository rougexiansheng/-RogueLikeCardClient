using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 給JsonUtility使用的List序列化類別
/// </summary>
[Serializable]
public class Serialization<T>
{
    [SerializeField]
    List<T> list;
    public List<T> ToList() { return list; }
    public Serialization(List<T> list)
    {
        this.list = list;
    }
}
