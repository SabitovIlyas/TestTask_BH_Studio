using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour, Observer
{
    public InputManager InputManager
    {
        set
        {
            inputManager = value;
            inputManager.RegisterObserver(this);
        }
    }
    
    private InputManager inputManager;
    [SerializeField] private TextMeshProUGUI scoreValue;
    [SerializeField] private TextMeshProUGUI endGameText;
    [SerializeField] private TextMeshProUGUI debugText;
    private CustomPlayer player;
    private GameManager gameManager;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void InitializePlayer(GameObject customPlayerGameObject)
    {
        player = customPlayerGameObject.GetComponent<CustomPlayer>();
        player.RegisterObserver(this);
    }

    public void InitializeGameManager(GameObject gameManagerGameObject)
    {
        gameManager = gameManagerGameObject.GetComponent<GameManager>();
        gameManager.RegisterObserver(this);
    }

    public void UpdateState(object customObject, Event customEvent)
    {
        if (customObject.GetType() == typeof(CustomPlayer))
            if (customEvent == Event.ScoreWasUpdated)
            {
                var playerScore = player.Score;
                scoreValue.text = playerScore.ToString();
            }
        
        if (customObject.GetType() == typeof(GameManager))
            if (customEvent == Event.EndGame)
                endGameText.text = string.Format("Победил игрок {0}!", gameManager.WinnerName);
        
        if (customObject.GetType() == typeof(InputManager))
            if (customEvent == Event.KeyPressedF2)
                debugText.text = "";
    }

    public void DisplayLog(string message)
    {
        if (debugText.IsDestroyed())
            return;
                
        if (debugText.isActiveAndEnabled)
            debugText.text += string.Format("\r\n{0}", message);
    }

    private void OnDestroy()
    {
        player?.RemoveObserver(this);
        gameManager?.RemoveObserver(this);
        inputManager?.RemoveObserver(this);
    }
}