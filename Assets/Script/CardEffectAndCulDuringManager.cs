using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
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
    public TurnManager turnManager;
    public NetworkSessionManager sessionManager;
    public float[][] userState = new float[3][]; // 초기 유저의 정보 -> 안 변함 -> 스테이지 변하면 변함
    // 유저의 지속 기간에 따른 스텟 변화 저장
    public List<UserTime> userChangingState = new(); 
    public List<UserDamaingChang> userDamaingChangs = new();
    public List<UserStat> userTotalStates = new(); // 현재 턴의 총 유저들의 스텟
    public List<UserDamgeGroup> userAboutDamages = new(); // 데미지 관련 리스트
    public List<TakenDamage> userTakenDamageGroups = new();
    CardPlayer player;
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
    }

    void Update()
    {
        if (player == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
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

    public void ReciveUpStatUserByBuffer(UpStateData upStateData) // 증가되는 유저 스텟 저장
    {
        print("유저 정보 저장 시작");
        if (upStateData.targetUserID < (ulong)userChangingState.Count)
        {
            userChangingState[(int)upStateData.targetUserID] = new UserTime()
            {
                chaingingHp = userTotalStates[(int)upStateData.targetUserID].maxHp*upStateData.upHp,
                chaingingDG = userTotalStates[(int)upStateData.targetUserID].maxDG*upStateData.upDamge,
                chaingingCritical = upStateData.upCritical,
                damageMultiplier = upStateData.damageTakenMultiplier,
                beneficialEffectMultiplier = upStateData.beneficialEffectMultiplier
            };
            switch(upStateData.cardType)
            {
                case JobManager.Jobs.defender :
                    if(upStateData.cardIndex == 4)
                    {
                        var takenDamage = FindTakenDamgeListByFilter(userTakenDamageGroups, (int)upStateData.targetUserID, turnManager.GiveTurnValue() -1);
                        float takenDamageTotal = 0;
                        if(takenDamage.Count == 0)
                        {
                            takenDamageTotal = 0;
                        }
                        else
                        {
                            foreach(TakenDamage takenDG in takenDamage)
                            {
                                takenDamageTotal += takenDG.takenDamage;
                            }
                        }
                        userChangingState[(int)upStateData.targetUserID].chaingingHp = takenDamageTotal * upStateData.upHp;
                    }
                    break; 
            }
            UpStatUserCurentTates((int)upStateData.targetUserID);
        }
    }
    void UpStatUserCurentTates(int targetUserID) // 증가된 유저의 스텟을 "최종(max)" 스텟에 업데이트 != "현재 스텟 변화 x !!"
    {
        userTotalStates[targetUserID].beneficialEffectMultiplier += userChangingState[targetUserID].beneficialEffectMultiplier; // 이로운 효과 먼저 계산 필수
        
        float userChangingHp = userChangingState[targetUserID].chaingingHp * userTotalStates[targetUserID].beneficialEffectMultiplier;
        float userChangingDG = userChangingState[targetUserID].chaingingDG * userTotalStates[targetUserID].beneficialEffectMultiplier;
        float userChangingCritical = userChangingState[targetUserID].chaingingCritical * userTotalStates[targetUserID].beneficialEffectMultiplier;
        float userChangingDamageMultiplier = userChangingState[targetUserID].damageMultiplier * userTotalStates[targetUserID].beneficialEffectMultiplier;

        userTotalStates[targetUserID].maxHp += userChangingHp;
        userTotalStates[targetUserID].maxDG += userChangingDG;
        userTotalStates[targetUserID].maxCritical += userChangingCritical;
        userTotalStates[targetUserID].damageMultiplier += userChangingDamageMultiplier;
        
        // 최대 스텟이 증가된 만큼 현재 스텟도 같은 양 만큼 (같은 비율로 만들기 위해서) 증가
        userTotalStates[targetUserID].currentHp += userChangingHp;
        userTotalStates[targetUserID].currentDG += userChangingDG;
        userTotalStates[targetUserID].currentCritical += userChangingCritical;
        print("유저 정보 : " + targetUserID + " 업데이트 성공");
        
    }
    public void ReciveDamageDataFromTemproy(UserHitDamage package, ulong sendUserID) // 데미지 임시 저장 -- > 이를 통해서 분류 
    {
        print("받은 정보 확인 !  : " + package.targetMonsterID + "/" + package.isTargetEnemy + "/" + package.hitHp + 
        "/" +  package.hitDamge + "/" + package.hitTakenDg + "/" + package.numberOfHits + " 보낸 놈 : " + sendUserID);
        userDamaingChangs[(int)sendUserID] = new UserDamaingChang()
        {
            targetMonsterID = package.targetMonsterID,
            isTargetEnemy = package.isTargetEnemy,
            chaningDamaingForHp =  package.hitHp,
            chaningDamaingForDamage = package.hitDamge,
            chaningDamaingForTakenDamage = package.hitTakenDg,
            numberOfHits = package.numberOfHits
        };
        

        ReciveCardEffectDamage((int)sendUserID);
    }
    public void ReciveCardEffectDamage(int sendUserID) // 데미지 정보 저장 -- 준 피해
    {
        int currentTurn = turnManager.GiveTurnValue();
        // for(int i = 0; i < userAboutDamages.Count; i++) // 가할 데미지 저장
        // {
            float prevTakenDamage = 0f; // 기본값은 0
            int prevIndex = currentTurn - 1;
            // 이전 턴 인덱스가 0보다 크거나 같고, 실제로 리스트에 데이터가 존재할 때만 가져옴
            if (prevIndex >= 0 && prevIndex < userAboutDamages[sendUserID].damage.Count)
            {
                prevTakenDamage = userAboutDamages[sendUserID].damage[prevIndex].hitTakenDamage;
            }
            else if (userAboutDamages[sendUserID].damage.Count > 0)
            {
                // (선택 사항) 만약 턴 계산이 꼬여서 인덱스가 안 맞더라도, 데이터가 있다면 마지막 데이터를 가져오는 안전장치
                prevTakenDamage = userAboutDamages[sendUserID].damage[^1].hitTakenDamage;
            }
            userAboutDamages[sendUserID].damage.Add(new UserAoubtDamage()
            {
                targetMonsterID = userDamaingChangs[sendUserID].targetMonsterID,
                isTargetEnemy = userDamaingChangs[sendUserID].isTargetEnemy,
                hitHpDamage = userTotalStates[sendUserID].maxHp * userDamaingChangs[sendUserID].chaningDamaingForHp,
                hitDGDamage = userTotalStates[sendUserID].maxDG * userDamaingChangs[sendUserID].chaningDamaingForDamage,
                hitTakenDamage = prevTakenDamage * userDamaingChangs[sendUserID].chaningDamaingForTakenDamage,
                numberOfHits = userDamaingChangs[sendUserID].numberOfHits,
                cuurentTrun = currentTurn
            });

            bool isCritical = CheckCriticalSuccess(sendUserID);
            if(isCritical)
            {
                int lastIndex = userAboutDamages[sendUserID].damage.Count - 1;
                userAboutDamages[sendUserID].damage[lastIndex].hitHpDamage *= ciriticalMultiplier;
                userAboutDamages[sendUserID].damage[lastIndex].hitDGDamage *= ciriticalMultiplier;
                userAboutDamages[sendUserID].damage[lastIndex].hitTakenDamage *= ciriticalMultiplier;
            }
        // }

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

    public void RequsetDownUserCurrentStatFromDamage(UserAoubtDamage givenDamge)
    {
        int realTurn = turnManager.GiveTurnValue();
        int targetUserID = (int)givenDamge.targetMonsterID;
        float totalDamage = givenDamge.hitHpDamage + givenDamge.hitDGDamage;
        
        
        List<TakenDamage> takenDamages  = FindTakenDamgeListByFilter(userTakenDamageGroups, targetUserID, realTurn);
        TakenDamage taken = new()
        {
            id = targetUserID,
            takenDamage = totalDamage,
            currentTurn = realTurn  
        };

        for(int i = 0; i < givenDamge.numberOfHits; i++)
        {
            if(!sessionManager.GivePlayerAliveBool(targetUserID))
            {
                int anotherTargetID = targetUserID;
                while(anotherTargetID == targetUserID)
                {
                    anotherTargetID = FindAnyAlivePlayer();
                }
                targetUserID = anotherTargetID;
            }
            userTotalStates[targetUserID].currentHp -= totalDamage * userTotalStates[targetUserID].damageMultiplier;
            if(takenDamages.Count == 0)
                takenDamages.Add(taken);
            else
                takenDamages[^1].takenDamage += totalDamage;    

            if(userTotalStates[targetUserID].currentHp <= 0 && sessionManager.GivePlayerAliveBool(targetUserID))
                player.RequsetDieSignal(targetUserID);
        }
        
    }

    public List<TakenDamage> FindTakenDamgeListByFilter(List<TakenDamage> orignList, int id, int turn)
    {
        return orignList.Where(data => data.id == id).Where(data => data.currentTurn == turn).ToList(); 
    }

    int FindAnyAlivePlayer()
    {
        return UnityEngine.Random.Range(0, sessionManager.GivePlayerTotalNum());
    }

    public void ReciveHealData(ulong userId, float healAmount)
    {
        print("히이일!!");
        userTotalStates[(int)userId].currentHp += userTotalStates[(int)userId].maxHp * healAmount;
        if(userTotalStates[(int)userId].currentHp >= userTotalStates[(int)userId].maxHp)
            userTotalStates[(int)userId].currentHp = userTotalStates[(int)userId].maxHp;
    }

}
