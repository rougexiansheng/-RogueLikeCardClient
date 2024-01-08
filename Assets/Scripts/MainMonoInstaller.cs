using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Zenject;

public class MainMonoInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // 存檔相關
        Container.BindInterfacesAndSelfTo<NetworkSaveManager>().AsSingle().NonLazy();
        // 假server
        Container.BindInterfacesAndSelfTo<FakeServer>().AsSingle().NonLazy();
        // 戰鬥相關
        Container.BindInterfacesAndSelfTo<SkillManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SkillAbilityMethods>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<PassiveManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PassiveAbilityMethods>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<BattleManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ItemManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MonsterManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<EnvironmentManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<DataManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<DataTableManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PreloadManager>().AsSingle().NonLazy();
        // 戰鬥演出
        Container.BindInterfacesAndSelfTo<PerformanceMethods>().AsSingle().NonLazy();
        // 流程相關
        Container.BindInterfacesAndSelfTo<MainFlowController>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GameFlowController>().AsSingle().NonLazy();
        Container.Bind<MainFlowEntry>().FromComponentInHierarchy().AsSingle();
        Container.Bind<UIManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<AssetManager>().FromComponentInHierarchy().AsSingle(); 
    }
}
