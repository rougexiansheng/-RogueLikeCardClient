using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public static class UtilityHelper
{
    /// <summary>
    /// 所有子物件設定layer
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="layer"></param>
    public static void SetChildLayer(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = layer;
            var t = child.GetComponentInChildren<Transform>();
            if (t != null) SetChildLayer(child, layer);
        }
    }

    /// <summary>
    /// 判斷點擊位置是否有在範圍內
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    public static bool IsInRect(RectTransform trans)
    {
        var canvas = trans.GetComponentInParent<Canvas>();
        if (canvas == null) return false;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 localMousePosition = trans.InverseTransformPoint(Touch.activeFingers[0].screenPosition);
            return trans.rect.Contains(localMousePosition);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(trans, Touch.activeFingers[0].screenPosition, canvas.worldCamera, out var v2);
            return trans.rect.Contains(v2);
        }
    }

    /// <summary>
    /// Log類型
    /// </summary>
    public enum BattleLogEnum
    {
        None,
        Passive,
        Skill,
        PassiveEffect,
        PassiveSound,
        SkillEffect,
        SkillSound,
        OnDamage,
        OnHeal,
        Performance,
    }

    public static string EnumToString(this Enum e)
    {
        return $"{e.GetType().Name}_{e}";
    }

    public static void BattleLog(string str, BattleLogEnum battleLog = BattleLogEnum.None)
    {
        var logStr = $"[{battleLog}]";
        var colorHex = ColorUtility.ToHtmlStringRGBA(Color.white);
        switch (battleLog)
        {
            // 紫色 被動
            case BattleLogEnum.Passive:
                colorHex = "#6691FF";
                break;
            case BattleLogEnum.PassiveEffect:
                colorHex = "#A7A9E2";
                break;
            case BattleLogEnum.PassiveSound:
                colorHex = "#F5F5F5";
                break;
            // 藍色 技能
            case BattleLogEnum.Skill:
                colorHex = "#569900";
                break;
            case BattleLogEnum.SkillEffect:
                colorHex = "#A7A9E2";
                break;
            case BattleLogEnum.SkillSound:
                colorHex = "#F5F5F5";
                break;
            // 紅 傷害
            case BattleLogEnum.OnDamage:
                colorHex = "#F13D3D";
                break;
            // 綠 治療
            case BattleLogEnum.OnHeal:
                colorHex = "#46C763";
                break;
            case BattleLogEnum.Performance:
                colorHex = "#7CA2A3";
                break;
            case BattleLogEnum.None:
            default:
                break;
        }
        var strFormat = $"<color={colorHex}>{logStr}|{str}</color>";
        Debug.Log(strFormat);
    }
}

