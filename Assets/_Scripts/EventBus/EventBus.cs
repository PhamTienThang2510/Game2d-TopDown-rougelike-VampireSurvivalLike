using System;

public static class EventBus<T> where T : struct, IEvent
{
    private static event Action<T> OnEvent;

    public static void Subscribe(Action<T> listener) => OnEvent += listener;
    public static void Unsubscribe(Action<T> listener) => OnEvent -= listener;
    public static void Publish(T eventData = default) => OnEvent?.Invoke(eventData);
    public static void Clear() => OnEvent = null;
}
