using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour, Observer, Subject
{
    private CustomPlayer player;
    private int score;
    private int scoreForVictory = 3;
    private NetworkManager networkManager;
    private bool isEndGame = false;
    private DateTime startLoadNewGameDateTime;
    private int waitingForSecondsForLoadNewGame = 5;
    private List<Observer> observers = new();
    public string Winner => winner;
    private string winner = string.Empty;
    
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
            if (IsTimeForLoadNewGameHasCome())
                CmdLoadNewGame();
    }

    public void UpdateState(object customObject, Event customEvent)
    {
        if (customObject.GetType() == typeof(CustomPlayer))
            if (customEvent == Event.ScoreUpdated)
            {
                score = player.Score;
                if (WasPlayerWin())
                    EndGame();
            }    
    }
    
    private bool WasPlayerWin()
    {
        return score >= scoreForVictory;
    }

    private void EndGame()
    {
        winner = player.Name;
        isEndGame = true;
        startLoadNewGameDateTime = DateTime.Now;
        NotifyObservers(Event.EndGame);
    }
    
    private bool IsTimeForLoadNewGameHasCome()
    {
        var timePassed = DateTime.Now - startLoadNewGameDateTime;
        return timePassed.TotalSeconds >= waitingForSecondsForLoadNewGame;
    }

    //Надо сделать, чтобы команда запускалась на сервере. И не только эта команда класса GameManager.
    //К сожалению, не успел полностью разобраться в этом вопросе и отладить программу.
    //Если дедлайн можно отложить на пару дней, то я доделаю тестовое задание.
    //[Command]
    private void CmdLoadNewGame()
    {
        var scene = SceneManager.GetActiveScene();
        networkManager.ServerChangeScene(scene.name);
        ResetIsEndGame();
    }

    private void ResetIsEndGame()
    {
        isEndGame = false;
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
        observers.Remove(observer);
    }

    public void NotifyObservers(Event customEvent)
    {
        foreach (var observer in observers)
            observer.UpdateState(this, customEvent);
    }
}