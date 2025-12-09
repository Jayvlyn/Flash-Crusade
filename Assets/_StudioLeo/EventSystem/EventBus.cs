using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> subscribers = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        if(subscribers.TryGetValue(typeof(T), out var existing))
        {
            subscribers[typeof(T)] = Delegate.Combine(existing, callback);
        }
        else
        {
            subscribers[typeof(T)] = callback;
        }
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        if(subscribers.TryGetValue(typeof(T),out var existing))
        {
            var newDelegate = Delegate.Remove(existing, callback);
            if(newDelegate == null)
            {
                subscribers.Remove(typeof(T));
            }
            else
            {
                subscribers[typeof(T)] = newDelegate;
            }
        }
    }

    public static void Publish<T>(T eventData)
    {
        if(subscribers.TryGetValue(typeof(T), out var del))
        {
            if(del is Action<T> callback)
            {
                callback.Invoke(eventData);
            }
        }
    }

    public static void ClearAll()
    {
        subscribers.Clear();
    }
}