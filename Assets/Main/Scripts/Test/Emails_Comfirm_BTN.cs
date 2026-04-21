using NarrativeGP;
using UnityEngine;

public class Emails_Comfirm_BTN : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        GameState gameState = FindObjectOfType<GameState>();
        if (gameState != null)
        {
            gameState.MarkSectionCompleted(SectionId.Emails);
        }
    }
}
