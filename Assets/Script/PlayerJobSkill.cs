using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// --- 탱커(Defender) 카드 효과 ---
// --- 기사(Knight) 카드 효과 ---
// --- 마법사(Wizard) 카드 효과 ---
// --- 힐러(Healer) 카드 효과 ---

// =================================================================================
// [Main Class] PlayerJobSkill
// =================================================================================

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
            { (JobManager.Jobs.healler, 0), new HealerCard0Effect() },
            { (JobManager.Jobs.healler, 1), new HealerCard1Effect() },
            // 2번, 5번은 미구현 상태라 제외
            { (JobManager.Jobs.healler, 3), new HealerCard3Effect() },
            { (JobManager.Jobs.healler, 4), new HealerCard4Effect() },
            { (JobManager.Jobs.healler, 6), new HealerCard6Effect() },
            { (JobManager.Jobs.healler, 7), new HealerCard7Effect() },
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
    
    public void UpStateForIDUser(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
        print("버프 임시 저장 보내기!");
        player.UpUserStatTemporary(userId, startDurTimeTrun, endDurTineTrun, hp, dg, critical, damageMultiplier, beneficialEffectMultiplier, cardType , cardIndex);
    }
    
    public void GiveDamageForIDUser(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits)
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
                        UpStateForIDUser((ulong)i, currentTrun , currentTrun + 3, 0, 0.15f, 0, 1.2f, 0, job, cardIndex);
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