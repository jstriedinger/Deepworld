using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    private static List<IUpdateObserver> _observers = new List<IUpdateObserver>();
    private static List<IUpdateObserver> _pendingObservers = new List<IUpdateObserver>();

    private static int _currentIndex;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (_currentIndex = _observers.Count - 1; _currentIndex >= 0; _currentIndex--)
        {
            _observers[_currentIndex].ObserverUpdate();
        }
        
        _observers.AddRange(_pendingObservers);
        _pendingObservers.Clear();
    }

    public static void AddObserver(IUpdateObserver observer)
    {
        _pendingObservers.Add(observer);
    }

    public static void RemoveObserver(IUpdateObserver observer)
    {
        _observers.Remove(observer);
        _currentIndex--;
    }
}
