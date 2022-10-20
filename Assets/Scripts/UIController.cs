using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour, Observer
{
    [SerializeField] private TextMeshProUGUI scoreValue;
    [SerializeField] private TextMeshProUGUI endGameText;
    private CustomPlayer player;
    private GameManager gameManager;

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
            if (customEvent == Event.ScoreUpdated)
            {
                var playerScore = player.Score;
                scoreValue.text = playerScore.ToString();
            }
        
        if (customObject.GetType() == typeof(GameManager))
            if (customEvent == Event.EndGame)
                endGameText.text = string.Format("Победил игрок {0}!", gameManager.Winner);
    }

    private void OnDestroy()
    {
        player?.RemoveObserver(this);
        gameManager?.RemoveObserver(this);
    }
}