using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// VContainer LifetimeScope
/// 씬에 빈 GameObject로 배치, 퀘스트 시스템 의존성 등록
/// Inspector에서 QuestData 리스트 순서대로 등록
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private List<QuestData> questDataList;
    [SerializeField] private ExpManager expManager;
    [SerializeField] private QuestUIController questUIController;
    [SerializeField] private QuestArrowDirector questArrowDirector;
    [SerializeField] private LevelUIController levelUIController;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(questDataList);
        builder.RegisterComponent(expManager);
        builder.RegisterComponent(questUIController);
        builder.RegisterComponent(questArrowDirector);
        builder.RegisterComponent(levelUIController);

        // QuestProgressTracker를 먼저 등록 — IStartable 실행 순서상 우선
        builder.Register<QuestProgressTracker>(Lifetime.Singleton)
               .AsImplementedInterfaces()
               .AsSelf();

        builder.Register<QuestManager>(Lifetime.Singleton)
               .AsImplementedInterfaces()
               .AsSelf();
    }
}
