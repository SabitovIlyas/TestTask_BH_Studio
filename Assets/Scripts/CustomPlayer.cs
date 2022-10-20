using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CustomPlayer : NetworkBehaviour, Subject
{
    public int Score => score;
    public string Name { get => name; set => name = value; }

    [SerializeField] private int invulnerabilityLimitSeconds = 3;
    private bool isTouched;
    private bool isInvulnerability;
    private RelativeMovement relativeMovement;
    private bool wasTouchAnotherPlayer;
    private int score;
    private CustomPlayer jerkedPlayer;
    private DateTime startInvulnerabilityDateTime;
    private Material material;
    private Color colorInvulnerability = Color.red;
    private Color defaultColor;
    private List<Observer> observers = new();
    
    void Start()
    {
        GetComponent<CharacterController>();
        relativeMovement = GetComponent<RelativeMovement>();
        var renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        material = renderer.material;
        defaultColor = material.color;
    }
    
    void Update()
    {
        if (WasTouched())
        {
            ResetIsTouched();
            SetInvulnerability();
        }
        else
        if (isInvulnerability)
            if (IsInvulnerabilityRanOut())
                ResetIsInvulnerability();
    }
    
    public void HitAnotherPlayer()
    {
        if (!relativeMovement.Jerked)
            return;
        
        var player = GetJerkedPlayer();
        
        if (player.isInvulnerability)
            return;

        player.SetIsTouched();
        IncreaseScore();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var hitCollider = hit.collider;
        var hitColliderName = hitCollider.name;
        var hitColliderNameLower = hitColliderName.ToLower();
        if (hitColliderNameLower.Contains("player"))
        {
            var playerGameObject = hitCollider.gameObject;
            jerkedPlayer = playerGameObject.GetComponent<CustomPlayer>();
            HitAnotherPlayer();
        }
    }

    public void IncreaseScore()
    {
        score++;
        NotifyObservers(Event.ScoreUpdated);
    }
    public CustomPlayer GetJerkedPlayer()
    {
        return jerkedPlayer;
    }

    public void SetIsTouched()
    {
        isTouched = true;
    }
    
    public bool WasTouched()
    {
        return isTouched;
    }

    public void ResetIsTouched()
    {
        isTouched = false;
    }
    
    public void SetInvulnerability()
    {
        isInvulnerability = true;
        startInvulnerabilityDateTime = DateTime.Now;
        SetColor();
    }

    public void ResetIsInvulnerability()
    {
        isInvulnerability = false;
        ResetColor();
    }

    public bool IsInvulnerabilityRanOut()
    {
        var timePassed = DateTime.Now - startInvulnerabilityDateTime;
        return timePassed.TotalSeconds >= invulnerabilityLimitSeconds;
    }

    private void SetColor()
    {
        material.color = colorInvulnerability;
    }

    private void ResetColor()
    {
        material.color = defaultColor;
    }

    public void RegisterObserver(Observer observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(Observer observer)
    {
        observers.Remove(observer);
    }

    public void NotifyObservers(Event customEvent)
    {
        foreach (var observer in observers)
            observer.UpdateState(this, customEvent);
    }
}