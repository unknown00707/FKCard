using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class EnemyMonster
{
    public Button[] monsters;
}

public class EnemyCulGroup : MonoBehaviour
{
    public StateCulManager stateCulManager;
    public Button[] enemyPrefabs;
    [Header("Monster")]
    public List<EnemyMonster> stageMonsters;
    [Header("Boss")]
    public Button[] stageBoss;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        foreach(Button btn in enemyPrefabs)
        {
            btn.gameObject.SetActive(false);
        }

        MakeSameInit(0,1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int RanMonsterGetValue(int stageNum)
    {
        return Random.Range(0, stageMonsters[stageNum].monsters.Count());
    }

    void MakeSameInit(int stageNum, int howMuch)
    {
        for(int i = 0; i < howMuch; i ++)
        {
            EnemyCardData enemyCardData = stageMonsters[stageNum].monsters[RanMonsterGetValue(stageNum)].GetComponent<EnemyCardData>();
            EnemyCardData prefabData = enemyPrefabs[i].GetComponent<EnemyCardData>();
            TextMeshProUGUI[] textpro = enemyPrefabs[i].GetComponentsInChildren<TextMeshProUGUI>();
            
            prefabData.enemyName = enemyCardData.enemyName;
            prefabData.enemyHP = enemyCardData.enemyHP;
            prefabData.enemyDamage = enemyCardData.enemyDamage;
            prefabData.isBoss = enemyCardData.isBoss;
            prefabData.enemyID = enemyCardData.enemyID;
            prefabData.img = enemyCardData.img;
            prefabData.ep = enemyCardData.ep;
            enemyPrefabs[i].image.sprite = prefabData.img;
            enemyPrefabs[i].gameObject.SetActive(true);

            textpro[0].text = prefabData.enemyHP.ToString();
            textpro[1].text = prefabData.enemyDamage.ToString();
        }
    }
}
