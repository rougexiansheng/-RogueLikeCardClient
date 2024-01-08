using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

/// <summary>
/// 主流程入口
/// </summary>
public class MainFlowEntry : MonoBehaviour
{
    public GameObject repoter;
    public GameObject statsMonitor;

    [SerializeField]
    [Inject]
    MainFlowController mainFlowController;
    public static MainFlowEntry instance;
    void Awake()
    {
        if (instance == null) instance = this;
    }
    private void OnDestroy()
    {
        mainFlowController.Stop();

    }
    public void Start()
    {
        if (GameConfig.instance.enableReporter)
        {
            Instantiate(repoter);
        }
        if (GameConfig.instance.enableStatsMonitor)
        {
            Instantiate(statsMonitor);
        }

        mainFlowController.Trigger(MainFlowController.MainFlowState.FakeServerInit);
    }

    public void Update()
    {
        mainFlowController.Update();
    }
    [InspectorButton]
    void test()
    {
        Debug.Log("Test");
    }
}
