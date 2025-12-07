using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
[System.Serializable]
public class UpStateData
{
    public ulong targetUserID;
    public int startTrunNum;
    public int endTrunNum;
    public float upHp;
    public float upDamge;
    public float upCritical;
    public int damageTakenMultiplier;
    public JobManager.Jobs cardType;
    public int cardIndex;
}
[System.Serializable]
public class UpStateGroup
{
    public List<UpStateData> upStateData = new();
}
[System.Serializable]
public class UserHitDamage
{
    public ulong targetMonsterID;
    public bool isTargetEnemy;
    public int startTrunNum;
    public int endTrunNum;
    public float hitHp;
    public float hitDamge;
    public float hitTakenDg;
}
public class PlayerJobSkill : MonoBehaviour
{
    public CardEffectAndCulDuringManager ceacdManager;
    public ChooseEnemyOrTeam chooseEnemyOrTeam;
    public ulong toUserID;
    public ulong toEnemyID;
    public List<UpStateGroup> upStateGroups = new();
    public List<UserHitDamage> userDamaingChangTemproy = new();
    public bool isSingleAttack; // 단일공격임?
    
    
    CardPlayer player;


    void Awake()
    {
        for(int i = 0; i < 3; i++)
        {
            upStateGroups.Add(new UpStateGroup());
            userDamaingChangTemproy.Add(new UserHitDamage());
        }
    }
    void Update()
    {
        if (player == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
        }
    }
    public void ReciveTargetUserIDFromChoose(ulong index, bool isToPlayer)
    {
        if(isToPlayer)
            toUserID = index;
        else
            toEnemyID = index;

    }
    private void UpStateForIDUser(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, int damageMultiplier, JobManager.Jobs cardType, int cardIndex)
    {
        player.UpUserStatTemporary(userId, startDurTimeTrun, endDurTineTrun, hp, dg, critical, damageMultiplier, cardType , cardIndex);
    }
    private void GiveDamageForIDUser(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg)
    {
        player.UpUserDamageTemporary(userId, isForEnemy ,startDurTimeTrun, endDurTineTrun, hp, dg, takenDg);
    }
    public void ReciveUpUserStatTemproy(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, int damageMultiplier, ulong sendUserID, JobManager.Jobs cardType, int cardIndex)
    {
        upStateGroups[(int)sendUserID].upStateData.Add(new UpStateData()
        {
           targetUserID = userId, startTrunNum = startDurTimeTrun, endTrunNum = endDurTineTrun, 
           upHp = hp, upDamge = dg, upCritical = critical, damageTakenMultiplier =  damageMultiplier,
           cardType = cardType, cardIndex = cardIndex
        });
    }

    public void ReciveUpDamageTemproy(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, ulong sendUserID)
    {
        userDamaingChangTemproy[(int)sendUserID].targetMonsterID = userId;
        userDamaingChangTemproy[(int)sendUserID].isTargetEnemy = isForEnemy;
        userDamaingChangTemproy[(int)sendUserID].startTrunNum = startDurTimeTrun;
        userDamaingChangTemproy[(int)sendUserID].endTrunNum = endDurTineTrun;
        userDamaingChangTemproy[(int)sendUserID].hitHp = hp;
        userDamaingChangTemproy[(int)sendUserID].hitDamge = dg;
        userDamaingChangTemproy[(int)sendUserID].hitTakenDg = takenDg;
    }
    public void ReciveUpUserStateByBufferCard()
    {
        // 적용될 버프의 우선순위 분류 및 적용
        for(int i = 0; i < upStateGroups.Count; i++)
        {
            List<UpStateData> upStateDatas = new();
            List<float> propertyNum = new();
            for (int j = 0; j < upStateGroups.Count; j++)
            {
                foreach(UpStateData upState in upStateGroups[j].upStateData)
                {
                    if((int)upState.targetUserID == i)
                    {
                        if((upState.startTrunNum <= GameManager.Instance.totalTrunNum.Value) 
                        && (GameManager.Instance.totalTrunNum.Value <= upState.endTrunNum))
                        {
                            propertyNum.Add((upState.upHp + upState.upDamge + upState.upCritical + upState.damageTakenMultiplier) / (float)100);
                            upStateDatas.Add(upState);    
                        }
                    }
                }
            }

            for(int j = 0; j < propertyNum.Count; j++)
            {
                float maxValue = propertyNum.Max();
                int maxIndex = propertyNum.IndexOf(maxValue);
                player.UpUserStatIFO(i, upStateDatas[maxIndex].upHp, upStateDatas[maxIndex].upDamge, 
                upStateDatas[maxIndex].upCritical, upStateDatas[maxIndex].damageTakenMultiplier, upStateDatas[maxIndex].cardType, upStateDatas[maxIndex].cardIndex);   
                propertyNum.RemoveAt(maxIndex);
                upStateDatas.RemoveAt(maxIndex);
            }
        }
    }
    public void ReciveUpUserDamage()
    {
        for(int i = 0; i < userDamaingChangTemproy.Count; i++)
        {
            if((userDamaingChangTemproy[i].startTrunNum <= GameManager.Instance.totalTrunNum.Value) && (GameManager.Instance.totalTrunNum.Value <= userDamaingChangTemproy[i].endTrunNum))
            {
                player.UpDamageUserInOut(userDamaingChangTemproy[i].targetMonsterID, userDamaingChangTemproy[i].isTargetEnemy, 
                userDamaingChangTemproy[i].hitHp,userDamaingChangTemproy[i].hitDamge, userDamaingChangTemproy[i].hitTakenDg);
            }
        }
    }
    public void DefenderSkills(int index)
    {
        int currentTrun = GameManager.Instance.totalTrunNum.Value;
        switch(index)
        {
            case 0:
                player.ReciveSignIsBufferCard(true);
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                UpStateForIDUser(toUserID, currentTrun, currentTrun + 1, (float)15/100, 0, 0, 100, JobManager.Jobs.defender, 0);
                break;
            case 1:
                isSingleAttack = true;
                player.ReciveSignIsBufferCard(false);
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                GiveDamageForIDUser(toEnemyID, true, currentTrun, currentTrun + 1, (float)10/100, 0, 0);
                break;
            case 2:
                player.ReciveSignIsBufferCard(true);
                UpStateForIDUser(toUserID, currentTrun, -1, (float)5/100, 0, 0, 100, JobManager.Jobs.defender, 2); // -1 mean infinite
                break;
            case 3:
                isSingleAttack = true;
                player.ReciveSignIsBufferCard(false);
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                GiveDamageForIDUser(toEnemyID, true, currentTrun, currentTrun + 1, 0, 0, (float)50/100);
                break;
            case 4:
                player.ReciveSignIsBufferCard(true);
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                UpStateForIDUser(toUserID, currentTrun + 1 , currentTrun + 2, (float)50/100, 100, 0, 0, JobManager.Jobs.defender, 4);
                break;
            case 5:
              
                break;
        }
    }





}

