using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
[System.Serializable]
public class UserTime
{
    public float chaingingHp;
    public float chaingingDG;
    public float chaingingCritical;
    public float damageMultiplier; // 받는 피해 배수
    public float beneficialEffectMultiplier; // 이로운 효과 배수
}
[System.Serializable]
public class UserDamaingChang
{
    public ulong targetMonsterID;
    public bool isTargetEnemy;
    public float chaningDamaingForHp;
    public float chaningDamaingForDamage;
    public float chaningDamaingForTakenDamage;
    public int numberOfHits;
}
[System.Serializable]
public class UserAoubtDamage
{
    public ulong targetMonsterID;
    public bool isTargetEnemy;
    public float hitHpDamage; // 준 피해
    public float hitDGDamage;
    public float hitTakenDamage;
    public int numberOfHits;
    public float inDamage; // 받은 피해
    public int cuurentTrun;
}
[System.Serializable]
public class UserStat
{
    public float maxHp;
    public float maxDG;
    public float maxCritical;
    public float currentHp;
    public float currentDG;
    public float currentCritical;
    public float damageMultiplier;
    public float beneficialEffectMultiplier;
}

[System.Serializable]
public class UserDamgeGroup
{
    public List<UserAoubtDamage> damage = new();
}

public class CardEffectAndCulDuringManager : MonoBehaviour
{
    public float[][] userState = new float[3][]; // 초기 유저의 정보 -> 안 변함 -> 스테이지 변하면 변함
    // 유저의 지속 기간에 따른 스텟 변화 저장
    public List<UserTime> userChangingState = new(); 
    public List<UserDamaingChang> userDamaingChangs = new();
    public List<UserStat> userTotalStates = new(); // 현재 턴의 총 유저들의 스텟
    public List<UserDamgeGroup> userAboutDamages = new(); // 데미지 관련 리스트

    public int ciriticalMultiplier = 2;

    void Awake()
    {
        for(int i = 0; i < 3; i++)
        {
            userState[i] = new float[5];
            userChangingState.Add(new UserTime());
            userDamaingChangs.Add(new UserDamaingChang());
            userTotalStates.Add(new UserStat());
            userAboutDamages.Add(new UserDamgeGroup());
        }


        for(int i = 0; i < userAboutDamages.Count; i++)
        {
            int j = 0;
            while(j < 50)
            {
                userAboutDamages[i].damage.Add(new UserAoubtDamage());
                j++;
            }
        }

    }

    public void ReciveUsersStat(float hp, float dg, float crip, float damageTakenMultiplier, float beneficialEffectMultiplier ,ulong userID)
    {
        userState[userID][0] = hp;
        userState[userID][1] = dg;
        userState[userID][2] = crip;
        userState[userID][3] = damageTakenMultiplier;
        userState[userID][4] = beneficialEffectMultiplier;

        userTotalStates[(int)userID].maxHp = userState[userID][0];
        userTotalStates[(int)userID].maxDG = userState[userID][1];
        userTotalStates[(int)userID].maxCritical = userState[userID][2];
        userTotalStates[(int)userID].damageMultiplier = userState[userID][3];
        userTotalStates[(int)userID].beneficialEffectMultiplier = userState[userID][4];

        userTotalStates[(int)userID].currentHp = userTotalStates[(int)userID].maxHp;
        userTotalStates[(int)userID].currentDG = userTotalStates[(int)userID].maxDG;
        userTotalStates[(int)userID].currentCritical = userTotalStates[(int)userID].maxCritical;
    }

    public void ReciveUpStatUserByBuffer(float hp, float damage, float critical, float damageMultiplier, float beneficialEffectMultiplier, ulong userID , JobManager.Jobs cardType, int cardIndex) // 증가되는 유저 스텟 저장
    {
        print("유저 정보 저장 시작");
        if (userID < (ulong)userChangingState.Count)
        {
            userChangingState[(int)userID] = new UserTime()
            {
                chaingingHp = userTotalStates[(int)userID].maxHp*hp,
                chaingingDG = userTotalStates[(int)userID].maxDG*damage,
                chaingingCritical = critical,
                damageMultiplier = damageMultiplier,
                beneficialEffectMultiplier = beneficialEffectMultiplier
            };
            switch(cardType)
            {
                case JobManager.Jobs.defender :
                    if(cardIndex == 4)
                    {
                        userChangingState[(int)userID].chaingingHp = 
                                userAboutDamages[(int)userID].damage[GameManager.Instance.totalTrunNum.Value -1].inDamage * hp;
                    }
                    break; 
            }
            UpStatUserCurentTates();
        }
    }
    void UpStatUserCurentTates() // 증가된 유저의 스텟을 최종 스텟에 업데이트
    {
        for(int i = 0; i < userTotalStates.Count; i++)
        {
            userTotalStates[i].beneficialEffectMultiplier += userChangingState[i].beneficialEffectMultiplier;
            userTotalStates[i].maxHp += userChangingState[i].chaingingHp * userTotalStates[i].beneficialEffectMultiplier;
            userTotalStates[i].maxDG += userChangingState[i].chaingingDG * userTotalStates[i].beneficialEffectMultiplier;
            userTotalStates[i].maxCritical += userChangingState[i].chaingingCritical * userTotalStates[i].beneficialEffectMultiplier;
            userTotalStates[i].damageMultiplier += userChangingState[i].damageMultiplier * userTotalStates[i].beneficialEffectMultiplier;
            print("유저 정보 : " + i + " 업데이트 성공");
        }
    }
    public void ReciveDamageDataFromTemproy(ulong enemyID, bool isToEnemy, ulong sendUserID ,float damageFromHp, float damagFromDg, float damageFromTakenDg, int numberOfHits) // 데미지 임시 저장 -- > 이를 통해서 분류 
    {
        print("받은 정보 확인 !  : " + enemyID + "/" + isToEnemy + "/" + sendUserID + "/" + damageFromHp + "/" + damagFromDg + "/" + damageFromTakenDg + "/" + numberOfHits);
        userDamaingChangs[(int)sendUserID] = new UserDamaingChang()
        {
            targetMonsterID = enemyID,
            isTargetEnemy = isToEnemy,
            chaningDamaingForHp =  damageFromHp,
            chaningDamaingForDamage = damagFromDg,
            chaningDamaingForTakenDamage = damageFromTakenDg,
            numberOfHits = numberOfHits
        };
        

        ReciveCardEffectDamage();
    }
    public void ReciveCardEffectDamage() // 데미지 정보 저장 -- 준 피해
    {
        int currentTurn = GameManager.Instance.totalTrunNum.Value;
        for(int i = 0; i < userAboutDamages.Count; i++) // 가할 데미지 저장
        {
            float prevTakenDamage = 0f; // 기본값은 0
            int prevIndex = currentTurn - 1;
            // 이전 턴 인덱스가 0보다 크거나 같고, 실제로 리스트에 데이터가 존재할 때만 가져옴
            if (prevIndex >= 0 && prevIndex < userAboutDamages[i].damage.Count)
            {
                prevTakenDamage = userAboutDamages[i].damage[prevIndex].hitTakenDamage;
            }
            else if (userAboutDamages[i].damage.Count > 0)
            {
                // (선택 사항) 만약 턴 계산이 꼬여서 인덱스가 안 맞더라도, 데이터가 있다면 마지막 데이터를 가져오는 안전장치
                prevTakenDamage = userAboutDamages[i].damage[^1].hitTakenDamage;
            }
            userAboutDamages[i].damage.Add(new UserAoubtDamage()
            {
                targetMonsterID = userDamaingChangs[i].targetMonsterID,
                isTargetEnemy = userDamaingChangs[i].isTargetEnemy,
                hitHpDamage = userTotalStates[i].maxHp * userDamaingChangs[i].chaningDamaingForHp,
                hitDGDamage = userTotalStates[i].maxDG * userDamaingChangs[i].chaningDamaingForDamage,
                hitTakenDamage = prevTakenDamage * userDamaingChangs[i].chaningDamaingForTakenDamage,
                numberOfHits = userDamaingChangs[i].numberOfHits,
                cuurentTrun = currentTurn
            });

            bool isCritical = CheckCriticalSuccess(i);
            if(isCritical)
            {
                int lastIndex = userAboutDamages[i].damage.Count - 1;
                userAboutDamages[i].damage[lastIndex].hitHpDamage *= ciriticalMultiplier;
                userAboutDamages[i].damage[lastIndex].hitDGDamage *= ciriticalMultiplier;
                userAboutDamages[i].damage[lastIndex].hitTakenDamage *= ciriticalMultiplier;
            }
        }

        print("데미지저장 성공");
    }
    bool CheckCriticalSuccess(int index)
    {
        float successProbability = userTotalStates[index].currentCritical; 
        float randomValue = UnityEngine.Random.Range(0, 101);
        if (randomValue <= successProbability)
            return true;
        return false;
    }

    public void ReciveHealData(ulong userId, float healAmount)
    {
        print("히이일!!");
        userTotalStates[(int)userId].currentHp += userTotalStates[(int)userId].maxHp * healAmount;
        if(userTotalStates[(int)userId].currentHp >= userTotalStates[(int)userId].maxHp)
            userTotalStates[(int)userId].currentHp = userTotalStates[(int)userId].maxHp;
    }

}
