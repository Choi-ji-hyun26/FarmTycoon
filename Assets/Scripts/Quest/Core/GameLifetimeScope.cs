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
    [SerializeField] private QuestUIController questUIController;
    [SerializeField] private QuestArrowDirector questArrowDirector;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(questDataList);
        builder.RegisterComponent(expManager);
        builder.RegisterComponent(questUIController);

        builder.Register<QuestManager>(Lifetime.Singleton)
               .AsImplementedInterfaces()
               .AsSelf();
    }

    protected override void Awake()
    {
        base.Awake();

        // Container 빌드 완료 후 QuestArrowDirector 초기화
        // QuestManager.Start()보다 먼저 Initialize 등록해야
        // 첫 OnQuestChanged 이벤트를 놓치지 않음
        var questManager = Container.Resolve<QuestManager>();
        if (questArrowDirector != null)
            questArrowDirector.Initialize(questManager);
    }
}
