using System;
using UnityEngine;

public class Initializer : MonoBehaviour, Observer
{
    private Vector3 startCameraPositionOffset = new Vector3(0, 1.5f, -3.75f);
    private GameObject cameraGameObject;
    private GameObject playerGameObject;
    private GameObject uiControllerGameObject;
    private GameObject gameManagerGameObject;
    private GameManager gameManager;
    private GameObject networkManagerGameObject;
    private int playerCounter = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (GameObject.FindGameObjectsWithTag("Initializer").Length > 1)
            Destroy(gameObject);
    }
    
    public void UpdateState(GameObject customObject, Event customEvent)
    {
        var gameObjectName = customObject.name;
        var gameObjectNameLower = gameObjectName.ToLower();
        if (gameObjectNameLower.Contains("camera"))
        {
            if (customEvent == Event.CameraWasCreated)
                cameraGameObject = customObject;
        }

        if (gameObjectNameLower.Contains("player"))
        {
            if (customEvent == Event.PlayerWasCreated)
            {
                var newPlayer = customObject.GetComponent<CustomPlayer>();
                if (newPlayer.isLocalPlayer)
                    playerGameObject = customObject;
                playerCounter++;
                SetPlayerName(customObject);
            }
        }
        
        if (gameObjectNameLower.Contains("ui"))
            if (customEvent == Event.UIControllerCreated)
                uiControllerGameObject = customObject;
        
        if (gameObjectNameLower.Contains("gamemanager"))
            if (customEvent == Event.GameManagerCreated)
                gameManagerGameObject = customObject;

        if (WasCameraInitilized() && WasPlayerInitilized())
        {
            SetUpCamera();
            SetUpPlayer();
        }

        if (WasPlayerInitilized() && WasGameManagerInitilized() && WasUIControllerInitilized())
        {
            SetUpGameManager();
            SetUpUIController();
        }
    }

    private void SetPlayerName(GameObject playerGameObject)
    {
        var player = playerGameObject.GetComponent<CustomPlayer>();
        player.Name = String.Format("Player №{0}", playerCounter);
    }
    
    private bool WasCameraInitilized()
    {
        return cameraGameObject != null;
    }
    
    private bool WasPlayerInitilized()
    {
        return playerGameObject != null;
    }
    
    private bool WasGameManagerInitilized()
    {
        return gameManagerGameObject != null;
    }
    
    private bool WasUIControllerInitilized()
    {
        return uiControllerGameObject != null;
    }

    private void SetUpCamera()
    {
        var orbitCamera = cameraGameObject.GetComponent<OrbitCamera>();
        var playerTransform = playerGameObject.transform;
        orbitCamera.Target = playerTransform;
        var playerPosition = playerTransform.position;
        var startCameraPosition = playerPosition + startCameraPositionOffset;
        cameraGameObject.transform.position = startCameraPosition;
        orbitCamera.InitializePosition();
    }

    private void SetUpPlayer()
    {
        var playerMovement = playerGameObject.GetComponent<RelativeMovement>();
        playerMovement.Target = cameraGameObject.transform;
    }

    private void SetUpGameManager()
    {
        var gameManager = gameManagerGameObject.GetComponent<GameManager>();
        gameManager.InitializePlayer(playerGameObject);
        networkManagerGameObject = GameObject.FindWithTag("NetworkManager");
        gameManager.InitializeNetworkManager(networkManagerGameObject);
        gameManager.RegisterObserver(this);
    }
    
    private void SetUpUIController()
    {
        var ui = uiControllerGameObject.GetComponent<UIController>();
        ui.InitializePlayer(playerGameObject);
        ui.InitializeGameManager(gameManagerGameObject);
    }
    
    public void UpdateState(object customObject, Event customEvent)
    {
        if (customObject.GetType() == typeof(GameManager))
            if (customEvent == Event.EndGame)
                playerCounter = 0;
    }

    private void OnDestroy()
    {
        gameManager?.RemoveObserver(this);
    }
}