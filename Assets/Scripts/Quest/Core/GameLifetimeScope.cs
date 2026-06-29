using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// VContainer LifetimeScope
/// 씬에 빈 GameObject로 배치, QuestManager 등록
/// Inspector에서 QuestData 리스트 순서대로 등록
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private List<QuestData> questDataList;
    [SerializeField] private ExpManager expManager;

    protected override void Configure(IContainerBuilder builder)
    {
        // QuestData 리스트를 인스턴스로 등록
        builder.RegisterInstance(questDataList);

        // ExpManager MonoBehaviour 등록
        builder.RegisterComponent(expManager);

        // QuestManager 등록 — IStartable, IDisposable 자동 연결
        builder.Register<QuestManager>(Lifetime.Singleton)
               .AsImplementedInterfaces()
               .AsSelf();
    }
}
