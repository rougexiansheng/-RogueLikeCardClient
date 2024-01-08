
using Zenject;
using UnityEngine;
public class PrMainMonoInstaller : MonoInstaller
{
    public override void InstallBindings()
    {

        // 流程相關
        Container.BindInterfacesAndSelfTo<PrMainFlowController>().AsSingle().NonLazy();
        Container.Bind<PrMainFlowEntry>().FromComponentInHierarchy().AsSingle();
        Container.Bind<UIManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<AssetManager>().FromComponentInHierarchy().AsSingle();
    }
}