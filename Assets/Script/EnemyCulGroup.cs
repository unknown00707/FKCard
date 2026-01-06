using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Android.Gradle.Manifest;
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
    public StageManager stageManager;
    public TurnManager turnManager;
    public Button[] enemyPrefabs;
    public TextMeshProUGUI totalHP;
    public TextMeshProUGUI totalDG;
    public int NUM_OF_DIFFER_TURN = 6;
    [Header("Monster")]
    public List<EnemyMonster> stageMonsters;
    public Dictionary<int, EnemyCardData> activeMonsterDic = new();
    private Dictionary<int, IMonasterEffect> monsterCardEffects; // current stage Num , monster index / current turn
    private Dictionary<(int, int), bool> mostserCheckUseSpecailSkill = new();
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

    private void InitializeCardEffects()
    {
        monsterCardEffects = new Dictionary<int, IMonasterEffect>
        {
            // 잡몹의 스킬들
            { 0, new Biting() },
            { 1, new Biting() },
            { 2, new Biting() },
            { 3, new Biting() },
            { 4, new Biting() },
            { 5, new Biting() },
        };
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
            prefabData.useSkillNums = enemyCardData.useSkillNums;
            prefabData.img = enemyCardData.img;
            prefabData.ep = enemyCardData.ep;
            enemyPrefabs[i].image.sprite = prefabData.img;
            enemyPrefabs[i].gameObject.SetActive(true);

            textpro[0].text = prefabData.enemyHP.ToString();
            textpro[1].text = prefabData.enemyDamage.ToString();

            activeMonsterDic.Add(i, prefabData);
        }
    }

    public void MakeSameTotalState()
    {
        float hp = 0;
        float dg = 0;

        foreach( EnemyCardData value in activeMonsterDic.Values)
        {
            hp += value.enemyHP;
            dg += value.enemyDamage;
        }

        totalHP.text = hp.ToString();
        totalDG.text = dg.ToString();
    }

    public void RequsetTheDamageToMonster()
    {
        foreach(UserDamgeGroup userDamgeGroup in cEACDManager.userAboutDamages)
        {
            for (int i = userDamgeGroup.damage.Count  - 1; i >= 0; i--)
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
                        Debug.Log($"공격! 남은 횟수: {damageData.numberOfHits}, 적 HP: {targetEnemy.enemyHP}");
                        damageData.numberOfHits--;
                        
                    }

                    // 5. 공격 횟수를 다 썼으면 리스트에서 영구 삭제
                    DamageDataRemoveByHitHumber(userDamgeGroup, damageData, i);
                }
                else // 플레이어 대상 (자해 / 팀 공격 등)
                {
                    cEACDManager.RequsetDownUserCurrentStatFromDamage(damageData);
                    // ... 플레이어 로직 ...
                    DamageDataRemoveByHitHumber(userDamgeGroup, damageData,i);
                }
            }
        }
        print("적에게 데미지 가하기 성공!");
        MonsterDieCheck();
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
    void DamageDataRemoveByHitHumber(UserDamgeGroup userDamgeGroup,UserAoubtDamage userAoubtDamage, int index)
    {
        if (userAoubtDamage.numberOfHits <= 0 && userAoubtDamage.cuurentTrun < (turnManager.GiveTurnValue() - NUM_OF_DIFFER_TURN))
            userDamgeGroup.damage.RemoveAt(index);
    }

    public void MonsterDieCheck()
    {
        var targetDic = activeMonsterDic.Where(data => data.Value.enemyHP <= 0)
            .Select(data => data.Key)
            .ToList();
        foreach(var target in targetDic)
        {
            activeMonsterDic.Remove(target);
        }
        enemyPrefabs.ToList().ForEach(btn =>
        {
            if (activeMonsterDic.ContainsValue(btn.GetComponent<EnemyCardData>()))
                return;
            btn.gameObject.SetActive(false);
        });
        MakeSameTotalState();
    }
    public void AttackAllEnemyCulOnStage()
    {
        int currentTurn = turnManager.GiveTurnValue();
        for(int i = 0; i < activeMonsterDic.Count; i++)
        {
            // Dictionary에서 해당 직업/카드의 효과를 찾아서 실행
            if (monsterCardEffects.TryGetValue(RandomSkillVAlue(i), out IMonasterEffect effect))
            {
                effect.ApplyEffect(this, currentTurn);
            }
            else
            {
                Debug.LogWarning($"아직 구현되지 않은 카드 효과입니다: {stageManager.GiveStageNum()}, Index: {0}");
            }    
        }
        
        
        print("모든 몬스터가 플레이어에게 데미지 주기 성공!");
    }

    int RandomSkillVAlue(int targetID)
    {
        int enemyCardIndex = activeMonsterDic[targetID].enemyID;
        int ranValue = Random.Range(0, activeMonsterDic[targetID].useSkillNums.Count());
        int useSkillNum = activeMonsterDic[targetID].useSkillNums[ranValue];
        if(mostserCheckUseSpecailSkill.TryGetValue((enemyCardIndex, useSkillNum),  out bool isUsed))
        {
            if(isUsed)
            {
                int postRanValue = ranValue;
                while(postRanValue == ranValue)
                {
                    ranValue = Random.Range(0, activeMonsterDic[targetID].useSkillNums.Count());
                }
                useSkillNum = activeMonsterDic[targetID].useSkillNums[ranValue];
                mostserCheckUseSpecailSkill.Add((enemyCardIndex, useSkillNum), true);
            }
            else
                mostserCheckUseSpecailSkill.Add((enemyCardIndex, useSkillNum), true);
        }
        else
        {
            mostserCheckUseSpecailSkill.Add((enemyCardIndex, useSkillNum), true); // 스킬 사용했다는 걸 표시
        }
        return  useSkillNum;
    }
}
