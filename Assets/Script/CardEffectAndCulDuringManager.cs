using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class UserTime
{
    public int durTime;
    public float chaingingHp;
    public float chaingingDG;
    public float chaingingCritical;
    
}
[System.Serializable]
public class UserAoubtDamage
{
    public float hitDamage; // 준 피해
    public float inDamage; // 받은 피해
    public int hitObjIndex;
}
[System.Serializable]
public class UserStat
{
    public float hp;
    public float dG;
    public float critical;
    
}
[System.Serializable]
public class UserTimeListWrapper
{
    public List<UserTime> times = new();
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
    public List<UserTimeListWrapper> userChangingState = new(); 
    public List<UserStat> userTotalStates = new(); // 현재 턴의 총 유저들의 스텟
    public List<UserDamgeGroup> userAboutDamages = new(); // 데미지 관련 리스트

    void Awake()
    {
        for(int i = 0; i < 3; i++)
        {
            userState[i] = new float[3];
            userChangingState.Add(new UserTimeListWrapper());
            userTotalStates.Add(new UserStat());
            userAboutDamages.Add(new UserDamgeGroup());
        }
    }

    public void ReciveUsersStat(float hp, float dg, float crip, ulong userID)
    {
        userState[userID][0] = hp;
        userState[userID][1] = dg;
        userState[userID][2] = crip;

        userTotalStates[(int)userID].hp = userState[userID][0];
        userTotalStates[(int)userID].dG = userState[userID][1];
        userTotalStates[(int)userID].critical = userState[userID][2];
    }
    UserTime ReUserTime(int durTime, float hp, float damage, float critical)
    {
        UserTime userTime = new()
        {
            durTime = durTime,
            chaingingHp = hp,
            chaingingDG = damage,
            chaingingCritical = critical
        };

        return userTime;
    }

    public void ReciveCardEffect(int durTime, float hp, float damage, float critical, ulong userID) // 증가되는 유저 스텟 저장
    {
        UserTime userTime = ReUserTime(durTime, hp, damage, critical);

        if (userID < (ulong)userChangingState.Count)
        {
            userChangingState[(int)userID].times.Add(userTime);
            UpStatUserCurentTates(userChangingState[(int)userID].times, userID);
        }
    }

    void UpStatUserCurentTates(List<UserTime> userPlus, ulong userID) // 증가된 유저의 스텟을 최종 스텟에 업데이트
    {
        foreach(UserTime userTime in userPlus)
        {
            if(userTime.durTime == 0)
            {
                userPlus.Remove(userTime);
            }
            else
            {
                userTotalStates[(int)userID].hp += userTime.chaingingHp;
                userTotalStates[(int)userID].dG += userTime.chaingingDG;
                userTotalStates[(int)userID].critical += userTime.chaingingCritical;
            }
            
            userTime.durTime--;
        }
    }

    public void ReciveCardEffectDamage(bool isToUser, float damage, ulong id, int currentTrunNum, int durTime, int enemyID) // 데미지 정보 저장
    {
        UserAoubtDamage userDamage = new ();
        if(isToUser)
            userDamage.inDamage = damage;
        else
            userDamage.hitDamage = damage;
        userDamage.hitObjIndex = enemyID;

        print(userAboutDamages[(int)id].damage.Count() + "/" + currentTrunNum + " : 데이터 저장 시작");        
        if(userAboutDamages[(int)id].damage.Count() == 0 && !isToUser && durTime == 1) // 초기 데미지 저장
        {
            userAboutDamages[(int)id].damage.Add(userDamage);
            print("첫번째 데이터 저장 성공");        
        }
        else if(!isToUser && durTime == 1)
        {
            if(userAboutDamages[(int)id].damage.Count() > currentTrunNum)
                userAboutDamages[(int)id].damage[currentTrunNum].hitDamage += userDamage.hitDamage;
            else
                userAboutDamages[(int)id].damage.Add(userDamage);
            print("즉발 다수 번째 데이터 저장 성공");
        }
        
        else if(userAboutDamages[(int)id].damage.Count() < currentTrunNum && !isToUser && durTime != 1) // 지속피해 구현
        {
            if(userAboutDamages[(int)id].damage.Count() <= currentTrunNum)
            {
                for(int i = 0; i < durTime; i ++)
                    userAboutDamages[(int)id].damage.Add(userDamage);    
            }
            else
            {
                userAboutDamages[(int)id].damage[currentTrunNum].hitDamage += userDamage.hitDamage;
                for(int i = 1; i < durTime; i ++)
                {
                    if(userAboutDamages[(int)id].damage.Count() < currentTrunNum + i)
                    {
                        userAboutDamages[(int)id].damage.Add(userDamage);
                    }
                    else
                    {
                        userAboutDamages[(int)id].damage[currentTrunNum + i].hitDamage += userDamage.hitDamage;
                    }
                }
            }
            
            print("지속피해 데이터 저장 성공");
        }
        else if(userAboutDamages[(int)id].damage.Count() > currentTrunNum && isToUser && durTime == 1) // 받는 데미지 저장
        {
            if(durTime == 1)
                userAboutDamages[(int)id].damage[currentTrunNum].inDamage = userDamage.inDamage;
            else
            {
                userAboutDamages[(int)id].damage[currentTrunNum].inDamage = userDamage.inDamage;
                for(int i = 1; i < durTime; i ++)
                {
                    if(userAboutDamages[(int)id].damage.Count() < currentTrunNum + i)
                    {
                        userAboutDamages[(int)id].damage.Add(userDamage);
                    }
                    else
                    {
                        userAboutDamages[(int)id].damage[currentTrunNum + i].inDamage += userDamage.inDamage;
                    }
                }
            }
            print("받는 데미지 데이터 저장 성공");
        }
        print("데이터 저장 종료");
    }

    public void ReSendTotalDamageToEnemy()
    {
        
    }

}
