using System;
using System.Collections.Generic;
using System.Diagnostics;

public class ServiceLocator
{
    public static ServiceLocator Current { get; private set; }
    private readonly Dictionary<string, object> services = new Dictionary<string, object>();

    public ServiceLocator()
    {
        Current = this;
    }

    public static void Create()
    {
        Current = new ServiceLocator();
    }

    public T Get<T>()
    {
        string key = typeof(T).Name;
        if (!services.ContainsKey(key))
        {
            Debug.WriteLine($"{key} not registered as service.");
            throw new InvalidOperationException();
        }
        return (T)services[key];
    }

    public void Register<T>(T service)
    {
        string key = typeof(T).Name;
        if (services.ContainsKey(key))
        {
            Debug.WriteLine($"{key} already registered.");
            return;
        }

        services.Add(key, service);
        Debug.WriteLine($"{key} added as service.");
    }

    public void Unregister<T>()
    {
        string key = typeof(T).Name;
        if (!services.ContainsKey(key))
        {
            Debug.WriteLine($"Attempted unregister of {key} which does not exist.");
            return;
        }

        services.Remove(key);
    }
}