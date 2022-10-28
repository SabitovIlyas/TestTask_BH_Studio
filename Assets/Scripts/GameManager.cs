using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour, Observer, Subject
{
    public Logger Logger { set => logger = value; }
    public string WinnerName => winnerName;

    private Logger logger = NullLogger.Create();
    private CustomPlayer player;
    private int score;
    private int scoreForVictory = 3;
    private NetworkManager networkManager;
    [SyncVar] [SerializeField] private bool isEndGame;
    private DateTime startLoadNewGameDateTime;
    private int waitingForSecondsForLoadNewGame = 5;
    private List<Observer> observers = new();
    [SyncVar] [SerializeField] private string winnerName = string.Empty;
    private bool wasObserversNotifiedAboutEndGame;
    [SyncVar] private bool isTimeForLoadNewGameHasCome;


    private void Awake()
    {
        ResetSettings();
    }

    public void InitializePlayer(GameObject playerGameObject)
    {
        player = playerGameObject.GetComponent<CustomPlayer>();
        player.RegisterObserver(this);
    }

    public void InitializeNetworkManager(GameObject networkManagerGameObject)
    {
        networkManager = networkManagerGameObject.GetComponent<CustomNetworkRoomManager>();
    }

    private void Update()
    {
        if (isEndGame)
        {
            if (hasAuthority) CmdIsTimeForLoadNewGameHasCome();

            if (isTimeForLoadNewGameHasCome && hasAuthority)
                CmdLoadNewGame();
            else if (isClient && !wasObserversNotifiedAboutEndGame)
            {
                NotifyObservers(Event.EndGame);
                wasObserversNotifiedAboutEndGame = true;
            }
        }

        else if (WasPlayerWin() && hasAuthority)
            CmdEndGame(player.gameObject);
        
    }

    public void UpdateState(object customObject, Event customEvent)
    {
        if (customObject.GetType() == typeof(CustomPlayer))
            if (customEvent == Event.ScoreWasUpdated)
            {
                var customPlayer = (CustomPlayer)customObject;
                score = customPlayer.Score;
                if (WasPlayerWin())                
                    customPlayer.GetAuthorityToGameManager(gameObject);              
            }
        
        if (customObject.GetType() == typeof(InputManager))
            if (customEvent == Event.KeyPressedF1)
            {
                var message = String.Format("=== {0} {1} ===\r\n=== ///////////// ===\r\n", gameObject, this);
                logger.LogWithData(message, this);
            }
    }
    
    private bool WasPlayerWin()
    {
        return score >= scoreForVictory;
    }

    [Command]
    private void CmdEndGame(GameObject playerGameObject)
    {
        var winner = playerGameObject.GetComponent<CustomPlayer>();
        winnerName = winner.name;
        isEndGame = true;
        startLoadNewGameDateTime = DateTime.Now;
    }
    
    [Command]
    private void CmdIsTimeForLoadNewGameHasCome()
    {
        var timePassed = DateTime.Now - startLoadNewGameDateTime;
        isTimeForLoadNewGameHasCome = timePassed.TotalSeconds >= waitingForSecondsForLoadNewGame;
    }

    [Command]
    public void CmdLoadNewGame()
    {
        logger.Log("CmdLoadNewGame()");
        var scene = SceneManager.GetActiveScene();
        Debug.Log("scene " + scene);
        Debug.Log("networkManager " + networkManager);
        if (networkManager == null)
        {
            var networkManagerGameObject = GameObject.FindWithTag("NetworkManager");
            networkManager = networkManagerGameObject.GetComponent<CustomNetworkRoomManager>();
        }
        networkManager.ServerChangeScene(scene.name);
    }

    private void ResetSettings()
    {
        isEndGame = false;
        score = 0;
        isTimeForLoadNewGameHasCome = false;
    }

    private void OnDestroy()
    {
        player?.RemoveObserver(this);
    }
    
    public void RegisterObserver(Observer observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(Observer observer)
    {
        if (observers != null && observers.Count > 0 && observer != null)
            observers.Remove(observer);
    }

    public void NotifyObservers(Event customEvent)
    {
        foreach (var observer in observers)
            observer.UpdateState(this, customEvent);
    }
}