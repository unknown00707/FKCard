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
    public int damageMultiplier; // 받는 피해
    
}
[System.Serializable]
public class UserDamaingChang
{
    public ulong targetMonsterID;
    public bool isTargetEnemy;
    public float chaningDamaingForHp;
    public float chaningDamaingForDamage;
    public float chaningDamaingForTakenDamage;
}
[System.Serializable]
public class UserAoubtDamage
{
    public ulong targetMonsterID;
    public bool isTargetEnemy;
    public float hitHpDamage; // 준 피해
    public float hitDGDamage;
    public float hitTakenDamage;
    public float inDamage; // 받은 피해
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
    public int damageMultiplier;
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
            userState[i] = new float[3];
            userChangingState.Add(new UserTime());
            userDamaingChangs.Add(new UserDamaingChang());
            userTotalStates.Add(new UserStat());
            userAboutDamages.Add(new UserDamgeGroup());
        }

        for(int i = 0; i < userAboutDamages.Count; i++)
        {
            int j = 0;
            while(j > 50)
            {
                userAboutDamages[i].damage.Add(new UserAoubtDamage());
                j++;
            }
        }
    }

    public void ReciveUsersStat(float hp, float dg, float crip, ulong userID)
    {
        userState[userID][0] = hp;
        userState[userID][1] = dg;
        userState[userID][2] = crip;

        userTotalStates[(int)userID].maxHp = userState[userID][0];
        userTotalStates[(int)userID].maxDG = userState[userID][1];
        userTotalStates[(int)userID].maxCritical = userState[userID][2];

        userTotalStates[(int)userID].currentHp = userTotalStates[(int)userID].maxHp;
        userTotalStates[(int)userID].currentDG = userTotalStates[(int)userID].maxDG;
        userTotalStates[(int)userID].currentCritical = userTotalStates[(int)userID].maxCritical;
    }

    public void ReciveUpStatUserByBuffer(float hp, float damage, float critical, int damageMultiplier, ulong userID , JobManager.Jobs cardType, int cardIndex) // 증가되는 유저 스텟 저장
    {
        if (userID < (ulong)userChangingState.Count)
        {
            userChangingState[(int)userID] = new UserTime()
            {
                chaingingHp = userTotalStates[(int)userID].maxHp*hp,
                chaingingDG = userTotalStates[(int)userID].maxDG*damage,
                chaingingCritical = critical,
                damageMultiplier = damageMultiplier
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
            userTotalStates[i].maxHp += userChangingState[i].chaingingHp;
            userTotalStates[i].maxDG += userChangingState[i].chaingingDG;
            userTotalStates[i].maxCritical += userChangingState[i].chaingingCritical;
            userTotalStates[i].damageMultiplier = userChangingState[i].damageMultiplier;
        }
    }
    public void ReciveDamageDataFromTemproy(ulong enemyID, bool isToEnemy, ulong sendUserID ,float damageFromHp, float damagFromDg, float damageFromTakenDg) // 데미지 임시 저장 -- > 이를 통해서 분류 
    {
        userDamaingChangs[(int)sendUserID].targetMonsterID = enemyID;
        userDamaingChangs[(int)sendUserID].isTargetEnemy = isToEnemy;
        userDamaingChangs[(int)sendUserID].chaningDamaingForHp = damageFromHp;
        userDamaingChangs[(int)sendUserID].chaningDamaingForDamage = damagFromDg;
        userDamaingChangs[(int)sendUserID].chaningDamaingForTakenDamage = damageFromTakenDg;

        ReciveCardEffectDamage();
    }
    public void ReciveCardEffectDamage() // 데미지 정보 저장 -- 준 피해
    {
        for(int i = 0; i < userAboutDamages.Count; i++) // 가할 데미지 저장
        {
            userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].targetMonsterID = userDamaingChangs[i].targetMonsterID;
            userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].isTargetEnemy = userDamaingChangs[i].isTargetEnemy;
            userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].hitHpDamage = userTotalStates[i].maxHp * userDamaingChangs[i].chaningDamaingForHp;
            userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].hitDGDamage = userTotalStates[i].maxDG * userDamaingChangs[i].targetMonsterID;
            userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].hitTakenDamage = userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].hitTakenDamage * userDamaingChangs[i].targetMonsterID;
        
            bool isCritical = CheckCriticalSuccess(i);
            if(isCritical)
            {
                userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].hitHpDamage *= ciriticalMultiplier;
                userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].hitDGDamage *= ciriticalMultiplier;
                userAboutDamages[i].damage[GameManager.Instance.totalTrunNum.Value].hitTakenDamage *= ciriticalMultiplier;
            }
        }
    }

    bool CheckCriticalSuccess(int index)
    {
        float successProbability = userTotalStates[index].currentCritical; 
        float randomValue = UnityEngine.Random.Range(0, 101);
        if (randomValue <= successProbability)
            return true;
        return false;
    }

    public void ReSendTotalDamageToEnemy()
    {
        
    }

}
