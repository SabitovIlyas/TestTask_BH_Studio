using System;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer : MonoBehaviour, Observer
{
    private Vector3 startCameraPositionOffset = new Vector3(0, 1.5f, -3.75f);
    private GameObject cameraGameObject;
    private OrbitCamera orbitCamera;
    private GameObject playerGameObject;
    private CustomPlayer player;
    private GameObject uiControllerGameObject;
    private UIController uiController;
    private GameObject gameManagerGameObject;
    private GameManager gameManager;
    private GameObject networkManagerGameObject;
    private NetworkManager networkManager;
    private int playerCounter = 0;
    private Logger logger;
    private GameObject inputManagerGameObject;
    private InputManager inputManager;
    private bool isCameraSet;
    private bool isPlayerSet;
    private bool isGameManagerSet;
    private bool isUIControllerSet;
    private bool isUILoggerSet;
    private bool isSpawnGameManagerNeeded;
    private bool debug = false;

    private void Awake()
    {
        var initializers = GameObject.FindGameObjectsWithTag("Initializer");
        if (initializers.Length > 1)
            DestroyImmediate(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += HandlerActiveSceneChanged;
        if (debug)
            logger = UILogger.Create();
        else
            logger = DebugLogger.Create();
    }

    private void InitializeNetworkManager()
    {
        networkManager = networkManagerGameObject.GetComponent<NetworkManager>();
        networkManager.GetInstanceID();
    }
    
    private void InitializeInputManager()
    {
        inputManager = inputManagerGameObject.GetComponent<InputManager>();
    }

    public void UpdateState(GameObject customObject, Event customEvent)
    {
        var gameObjectName = customObject.name;
        gameObjectName = gameObjectName.ToLower();
        gameObjectName = gameObjectName.Trim();
        if (gameObjectName.Contains("camera"))
        {
            if (customEvent == Event.CameraWasCreated)
            {
                cameraGameObject = customObject;
                logger.Log("Camera was linked");
            }
        }

        if (gameObjectName.Contains("player"))
        {
            if (customEvent == Event.PlayerWasCreated)
            {
                var newPlayer = customObject.GetComponent<CustomPlayer>();
                if (newPlayer.isLocalPlayer)
                    playerGameObject = customObject;
                playerCounter++;
                SetPlayerName(customObject);
                logger.Log("Player was linked");
            }
        }
        
        if (gameObjectName.Contains("ui"))
            if (customEvent == Event.UIControllerWasCreated)
            {
                uiControllerGameObject = customObject;
                logger.Log("UIController was linked");
            }
        
        if (gameObjectName.Contains("gamemanager"))
            if (customEvent == Event.GameManagerWasCreated)
            {
                gameManagerGameObject = customObject;
                logger.Log("GameManager was linked");
            }

        if (gameObjectName.Contains("networkmanager"))
            if (customEvent == Event.NetworkManagerWasCreated)
            {
                networkManagerGameObject = customObject;
                InitializeNetworkManager();
                logger.Log("NetworkManager was linked");
            }
        
        if (gameObjectName.Contains("inputmanager"))
            if (customEvent == Event.InputManagerWasCreated)
            {
                inputManagerGameObject = customObject;
                InitializeInputManager();
                logger.Log("InputManager was linked");
            }

        if (WasCameraInitialized() && WasPlayerInitialized() && !isCameraSet && !isPlayerSet)
        {
            SetUpCamera();
            if (isCameraSet)
                logger.Log("Camera was set");
            SetUpPlayer();
            if (isPlayerSet)
                logger.Log("Player was set");
        }
        
        if (WasPlayerInitialized() && WasUIControllerInitialized() && WasGameManagerInitialized() &&
            !isUIControllerSet && !isUILoggerSet)
        {
            SetUpUIController();
            if (isUIControllerSet)
                logger.Log("UIController was set");

            SetUpUILogger();
            if (isUILoggerSet)
                logger.Log("UILogger was set");
        }

        if (WasGameManagerInitialized() && WasPlayerInitialized() && !isGameManagerSet)
        {
            SetUpGameManager();
            if (isGameManagerSet)
                logger.Log("Game Manager was set");
        }
        
        if (isSpawnGameManagerNeeded)
        {
            SpawnGameManager();
            isSpawnGameManagerNeeded = false;
        }
    }

    private void SetPlayerName(GameObject playerGameObject)
    {
        var player = playerGameObject.GetComponent<CustomPlayer>();
        player.Name = String.Format("Player №{0}", playerCounter);
    }
    
    private bool WasCameraInitialized()
    {
        return cameraGameObject != null;
    }
    
    private bool WasPlayerInitialized()
    {
        return playerGameObject != null;
    }
    
    private bool WasGameManagerInitialized()
    {
        return gameManagerGameObject != null;
    }
    
    private bool WasUIControllerInitialized()
    {
        return uiControllerGameObject != null;
    }

    private void SetUpCamera()
    {
        orbitCamera = cameraGameObject.GetComponent<OrbitCamera>();
        orbitCamera.Logger = logger;
        var playerTransform = playerGameObject.transform;
        orbitCamera.Target = playerTransform;
        var playerPosition = playerTransform.position;
        var startCameraPosition = playerPosition + startCameraPositionOffset;
        cameraGameObject.transform.position = startCameraPosition;
        orbitCamera.InitializePosition();
        isCameraSet = true;
    }

    private void SetUpPlayer()
    {
        var playerMovement = playerGameObject.GetComponent<RelativeMovement>();
        playerMovement.Target = cameraGameObject.transform;
        player = playerGameObject.GetComponent<CustomPlayer>();
        player.Logger = logger;
        player.InputManager = inputManager;
        isPlayerSet = true;
    }

    private void SetUpGameManager()
    {
        if (!networkManager.IsDestroyed())
            TrySetUpGameManager();
        else
            logger.Log("networkManager.IsDestroyed(): " + networkManager.GetInstanceID());
    }

    private void TrySetUpGameManager()
    {
        gameManager = gameManagerGameObject.GetComponent<GameManager>();
        gameManager.InitializePlayer(playerGameObject);
        gameManager.Logger = logger;
        gameManager.InitializeNetworkManager(networkManagerGameObject);
        gameManager.RegisterObserver(this);
        isGameManagerSet = true;
    }
    
    private void SetUpUIController()
    {
        uiController = uiControllerGameObject.GetComponent<UIController>();
        uiController.InitializePlayer(playerGameObject);
        uiController.InitializeGameManager(gameManagerGameObject);
        isUIControllerSet = true;
    }
    
    private void SetUpUILogger()
    {
        if (logger.GetType() == typeof(UIController))
        {
            var uiLogger = (UILogger)logger;
            uiLogger.UIController = uiController;
        }
        
        uiController.InputManager = inputManager;
        isUILoggerSet = true;
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
        player?.RemoveObserver(this);
        SceneManager.activeSceneChanged -= HandlerActiveSceneChanged;
    }

    private void HandlerActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ResetSettings();

        if (newScene.name == "OnlineScene")
            isSpawnGameManagerNeeded = true;
    }

    private void SpawnGameManager()
    {
        try
        {
            if (!NetworkServer.active)
                return;

            var prefabs = networkManager.spawnPrefabs;
            var prefabGameManager = prefabs.Find(p=>p.name.ToLower().Trim().Contains("gamemanager"));
            GameObject gameManagerGameObject = Instantiate(prefabGameManager, new Vector3(0, 0, 0), 
                Quaternion.identity);
            NetworkServer.Spawn(gameManagerGameObject);
        }
        catch (Exception e)
        {
            var message = e.Message;
            if (message.Contains("Cannot spawn objects without an active server"))
                logger.LogWithData("Попытка spawn'ить Game Manager на клиенте: " + message, this);
            else
                throw;
        }
        
    }

    private void ResetSettings()
    {
        isCameraSet = false;
        isPlayerSet = false;
        isGameManagerSet = false;
        isUIControllerSet = false;
        isUILoggerSet = false;
        playerCounter = 0;
    }
}