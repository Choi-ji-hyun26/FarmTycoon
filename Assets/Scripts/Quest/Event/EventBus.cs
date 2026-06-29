using System;
using System.Collections.Generic;

/// <summary>
/// 타입 기반 제네릭 이벤트 버스
/// 발행: EventBus<TEvent>.Publish(payload)
/// 구독: EventBus<TEvent>.Subscribe(handler)
/// 해제: EventBus<TEvent>.Unsubscribe(handler)
/// </summary>
public static class EventBus<T>
{
    private static readonly List<Action<T>> _handlers = new();

    public static void Subscribe(Action<T> handler)
    {
        if (!_handlers.Contains(handler))
            _handlers.Add(handler);
    }

    public static void Unsubscribe(Action<T> handler)
    {
        _handlers.Remove(handler);
    }

    public static void Publish(T payload)
    {
        // 순회 중 handlers 변경에 대비해 복사본으로 순회
        var snapshot = new List<Action<T>>(_handlers);
        foreach (var handler in snapshot)
            handler?.Invoke(payload);
    }

    /// <summary>
    /// 테스트 및 씬 전환 시 핸들러 전체 초기화
    /// </summary>
    public static void Clear()
    {
        _handlers.Clear();
    }
}