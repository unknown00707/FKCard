using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// [System.Serializable] -> 이건 인스펙터용
// INetworkSerializable -> 이건 네트워크 전송용
[System.Serializable]
public class UpStateData : INetworkSerializable
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
    // 💡 네트워크 전송 규칙 추가
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref targetUserID);
        serializer.SerializeValue(ref startTrunNum);
        serializer.SerializeValue(ref endTrunNum);
        serializer.SerializeValue(ref upHp);
        serializer.SerializeValue(ref upDamge);
        serializer.SerializeValue(ref upCritical);
        serializer.SerializeValue(ref damageTakenMultiplier);
        serializer.SerializeValue(ref beneficialEffectMultiplier);
        serializer.SerializeValue(ref cardType);
        serializer.SerializeValue(ref cardIndex);
    }
}
[System.Serializable]
public class UpStateGroup
{
    public List<UpStateData> upStateData = new();
}
[System.Serializable]
public class UserHitDamage : INetworkSerializable
{
    public ulong targetMonsterID;
    public bool isTargetEnemy;
    public int startTrunNum;
    public int endTrunNum;
    public float hitHp;
    public float hitDamge;
    public float hitTakenDg;
    public int numberOfHits;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref targetMonsterID);
        serializer.SerializeValue(ref isTargetEnemy);
        serializer.SerializeValue(ref startTrunNum);
        serializer.SerializeValue(ref endTrunNum);
        serializer.SerializeValue(ref hitHp);
        serializer.SerializeValue(ref hitDamge);
        serializer.SerializeValue(ref hitTakenDg);
        serializer.SerializeValue(ref numberOfHits);
    }
}
[System.Serializable]
public class UserDamaing
{    
    public List<UserHitDamage> userHitDamages = new();
}
[System.Serializable]
public class UserHealData : INetworkSerializable
{
    public ulong givenUserId;
    public int startTrunNum;
    public int endDurTineTrun;
    public float healAmount;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref givenUserId);
        serializer.SerializeValue(ref startTrunNum);
        serializer.SerializeValue(ref endDurTineTrun);
        serializer.SerializeValue(ref healAmount);
    }
}

public class PlayerJobSkill : NetworkBehaviour
{
    [Header("Manager")]
    public CardEffectAndCulDuringManager ceacdManager;
    public ChooseEnemyOrTeam chooseEnemyOrTeam;
    public CardSpaceCheck cardSpaceCheck;
    public EnemyCulGroup enemyCulGroup;
    public TurnManager turnManager;
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
    public void UpStateByBuffe(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
        print("버프 임시 저장 보내기!");
        UpStateData package = new UpStateData
        {
            targetUserID = userId,
            startTrunNum = startDurTimeTrun,
            endTrunNum = endDurTineTrun,
            upHp = hp,
            upDamge = dg,
            upCritical = critical,
            damageTakenMultiplier = damageMultiplier,
            beneficialEffectMultiplier = beneficialEffectMultiplier,
            cardType = cardType,
            cardIndex = cardIndex
        };

        if(IsOwner)
            player.RequsetStoreStateByBuffeServerRpc(package);
    }
    public void StoreDamageData(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits)
    {
        print("데미지 임시 저장 보내기!");
        UserHitDamage package = new UserHitDamage
        {
            targetMonsterID = userId,
            isTargetEnemy = isForEnemy,
            startTrunNum = startDurTimeTrun,
            endTrunNum = endDurTineTrun,
            hitHp = hp,
            hitDamge = dg,
            hitTakenDg = takenDg,
            numberOfHits = numberOfHits
        };

        if(IsOwner)
            player.RequsetStoreDamageServerRpc(package);
    }
    //버프 임시 저장
    public void ReciveUpUserStatTemproy(UpStateData upStateData, ulong sendUserID)
    {
        upStateGroups[(int)sendUserID].upStateData.Add(upStateData);
        print("버프 임시 저장!");
    }

    //데미지 저장
    public void ReciveUpDamageTemproy(UserHitDamage userHitDamage, ulong sendUserID)
    {
        userDamaingChangTemproy[(int)sendUserID].userHitDamages.Add(userHitDamage);
        print("데미지 임시 저장!");
    }

    // 버프 데이터 보내기
    public void ReciveUpUserStateByBufferCard()
    {
        print("버프 저장 보내기!");
        // 적용될 버프의 우선순위 분류 및 적용
        for(int i = 0; i < upStateGroups.Count; i++)
        {
            List<float> propertyNum = new();

            var validBuffs = upStateGroups.SelectMany(group => group.upStateData)
                .Where(upState => (int)upState.targetUserID == i &&
                                  upState.startTrunNum <= turnManager.GiveTurnValue() &&
                                  (upState.endTrunNum == -1 || turnManager.GiveTurnValue() <= upState.endTrunNum))
                .ToList();
            foreach(var buff in validBuffs)
            {
                print($"Valid Buff for User {i}: HP +{buff.upHp}, Damage +{buff.upDamge}, Critical +{buff.upCritical}, Damage Taken Multiplier +{buff.damageTakenMultiplier}");
                propertyNum.Add((buff.upHp + buff.upDamge + buff.upCritical + buff.damageTakenMultiplier) / 100f);
            }

            int propertyNumCount = propertyNum.Count;
            for(int j = 0; j < propertyNumCount; j++)
            {
                if(!IsOwner) return;

                float maxValue = propertyNum.Max();
                int maxIndex = propertyNum.IndexOf(maxValue);
                UpStateData package = new UpStateData
                {
                    targetUserID = validBuffs[maxIndex].targetUserID,
                    startTrunNum = validBuffs[maxIndex].startTrunNum,
                    endTrunNum = validBuffs[maxIndex].endTrunNum,
                    upHp = validBuffs[maxIndex].upHp,
                    upDamge = validBuffs[maxIndex].upDamge,
                    upCritical = validBuffs[maxIndex].upCritical,
                    damageTakenMultiplier = validBuffs[maxIndex].damageTakenMultiplier,
                    beneficialEffectMultiplier = validBuffs[maxIndex].beneficialEffectMultiplier,
                    cardType = validBuffs[maxIndex].cardType,
                    cardIndex = validBuffs[maxIndex].cardIndex
                };
                
                player.RequsetUpStateByBuffeServerRpc(package);  
                propertyNum.RemoveAt(maxIndex);
                validBuffs.RemoveAt(maxIndex);
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
                if(userDamaingChangTemproy[i].userHitDamages[j].startTrunNum <= turnManager.GiveTurnValue())
                {
                    if((turnManager.GiveTurnValue() <= userDamaingChangTemproy[i].userHitDamages[j].endTrunNum) || turnManager.GiveTurnValue() == -1)
                    {
                        UserHitDamage userHitDamage = new UserHitDamage{
                            targetMonsterID = userDamaingChangTemproy[i].userHitDamages[j].targetMonsterID,
                            isTargetEnemy = userDamaingChangTemproy[i].userHitDamages[j].isTargetEnemy,
                            startTrunNum = userDamaingChangTemproy[i].userHitDamages[j].startTrunNum,
                            endTrunNum = userDamaingChangTemproy[i].userHitDamages[j].endTrunNum,
                            hitHp = userDamaingChangTemproy[i].userHitDamages[j].hitHp,
                            hitDamge = userDamaingChangTemproy[i].userHitDamages[j].hitDamge,
                            hitTakenDg = userDamaingChangTemproy[i].userHitDamages[j].hitTakenDg,
                            numberOfHits = userDamaingChangTemproy[i].userHitDamages[j].numberOfHits
                        };
                        if(IsOwner)
                            player.RequsetUpDamageServerRpc(userHitDamage);
                    }
                }    
            }
        }
    }
    public void GiveHealToTemproy(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        UserHealData userHealData = new UserHealData()
        {
            givenUserId = userId,
            startTrunNum = startDurTimeTrun,
            endDurTineTrun = endDurTineTrun,
            healAmount = healAmount
        };
        if(IsOwner)
            player.RequsetHealDataSendServerRpc(userHealData);
    }
    // 힐량 임시 저장
    public void ReciveUpHealDataTemproy(UserHealData healData)
    {
        userHealDataTemproy.Add(new UserHealData()
        {
            givenUserId = healData.givenUserId,
            startTrunNum = healData.startTrunNum, 
            endDurTineTrun = healData.endDurTineTrun,
            healAmount = healData.healAmount
        });
    }
    // 힐량 보내기
    public void RequsetHealDataSend()
    {
        for(int i = 0; i < userHealDataTemproy.Count; i++)
        {
            if(userHealDataTemproy[i].startTrunNum <= turnManager.GiveTurnValue())
            {   
                if(turnManager.GiveTurnValue() < userHealDataTemproy[i].endDurTineTrun)
                    ceacdManager.ReciveHealData(userHealDataTemproy[i].givenUserId, userHealDataTemproy[i].healAmount);
            }    
        }
    }
    // 💡 핵심 변경: 복잡한 switch-case가 사라지고 단 3줄로 끝남!
    public void TriggerSkillFromChoosEnemyOrTeam()
    {
        print("플레이어 스킬 스크립트 - 이벤트 트리거 작동!");
        int currentTurn = turnManager.GiveTurnValue();

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
        int currentTrun = turnManager.GiveTurnValue();
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
                for(int i = 0; i < turnManager.GiveTurnValue(); i++)
                {
                    if((ulong)i != NetworkManager.Singleton.LocalClientId)
                    {
                        UpStateByBuffe((ulong)i, currentTrun , currentTrun + 3, 0, 0.15f, 0, 1.2f, 0, job, cardIndex);
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