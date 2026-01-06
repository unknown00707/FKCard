using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class EnemyDataOfDamage
{
    public float takenDamge;
    public float takenDamgeMultipler;

}
public class EnemyCardData : MonoBehaviour
{
    public StateCulManager stateCulManager;
    public List<EnemyCardData> enemyCardDatasByTrun;
    public string enemyName;
    public float enemyHP = 500;
    public float enemyDamage = 70;
    public bool isBoss;
    public int enemyID;
    public int[] useSkillNums;
    public Sprite img;
    public string ep;
    
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
