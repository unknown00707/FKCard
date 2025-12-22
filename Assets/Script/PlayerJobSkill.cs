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

public class PlayerJobSkill : NetworkBehaviour
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
    public bool isSingleAttack; 
    public JobManager.Jobs job;
    public int cardIndex;
    
    private CardPlayer player;
    
    // 💡 핵심: 카드 효과를 저장할 Dictionary (전략 패턴 저장소)
    private Dictionary<(JobManager.Jobs, int), ICardEffect> cardEffects;

    void Awake()
    {
        for(int i = 0; i < 3; i++)
        {
            upStateGroups.Add(new UpStateGroup());
            userDamaingChangTemproy.Add(new UserDamaing());
        }

        // 💡 초기화: 모든 카드 효과를 Dictionary에 등록
        InitializeCardEffects();
    }
    private void InitializeCardEffects()
    {
        cardEffects = new Dictionary<(JobManager.Jobs, int), ICardEffect>
        {
            // --- 방어자 ---
            { (JobManager.Jobs.defender, 0), new DefenderCard0Effect() },
            { (JobManager.Jobs.defender, 1), new DefenderCard1Effect() },
            { (JobManager.Jobs.defender, 2), new DefenderCard2Effect() },
            { (JobManager.Jobs.defender, 3), new DefenderCard3Effect() },
            { (JobManager.Jobs.defender, 4), new DefenderCard4Effect() },
            { (JobManager.Jobs.defender, 5), new DefenderCard5Effect() },
            
            // --- 기사 ---
            { (JobManager.Jobs.knight, 0), new KnightCard0Effect() },
            { (JobManager.Jobs.knight, 1), new KnightCard1Effect() },
            { (JobManager.Jobs.knight, 2), new KnightCard2Effect() },
            { (JobManager.Jobs.knight, 3), new KnightCard3Effect() },
            { (JobManager.Jobs.knight, 4), new KnightCard4Effect() },
            { (JobManager.Jobs.knight, 5), new KnightCard5Effect() },

            // --- 마법사 ---
            { (JobManager.Jobs.wizard, 0), new WizardCard0Effect() },
            { (JobManager.Jobs.wizard, 1), new WizardCard1Effect() },
            { (JobManager.Jobs.wizard, 2), new WizardCard2Effect() },
            { (JobManager.Jobs.wizard, 3), new WizardCard3Effect() },
            { (JobManager.Jobs.wizard, 4), new WizardCard4Effect() },

            // --- 힐러 ---
            { (JobManager.Jobs.healer, 0), new HealerCard0Effect() },
            { (JobManager.Jobs.healer, 1), new HealerCard1Effect() },
            // 2번, 5번은 미구현 상태라 제외
            { (JobManager.Jobs.healer, 3), new HealerCard3Effect() },
            { (JobManager.Jobs.healer, 4), new HealerCard4Effect() },
            { (JobManager.Jobs.healer, 6), new HealerCard6Effect() },
            { (JobManager.Jobs.healer, 7), new HealerCard7Effect() },
        };
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
        if(isToPlayer) toUserID = index;
        else toEnemyID = index;
    }
    [ServerRpc]
    public void UpStateForIDUserServerRpc(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
        print("버프 임시 저장 보내기!");
        GameManager.Instance.InStorUserUpStatTemproy(userId, startDurTimeTrun, endDurTineTrun, hp, dg, critical, damageMultiplier, beneficialEffectMultiplier,OwnerClientId, cardType, cardIndex);
    }
    [ServerRpc]
    public void GiveDamageForIDUserServerRpc(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits)
    {
        print("데미지 임시 저장 보내기!");
        GameManager.Instance.InStorUserDamageTemproy(userId, isForEnemy, startDurTimeTrun, endDurTineTrun, hp, dg, takenDg, numberOfHits, OwnerClientId);
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
    [ServerRpc]
    public void ReciveUpUserStateByBufferCardServerRpc()
    {
        print("버프 저장 보내기!");
        // 적용될 버프의 우선순위 분류 및 적용
        for(int i = 0; i < upStateGroups.Count; i++)
        {
            List<UpStateData> upStateDatas = new();
            List<float> propertyNum = new();
           
            // foreach(UpStateData upState in upStateGroups[i].upStateData)
            // {
            //     if((int)upState.targetUserID == i)
            //     {
            //         if(upState.startTrunNum <= GameManager.Instance.totalTrunNum.Value)
            //         {
            //             if((upState.endTrunNum == -1) || (GameManager.Instance.totalTrunNum.Value <= upState.endTrunNum))
            //             {// -1 mean infinit
            //                 print("Success Accoes Income Buffe . . .");
            //                 propertyNum.Add((upState.upHp + upState.upDamge + upState.upCritical + upState.damageTakenMultiplier) / 100);
            //                 upStateDatas.Add(upState);     
            //             }
            //             else 
            //                 upStateGroups[i].upStateData.Remove(upState);
            //         }
            //         else 
            //             upStateGroups[i].upStateData.Remove(upState);
            //     }
            // }

            var validBuffs = upStateGroups.SelectMany(group => group.upStateData)
                .Where(upState => (int)upState.targetUserID == i &&
                                  upState.startTrunNum <= GameManager.Instance.totalTrunNum.Value &&
                                  (upState.endTrunNum == -1 || GameManager.Instance.totalTrunNum.Value <= upState.endTrunNum))
                .ToList();
            foreach(var buff in validBuffs)
            {
                print($"Valid Buff for User {i}: HP +{buff.upHp}, Damage +{buff.upDamge}, Critical +{buff.upCritical}, Damage Taken Multiplier +{buff.damageTakenMultiplier}");
                propertyNum.Add((buff.upHp + buff.upDamge + buff.upCritical + buff.damageTakenMultiplier) / 100);
                upStateDatas.Add(buff);  
            }

            // for(int j = 0; j < propertyNum.Count; j++)
            // {
            //     float maxValue = propertyNum.Max();
            //     int maxIndex = propertyNum.IndexOf(maxValue);
            //     GameManager.Instance.InUserUpStat(upStateDatas[maxIndex].upHp, upStateDatas[maxIndex].upDamge, 
            //         upStateDatas[maxIndex].upCritical, upStateDatas[maxIndex].damageTakenMultiplier, upStateDatas[maxIndex].beneficialEffectMultiplier, (ulong)i,
            //         upStateDatas[maxIndex].cardType, upStateDatas[maxIndex].cardIndex);   
            //     propertyNum.RemoveAt(maxIndex);
            //     upStateDatas.RemoveAt(maxIndex);
            //     print("Success Send Data . . .");
            // }
            int propertyNumCount = propertyNum.Count;
            for(int j = 0; j < propertyNumCount; j++)
            {
                float maxValue = propertyNum.Max();
                int maxIndex = propertyNum.IndexOf(maxValue);
                GameManager.Instance.InUserUpStat(validBuffs[maxIndex].upHp, validBuffs[maxIndex].upDamge, 
                    validBuffs[maxIndex].upCritical, validBuffs[maxIndex].damageTakenMultiplier, validBuffs[maxIndex].beneficialEffectMultiplier, (ulong)i,
                    validBuffs[maxIndex].cardType, validBuffs[maxIndex].cardIndex);   
                propertyNum.RemoveAt(maxIndex);
                validBuffs.RemoveAt(maxIndex);
                print("Success Send Data . . .");
            }
        }
    }
    // 데미지 저장 보내기
    [ServerRpc]
    public void ReciveUpUserDamageServerRpc()
    {
        print("데미지 저장 보내기!");
        for(int i = 0; i < userDamaingChangTemproy.Count; i++)
        {
            for(int j = 0; j < userDamaingChangTemproy[i].userHitDamages.Count; j++)
            {
                if(userDamaingChangTemproy[i].userHitDamages[j].startTrunNum <= GameManager.Instance.totalTrunNum.Value)
                {
                    if((GameManager.Instance.totalTrunNum.Value <= userDamaingChangTemproy[i].userHitDamages[j].endTrunNum) || GameManager.Instance.totalTrunNum.Value == -1) 
                        GameManager.Instance.InDamageToUser(userDamaingChangTemproy[i].userHitDamages[j].targetMonsterID, userDamaingChangTemproy[i].userHitDamages[j].isTargetEnemy, 
                            OwnerClientId, userDamaingChangTemproy[i].userHitDamages[j].hitHp,userDamaingChangTemproy[i].userHitDamages[j].hitDamge, 
                            userDamaingChangTemproy[i].userHitDamages[j].hitTakenDg, userDamaingChangTemproy[i].userHitDamages[j].numberOfHits);
                }    
            }
        }
    }
    [ServerRpc]
    public void GiveHealToTemproyServerRpc(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        GameManager.Instance.InComeHealTemproy(userId, startDurTimeTrun, endDurTineTrun, healAmount);
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
    // 💡 핵심 변경: 복잡한 switch-case가 사라지고 단 3줄로 끝남!
    public void TriggerSkillFromChoosEnemyOrTeam()
    {
        print("플레이어 스킬 스크립트 - 이벤트 트리거 작동!");
        int currentTurn = GameManager.Instance.totalTrunNum.Value;

        // Dictionary에서 해당 직업/카드의 효과를 찾아서 실행
        if (cardEffects.TryGetValue((job, cardIndex), out ICardEffect effect))
        {
            effect.ApplyEffect(this, currentTurn);
        }
        else
        {
            Debug.LogWarning($"아직 구현되지 않은 카드 효과입니다: {job}, Index: {cardIndex}");
        }

        // 직업 카드 스킬 저장 후 카드 발동 준비 됨
        player.ReciveSignCardEffectReady(false);
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
                        UpStateForIDUserServerRpc((ulong)i, currentTrun , currentTrun + 3, 0, 0.15f, 0, 1.2f, 0, job, cardIndex);
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
                job = JobManager.Jobs.healer;
                cardIndex = 0;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, false);
                break;
            case 1:
                job = JobManager.Jobs.healer;
                cardIndex = 1;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 2:
                job = JobManager.Jobs.healer;
                cardIndex = 2;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 3:
                job = JobManager.Jobs.healer;
                cardIndex = 3;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 4:
                job = JobManager.Jobs.healer;
                cardIndex = 4;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 5:
                job = JobManager.Jobs.healer;
                cardIndex = 5;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 6:
                job = JobManager.Jobs.healer;
                cardIndex = 6;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, true);
                break;
            case 7:
                job = JobManager.Jobs.healer;
                cardIndex = 7;
                chooseEnemyOrTeam.SetUpOnChooseEnemyOrTeam(true, false);
                break;
        }
    }
}