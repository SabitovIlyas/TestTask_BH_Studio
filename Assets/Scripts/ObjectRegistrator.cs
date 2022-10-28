using System;
using System.Linq;
using UnityEngine;

public class ObjectRegistrator : MonoBehaviour
{
    [SerializeField] private Event customEvent;
    
    void Start()
    {
        InitializerUpdateState();
    }
    
    private void InitializerUpdateState()
    {
        try
        {
            TryInitializerUpdateState();
        }
        catch (MissingReferenceException e)
        {
            Debug.Log(e);
            TryInitializerUpdateState();
        }
        
        catch (NullReferenceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    private void TryInitializerUpdateState()
    {
        var initializerGameObjects = GameObject.FindGameObjectsWithTag("Initializer");
        var initializerGameObject = initializerGameObjects.First();

        if (initializerGameObject == null)
        {
            Debug.Log("initializerGameObject==null");
        }
        
        if (initializerGameObject != gameObject)
        {
            var initializer = initializerGameObject.GetComponent<Initializer>();
            if (initializer != null)
                initializer.UpdateState(gameObject, customEvent);
            else
                Debug.Log("initializer==null");
        }
    }
}
