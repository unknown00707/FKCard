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
    public float damageTakenMultiplier;
    public float beneficialEffectMultiplier;
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
    public int numberOfHits;
}
[System.Serializable]
public class UserDamaing
{
    public List<UserHitDamage> userHitDamages = new();
}
[System.Serializable]
public class UserHealData
{
    public ulong givenUserId;
    public int startTrunNum;
    public int endDurTineTrun;
    public float healAmount;
}
public class PlayerJobSkill : MonoBehaviour
{
    [Header("Manager")]
    public CardEffectAndCulDuringManager ceacdManager;
    public ChooseEnemyOrTeam chooseEnemyOrTeam;
    public CardSpaceCheck cardSpaceCheck;
    public EnemyCulGroup enemyCulGroup;
    [Header("Data")]
    public ulong toUserID;
    public ulong toEnemyID;
    public List<UpStateGroup> upStateGroups = new();
    public List<UserDamaing> userDamaingChangTemproy = new();
    public List<UserHealData> userHealDataTemproy = new();
    public bool isSingleAttack; // 단일공격임?
    public JobManager.Jobs job;
    public int cardIndex;
    
    CardPlayer player;


    void Awake()
    {
        for(int i = 0; i < 3; i++)
        {
            upStateGroups.Add(new UpStateGroup());
            userDamaingChangTemproy.Add(new UserDamaing());
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
    //버프 임시 저장 보내기
    private void UpStateForIDUser(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
        print("버프 임시 저장 보내기!");
        player.UpUserStatTemporary(userId, startDurTimeTrun, endDurTineTrun, hp, dg, critical, damageMultiplier, beneficialEffectMultiplier, cardType , cardIndex);
    }
    // 데미지 임시 저장 보내기
    private void GiveDamageForIDUser(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits)
    {
        print("데미지 임시 저장 보내기!");
        player.UpUserDamageTemporary(userId, isForEnemy ,startDurTimeTrun, endDurTineTrun, hp, dg, takenDg, numberOfHits);
    }
    //버프 임시 저장
    public void ReciveUpUserStatTemproy(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier, ulong sendUserID, JobManager.Jobs cardType, int cardIndex)
    {
        upStateGroups[(int)sendUserID].upStateData.Add(new UpStateData()
        {
           targetUserID = userId, startTrunNum = startDurTimeTrun, endTrunNum = endDurTineTrun, 
           upHp = hp, upDamge = dg, upCritical = critical, damageTakenMultiplier =  damageMultiplier,
           cardType = cardType, cardIndex = cardIndex, beneficialEffectMultiplier = beneficialEffectMultiplier
        });
        print("버프 임시 저장!");
    }
    //데미지 저장
    public void ReciveUpDamageTemproy(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits, ulong sendUserID)
    {
        userDamaingChangTemproy[(int)sendUserID].userHitDamages.Add(new UserHitDamage()
        {
            targetMonsterID = userId,
            isTargetEnemy = isForEnemy,
            startTrunNum = startDurTimeTrun,
            endTrunNum = endDurTineTrun,
            hitHp = hp,
            hitDamge = dg,
            hitTakenDg = takenDg,
            numberOfHits = numberOfHits
        });
        
        print("데미지 임시 저장!");
    }
    // 버프 데이터 보내기
    public void ReciveUpUserStateByBufferCard()
    {
        print("버프 저장 보내기!");
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
                        if(upState.startTrunNum <= GameManager.Instance.totalTrunNum.Value)
                        {
                            if((upState.endTrunNum == -1) || (GameManager.Instance.totalTrunNum.Value <= upState.endTrunNum))
                            {// -1 mean infinit
                                print("Success Accoes Income Buffe . . .");
                                propertyNum.Add((upState.upHp + upState.upDamge + upState.upCritical + upState.damageTakenMultiplier) / 100);
                                upStateDatas.Add(upState);     
                            }
                               
                        }
                    }
                }
            }

            for(int j = 0; j < propertyNum.Count; j++)
            {
                float maxValue = propertyNum.Max();
                int maxIndex = propertyNum.IndexOf(maxValue);
                player.UpUserStatIFO(i, upStateDatas[maxIndex].upHp, upStateDatas[maxIndex].upDamge, 
                    upStateDatas[maxIndex].upCritical, upStateDatas[maxIndex].damageTakenMultiplier, upStateDatas[maxIndex].beneficialEffectMultiplier,
                    upStateDatas[maxIndex].cardType, upStateDatas[maxIndex].cardIndex);   
                propertyNum.RemoveAt(maxIndex);
                upStateDatas.RemoveAt(maxIndex);
                print("Success Send Data . . .");
            }
        }
    }
    // 데미지 저장 보내기
    public void ReciveUpUserDamage()
    {
        print("데미지 저장 보내기!");
        for(int i = 0; i < userDamaingChangTemproy.Count; i++)
        {
            for(int j = 0; j < userDamaingChangTemproy[i].userHitDamages.Count; j++)
            {
                if(userDamaingChangTemproy[i].userHitDamages[j].startTrunNum <= GameManager.Instance.totalTrunNum.Value)
                {
                    if((GameManager.Instance.totalTrunNum.Value <= userDamaingChangTemproy[i].userHitDamages[j].endTrunNum) || GameManager.Instance.totalTrunNum.Value == -1) 
                        player.UpDamageUserInOut(userDamaingChangTemproy[i].userHitDamages[j].targetMonsterID, userDamaingChangTemproy[i].userHitDamages[j].isTargetEnemy, 
                            userDamaingChangTemproy[i].userHitDamages[j].hitHp,userDamaingChangTemproy[i].userHitDamages[j].hitDamge, 
                            userDamaingChangTemproy[i].userHitDamages[j].hitTakenDg, userDamaingChangTemproy[i].userHitDamages[j].numberOfHits);
                }    
            }
        }
    }
    // 힐량 임시 보내기
    public void GiveHealToTemproy(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        player.ToHealTemporary(userId, startDurTimeTrun, endDurTineTrun, healAmount);
    }
    // 힐량 임시 저장
    public void ReciveUpHealDataTemproy(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        userHealDataTemproy.Add(new UserHealData()
        {
            givenUserId = userId,
            startTrunNum = startDurTimeTrun, 
            endDurTineTrun = endDurTineTrun,
            healAmount = healAmount
        });
    }
    // 힐량 보내기
    public void RequsetHealDataSend()
    {
        for(int i = 0; i < userHealDataTemproy.Count; i++)
        {
            if(userHealDataTemproy[i].startTrunNum <= GameManager.Instance.totalTrunNum.Value)
            {   
                if(GameManager.Instance.totalTrunNum.Value < userHealDataTemproy[i].endDurTineTrun)
                    ceacdManager.ReciveHealData(userHealDataTemproy[i].givenUserId, userHealDataTemproy[i].healAmount);
            }    
        }
    }
    // 모든 스킬의 트리거 함수
    public void TriggerSkillFromChoosEnemyOrTeam()
    {
        print("플레이어 스킬 스크립트 - 이벤트 트리거 작동!");
        int currentTrun = GameManager.Instance.totalTrunNum.Value;
        switch(job)
        {
            case JobManager.Jobs.defender:
                switch(cardIndex)
                {
                    case 0:
                        UpStateForIDUser(toUserID, currentTrun, currentTrun + 1, (float)15/100, 0, 0, 0, 0 , job, cardIndex);
                        break;
                    case 1:
                        GiveDamageForIDUser(toEnemyID, true, currentTrun, currentTrun + 1, (float)10/100, 0, 0, 1);
                        break;
                    case 2:
                        UpStateForIDUser(toUserID, currentTrun, -1, (float)5/100, 0, 0, 0, 0 ,job, cardIndex); // -1 mean infinite
                        break;
                    case 3:
                        GiveDamageForIDUser(toEnemyID, true, currentTrun + 1, currentTrun + 2, 0, 0, (float)50/100, 1);
                        break;
                    case 4:
                        UpStateForIDUser(toUserID, currentTrun + 1 , currentTrun + 2, (float)50/100, 0, 0, 0, 0 ,job, cardIndex);
                        break;
                    case 5:
                        UpStateForIDUser(toUserID, currentTrun , currentTrun + 3, 0, 0, 0, 0.2f, 0 ,job,cardIndex);
                        break;
                }
                break;
            case JobManager.Jobs.knight:
                switch(cardIndex)
                {
                    case 0:
                        GiveDamageForIDUser(toEnemyID, true, currentTrun, currentTrun + 1, 0, (float)50/100, 0, 2);
                        break;
                    case 1:
                        GiveDamageForIDUser(toEnemyID, true, currentTrun, currentTrun + 1, 0, (float)120/100, 0, 1);
                        break;
                    case 2:
                        GiveDamageForIDUser(toEnemyID, true, currentTrun, -1, 0, (float)20/100, 0, 1);
                        break;
                    case 3:
                        cardSpaceCheck.WaitToPrivateAblieTrunFC(currentTrun + 4); // ex : 3턴간 행동 불가 = +4
                        GiveDamageForIDUser(toEnemyID, true, currentTrun + 3, currentTrun + 4, 0, (float)100/100, 0, 3);
                        break;
                    case 4:
                        cardSpaceCheck.WaitToPrivateAblieTrunFC(currentTrun + 2); // ex : 1턴간 행동 불가 = +2
                        UpStateForIDUser(toUserID, currentTrun + 1, currentTrun + 2, 0, 0, 40f, 0, 0 ,job, cardIndex);
                        break;
                    case 5:
                        GiveDamageForIDUser(toEnemyID, true, currentTrun, currentTrun +1, 0, 0, (float)30/100, 1);
                        break;
                }
                break;
            case JobManager.Jobs.wizard:
                switch(cardIndex)
                {
                    case 0:
                        for(int i = 0; i < enemyCulGroup.enemyPrefabs.Count(); i++)
                        {
                            if(enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                                GiveDamageForIDUser((ulong)i, true, currentTrun, currentTrun + 1, 0, (float)70/100, 0, 1);
                        }
                        break;
                    case 1:
                        UpStateForIDUser(toUserID, currentTrun, currentTrun + 4, 0, (float)30/100, 0, 0, 0 ,job, cardIndex);
                        break;
                    case 2:
                        GiveDamageForIDUser(toEnemyID, true, currentTrun, currentTrun + 6, 0, (float)30/100, 0, 1);
                        break;
                    case 3:
                        GiveDamageForIDUser(toUserID, false, currentTrun, currentTrun + 1, (float)50/100, 0, 0, 1);
                        for(int i = 0; i < enemyCulGroup.enemyPrefabs.Count(); i++)
                        {
                            if(enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                                GiveDamageForIDUser((ulong)i, true, currentTrun, currentTrun + 1, 0, (float)150/100, 0, 1);
                        }
                        break;
                    case 4:
                        for(int i = 0; i < enemyCulGroup.enemyPrefabs.Count(); i++)
                        {
                            if(enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                                GiveDamageForIDUser((ulong)i, true, currentTrun, currentTrun + 1, 0, (float)300/100, 0, 1);
                        }
                        for(int i = 0; i < GameManager.Instance.totalTrunNum.Value; i++)
                        {
                            print("팀 공격!");
                            GiveDamageForIDUser((ulong)i, false, currentTrun, currentTrun + 1, (float)30/100, 0, 0, 1);
                        }
                        break;
                }
                break;
            case JobManager.Jobs.healler:
                switch(cardIndex)
                {
                    case 0:
                        GiveHealToTemproy(toUserID, currentTrun, currentTrun + 1, (float)15/100);
                        break;
                    case 1:
                        for(int i = 0; i < GameManager.Instance.totalTrunNum.Value; i++)
                        {
                            GiveHealToTemproy((ulong)i, currentTrun, currentTrun + 1, (float)7.5/100);
                        }
                        break;
                    case 2:
                        // 자신이 받을 데미지를 아군 한명에게 75%위력으로 대타세우기
                        break;
                    case 3:
                        for(int i = 0; i < GameManager.Instance.totalTrunNum.Value; i++)
                        {
                            GiveHealToTemproy((ulong)i, currentTrun, currentTrun + 1, (float)Random.Range(3f,16f)/100);
                        }
                        break;
                    case 4:
                        for(int i = 0; i < GameManager.Instance.totalTrunNum.Value; i++)
                        {
                            UpStateForIDUser((ulong)i, currentTrun, currentTrun +1 , (float)5/100 , 0 , 0 , 0 , 0 ,job ,cardIndex);
                        }
                        break;
                    case 5:
                        // 아군 한명 30%체력으로 부활
                        break;
                    case 6:
                        GiveHealToTemproy(toUserID, currentTrun, -1, (float)5/100);
                        break;
                    case 7:
                        UpStateForIDUser(toUserID, currentTrun, currentTrun + 4 , 0 , 0 , 0 , 0 , 0.5f ,job ,cardIndex);
                        break;
                }
                break;
        }

        // 직업 카드 스킬 저장 후 카드 발동 준비 됨
        player.ReciveSignCardEffectReady(false); // -> 배치 완료해서 배치 할 곳 없음의 이미 --> 턴 넘길 준비
    }
    public void DefenderSkills(int index)
    {
        int currentTrun = GameManager.Instance.totalTrunNum.Value;
        switch(index)
        {
            case 0:
                job = JobManager.Jobs.defender;
                cardIndex = 0;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 1:
                job = JobManager.Jobs.defender;
                cardIndex = 1;
                //isSingleAttack = true;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
            case 2:
                job = JobManager.Jobs.defender;
                cardIndex = 2;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 3:
                job = JobManager.Jobs.defender;
                cardIndex = 3;
                isSingleAttack = true;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
            case 4:
                job = JobManager.Jobs.defender;
                cardIndex = 4;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 5:
                job = JobManager.Jobs.defender;
                cardIndex = 5;
                for(int i = 0; i < GameManager.Instance.playerTotalNum.Value; i++)
                {
                    if((ulong)i != NetworkManager.Singleton.LocalClientId)
                    {
                        UpStateForIDUser((ulong)i, currentTrun , currentTrun + 3, 0, (float)15/100, 0, 1.2f, 0, job, cardIndex);
                    }
                }
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
        }
    }
    public void KnightSkills(int index)
    {
        switch(index)
        {
            case 0:
                job = JobManager.Jobs.knight;
                cardIndex = 0;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
            case 1:
                job = JobManager.Jobs.knight;
                cardIndex = 1;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
            case 2:
                job = JobManager.Jobs.knight;
                cardIndex = 2;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
            case 3:
                job = JobManager.Jobs.knight;
                cardIndex = 3;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
            case 4:
                job = JobManager.Jobs.knight;
                cardIndex = 4;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 5:
                job = JobManager.Jobs.knight;
                cardIndex = 5;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
        }
    }
    public void WizardSkills(int index)
    {
        switch(index)
        {
            case 0:
                job = JobManager.Jobs.wizard;
                cardIndex = 0;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 1:
                job = JobManager.Jobs.wizard;
                cardIndex = 1;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 2:
                job = JobManager.Jobs.wizard;
                cardIndex = 2;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(false, false);
                break;
            case 3:
                job = JobManager.Jobs.wizard;
                cardIndex = 3;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 4:
                job = JobManager.Jobs.wizard;
                cardIndex = 4;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
        }
    }
    public void HeaderSkills(int index)
    {
        switch(index)
        {
            case 0:
                job = JobManager.Jobs.healler;
                cardIndex = 0;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, false);
                break;
            case 1:
                job = JobManager.Jobs.healler;
                cardIndex = 1;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 2:
                job = JobManager.Jobs.healler;
                cardIndex = 2;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 3:
                job = JobManager.Jobs.healler;
                cardIndex = 3;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 4:
                job = JobManager.Jobs.healler;
                cardIndex = 4;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 5:
                job = JobManager.Jobs.healler;
                cardIndex = 5;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 6:
                job = JobManager.Jobs.healler;
                cardIndex = 6;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 7:
                job = JobManager.Jobs.healler;
                cardIndex = 7;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, false);
                break;
        }
    }



}

