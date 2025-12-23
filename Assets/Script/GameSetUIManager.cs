using UnityEngine;

public class GameSetUIManager : MonoBehaviour
{
    public static GameSetUIManager instance {get; private set; }
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject evenetPanel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
