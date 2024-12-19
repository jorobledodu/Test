using TMPro;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;
    public float Money; // Player's money

    private void Awake()
    {
        Singleton();
    }

    private void Singleton()
    {
        // Ensure Singleton behavior
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateMoneyText()
    {
        moneyText.text = $"Money: {Money}";
    }

    public void SayHello()
    {
        Debug.Log("Hello"); // Print 'Hello' in the console
    }
}
