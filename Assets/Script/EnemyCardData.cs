using UnityEngine;

public class EnemyCardData : MonoBehaviour
{
    public StateCulManager stateCulManager;

    public string enemyName;
    public float enemyHP = 500;
    public float enemyDamage = 70;
    public bool isBoss;
    public int enemyID;
    public Sprite img;
    public string ep;
    void Awake()
    {
        
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
            OnPointerExit();
    }
    public void OnpointClick()
    {
        stateCulManager.ShowTheDetailEnemyCard(gameObject.GetComponent<EnemyCardData>(), true);
    }

    void OnPointerExit()
    {
        stateCulManager.ShowTheDetailEnemyCard(gameObject.GetComponent<EnemyCardData>(), false);
    }

}
