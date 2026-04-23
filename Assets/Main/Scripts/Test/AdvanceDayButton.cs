using NarrativeGP;
using UnityEngine;

public class AdvanceDayButton : MonoBehaviour
{
    [SerializeField] private GameState gameState;

    private void Reset()
    {
        gameState = FindFirstObjectByType<GameState>();
    }

    public void OnClick()
    {
        if (gameState == null)
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        if (gameState != null)
        {
            gameState.StartNewDay();
        }
    }
}
