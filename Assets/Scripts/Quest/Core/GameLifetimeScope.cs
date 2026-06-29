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
        // QuestData 리스트를 인스턴스로 등록
        builder.RegisterInstance(questDataList);

        // MonoBehaviour 등록
        builder.RegisterComponent(expManager);
        builder.RegisterComponent(questUIController);

        // QuestManager 등록 — IStartable, IDisposable 자동 연결
        builder.Register<QuestManager>(Lifetime.Singleton)
               .AsImplementedInterfaces()
               .AsSelf();
    }

    // QuestArrowDirector는 VContainer 외부에서 초기화
    // QuestManager가 IStartable.Start() 이후 사용 가능하므로
    // LifetimeScope의 OnAfterBuild에서 연결
    protected override void Awake()
    {
        base.Awake();
        // Container 빌드 완료 후 QuestArrowDirector 초기화
        this.Container.Resolve<QuestManager>();
    }

    private void Start()
    {
        if (questArrowDirector != null)
            questArrowDirector.Initialize(Container.Resolve<QuestManager>());
    }
}
