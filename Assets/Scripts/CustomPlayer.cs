using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CustomPlayer : NetworkBehaviour, Subject, Observer
{
    public int Score => score;
    public string Name { set => name = value; }
    public Logger Logger { set => logger = value; }

    public InputManager InputManager
    {
        set
        {
            inputManager = value;
            inputManager.RegisterObserver(this);
        }
    }

    private InputManager inputManager;
    private Logger logger = NullLogger.Create();
    [SyncVar] [SerializeField] private int invulnerabilityLimitSeconds = 3;
    [SyncVar] [SerializeField] private bool isTouched;
    [SyncVar] [SerializeField] private bool isInvulnerability;
    private RelativeMovement relativeMovement;
    private bool wasTouchAnotherPlayer;
    private int previousScore;
    [SyncVar] [SerializeField] private int score;
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
        if (isServer)
        {
            if (WasTouched())
            {
                ResetIsTouched();
                SetInvulnerability();
            }
            else if (isInvulnerability)
                if (IsInvulnerabilityRanOut())
                    ResetIsInvulnerability();
        }
        if (isClient)
        {
            if (WasScoreUpdated())
            {
                NotifyObserversAboutScoreUpdated();
                ResetScoreUpdatedFlag();
            }
        }
    }

    private bool WasScoreUpdated()
    {
        return previousScore != score;
    }

    private void ResetScoreUpdatedFlag()
    {
        previousScore = score;
    }

    [Command]
    private void CmdHitAnotherPlayer(CustomPlayer player)
    {
        if (!relativeMovement.Jerked)
            return;

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
            CmdHitAnotherPlayer(jerkedPlayer);
        }
    }

    private void IncreaseScore()
    {
        logger.Log("Увеличиваем количество очков!");
        score++;
    }

    public CustomPlayer GetJerkedPlayer()
    {
        return jerkedPlayer;
    }

    public void SetIsTouched()
    {
        isTouched = true;
    }

    [Server]
    private bool WasTouched()
    {
        return isTouched;
    }

    [Server]
    private void ResetIsTouched()
    {
        isTouched = false;
    }

    [Server]
    private void SetInvulnerability()
    {
        isInvulnerability = true;
        startInvulnerabilityDateTime = DateTime.Now;
        SetColor();
    }

    [Server]
    private void ResetIsInvulnerability()
    {
        isInvulnerability = false;
        ResetColor();
    }

    [Server]
    private bool IsInvulnerabilityRanOut()
    {
        var timePassed = DateTime.Now - startInvulnerabilityDateTime;
        return timePassed.TotalSeconds >= invulnerabilityLimitSeconds;
    }

    private void NotifyObserversAboutScoreUpdated()
    {
        NotifyObservers(Event.ScoreWasUpdated);
    }

    [ClientRpc]
    private void ResetColor()
    {
        material.color = defaultColor;
    }

    [ClientRpc]
    private void SetColor()
    {
        material.color = colorInvulnerability;
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

    public void UpdateState(object customObject, Event customEvent)
    {
        if (customObject.GetType() == typeof(InputManager))
            if (customEvent == Event.KeyPressedF1)
            {
                var message = String.Format("=== {0} {1} ===\r\n=== ///////////// ===\r\n", gameObject, this);
                logger.LogWithData(message, this);
            }
    }

    private void OnDestroy()
    {
        inputManager?.RemoveObserver(this);
    }

    public void GetAuthorityToGameManager(GameObject gameManagerGameObject)
    {
        CmdGetAuthorityToGameManager(gameManagerGameObject);
    }

    [Command]
    private void CmdGetAuthorityToGameManager(GameObject gameManagerGameObject)
    {
        var gameManager = gameManagerGameObject.GetComponent<GameManager>();
        var gameManagerNetIdentity = gameManager.netIdentity;
        gameManagerNetIdentity.AssignClientAuthority(connectionToClient);
    }
}