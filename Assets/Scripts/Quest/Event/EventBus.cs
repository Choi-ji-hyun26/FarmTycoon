using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 타입 기반 이벤트 버스 (비제네릭 중앙 관리)
/// 내부적으로 Dictionary<Type, Delegate>로 모든 이벤트 타입을 관리
/// 발행: EventBus.Publish(payload)
/// 구독: EventBus.Subscribe<TEvent>(handler)
/// 해제: EventBus.Unsubscribe<TEvent>(handler)
///
/// static 저장소가 하나뿐이므로 RuntimeInitializeOnLoadMethod 한 번으로
/// Domain Reload 비활성화 환경에서도 안전하게 초기화됨
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new();

    // 플레이 시작 시점에 자동 호출 — Domain Reload 비활성화 대응
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _handlers.Clear();
    }

    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);

        if (_handlers.TryGetValue(type, out var existing))
        {
            var current = (Action<T>)existing;
            // 중복 구독 방지
            if (!current.GetInvocationList().Contains((Delegate)handler))
                _handlers[type] = current + handler;
        }
        else
        {
            _handlers[type] = handler;
        }
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);

        if (!_handlers.TryGetValue(type, out var existing))
            return;

        var current = (Action<T>)existing - handler;

        if (current == null)
            _handlers.Remove(type);
        else
            _handlers[type] = current;
    }

    public static void Publish<T>(T payload)
    {
        var type = typeof(T);

        if (!_handlers.TryGetValue(type, out var existing))
            return;

        // Delegate.Invoke는 내부적으로 호출 리스트 복사본을 사용하므로
        // 순회 중 구독 해제가 발생해도 안전
        ((Action<T>)existing).Invoke(payload);
    }

    public static void Clear()
    {
        _handlers.Clear();
    }
}
