using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬에 배치된 화살표 타겟 Transform을 ID로 관리
/// 각 Zone이 Start()에서 자신을 등록
/// QuestArrowDirector가 ID로 타겟 위치를 조회
/// </summary>
public class QuestArrowRegistry : MonoBehaviour
{
    public static QuestArrowRegistry Instance { get; private set; }

    private readonly Dictionary<string, Transform> _registry = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(string id, Transform target)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (!_registry.ContainsKey(id))
            _registry.Add(id, target);
        else
            Debug.LogWarning($"QuestArrowRegistry: '{id}' 이미 등록됨. 덮어씁니다.");
            _registry[id] = target;
    }

    public void Unregister(string id)
    {
        _registry.Remove(id);
    }

    public bool TryGetTarget(string id, out Transform target)
    {
        return _registry.TryGetValue(id, out target);
    }
}
