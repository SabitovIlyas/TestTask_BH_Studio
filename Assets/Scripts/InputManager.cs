using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour, Subject
{
    private List<Observer> observers = new();
    private readonly bool debug = false; 

    private void Awake()
    {
        var initializers = GameObject.FindGameObjectsWithTag("InputManager");
        if (initializers.Length > 1)
            DestroyImmediate(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!debug)
            return;
        
        if (Input.GetKeyDown(KeyCode.F1))
            NotifyObservers(Event.KeyPressedF1);
        if (Input.GetKeyDown(KeyCode.F2))
            NotifyObservers(Event.KeyPressedF2);
    }
    
    public void RegisterObserver(Observer observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(Observer observer)
    {
        if (observers != null && observers.Count > 1 && observer != null)
            observers.Remove(observer);
    }

    public void NotifyObservers(Event customEvent)
    {
        foreach (var observer in observers)
            observer.UpdateState(this, customEvent);
    }
}
