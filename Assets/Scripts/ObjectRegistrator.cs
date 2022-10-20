using UnityEngine;

public class ObjectRegistrator : MonoBehaviour
{
    [SerializeField] private Event customEvent;
    
    void Start()
    {
        var initializerGameObject = GameObject.FindWithTag("Initializer");
        var initializer = initializerGameObject.GetComponent<Initializer>();
        initializer.UpdateState(gameObject, customEvent);
    }
}
