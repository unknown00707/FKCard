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
    public CardEffectAndCulDuringManager cEACDManager;
    public TurnManager turnManager;
    public Button[] enemyPrefabs;
    public TextMeshProUGUI totalHP;
    public TextMeshProUGUI totalDG;
    [Header("Monster")]
    public List<EnemyMonster> stageMonsters;
    public Dictionary<int, EnemyCardData> activeMonsterDic = new();
    [Header("Boss")]
    public Button[] stageBoss;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        MakeSameInit(0,2);
        MakeSameTotalState();
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
        foreach(Button btn in enemyPrefabs)
        {
            btn.gameObject.SetActive(false);
        }

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

            activeMonsterDic.Add(i, enemyCardData);
        }
    }

    public void MakeSameTotalState()
    {
        float hp = 0;
        float dg = 0;

        foreach(Button btn in enemyPrefabs)
        {
            if(btn.gameObject.activeInHierarchy)
            {
                EnemyCardData enemyCardData = btn.GetComponent<EnemyCardData>();
                hp += enemyCardData.enemyHP;
                dg += enemyCardData.enemyDamage;
            }
        }

        totalHP.text = hp.ToString();
        totalDG.text = dg.ToString();
    }

    public void RequsetTheDamageToMonster()
    {
        foreach(UserDamgeGroup userDamgeGroup in cEACDManager.userAboutDamages)
        {
            for (int i = userDamgeGroup.damage.Count; i >= 0; i--)
            {
                UserAoubtDamage damageData = userDamgeGroup.damage[i];
                if (damageData.cuurentTrun != turnManager.GiveTurnValue())
                    continue;

                if (damageData.isTargetEnemy) // 몬스터 공격
                {
                    // 타겟 몬스터 가져오기
                    // (TargetID가 유효한지, 몬스터가 살아있는지 체크하는 함수가 있다고 가정)
                    EnemyCardData targetEnemy = activeMonsterDic[(int)damageData.targetMonsterID];

                    while (damageData.numberOfHits > 0)
                    {
                        // 4. [핵심 기능] 타겟이 없거나 죽었으면, 살아있는 다른 몬스터 찾기
                        if (targetEnemy == null || targetEnemy.enemyHP <= 0)
                        {
                            targetEnemy = FindAliveEnemy(); // 살아있는 적 찾는 함수 필요
                            
                            if (targetEnemy == null) 
                            {
                                // 더 이상 때릴 적이 없으면 공격 중단
                                break; 
                            }
                            // 타겟이 바뀌었으니 ID 업데이트 (선택 사항)
                            // damageData.targetMonsterID = targetEnemy.monsterID; 
                        }

                        // 데미지 계산
                        float totalDmg = damageData.hitHpDamage + damageData.hitDGDamage + damageData.hitTakenDamage;
                        targetEnemy.enemyHP -= totalDmg;
                        // 공격 횟수 차감
                        damageData.numberOfHits--;
                        
                        Debug.Log($"공격! 남은 횟수: {damageData.numberOfHits}, 적 HP: {targetEnemy.enemyHP}");
                    }

                    // 5. 공격 횟수를 다 썼으면 리스트에서 영구 삭제
                    if (damageData.numberOfHits <= 0)
                    {
                        userDamgeGroup.damage.RemoveAt(i);
                    }
                }
                else // 플레이어 대상 (힐/버프 등)
                {
                    // ... 플레이어 로직 ...
                }
            }
            print("적에게 데미지 가하기 성공!");
        }
    }
    // 살아있는 아무 몬스터나 찾는 함수 (공격 전이용)
    EnemyCardData FindAliveEnemy()
    {
        foreach (EnemyCardData keyValue in activeMonsterDic.Values)
        {
            if (keyValue.enemyHP > 0)
                return keyValue;
        }
        return null; // 다 죽음
    }
}
