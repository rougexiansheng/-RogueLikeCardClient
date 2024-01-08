using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class PrMainFlowEntry : MonoBehaviour
{


    [SerializeField]
    [Inject]
    PrMainFlowController mainFlowController;
    public static PrMainFlowEntry instance;


    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnDestroy()
    {
        mainFlowController.Stop();
    }

    public void Start()
    {
        mainFlowController.Trigger(PrMainFlowController.PrMainFlow.Init);
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
