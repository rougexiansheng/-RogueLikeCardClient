using SDKProtocol;
using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UIRankingItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_ranking;
    [SerializeField]
    private TMP_Text m_stageProgress;
    [SerializeField]
    private TMP_Text m_spendTime;
    [SerializeField]
    private TMP_Text m_heroName;
    [SerializeField]
    private TMP_Text m_playerName;

    public void Init(DungeonRankingDataItem data)
    {
        m_ranking.text = data.ranking.ToString();
        m_stageProgress.text = data.stageProgress.ToString();
        var time = TimeSpan.FromMilliseconds(data.spendTime);
        var sec = time.Seconds.ToString().PadLeft(2, '0'); // 秒數不足兩位數補0
        var ms = time.Milliseconds.ToString().PadLeft(3, '0');
        m_spendTime.text = $"{(int)time.TotalMinutes}:{sec}.{ms}"; // 要轉成 999:00
        m_heroName.text = (!string.IsNullOrEmpty(data.professionName)) ? data.professionName : "密絲可";
        m_playerName.text = data.playerName;
    }
}
