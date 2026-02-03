using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    // 기본적으로 NetworkVariable은 서버만 쓸 수 있고(Write), 모두가 읽을 수 있어(Read).
    public NetworkList<FixedString64Bytes> playerJobs;
    public NetworkList<bool> playerReady = new(); // 직업 선택 준비

    JobManager jobManager;
    CardSpaceCheck cardSpaceCheck;
    CardEffectAndCulDuringManager durManager;
    PlayerJobSkill playerJobSkill;
    StageManager stageManager;
    NetworkSessionManager networkSessionManager;
    TurnManager turnManager;
    EnemyCulGroup enemyCulGroup;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        jobManager = FindAnyObjectByType<JobManager>();
        cardSpaceCheck = FindAnyObjectByType<CardSpaceCheck>();
        durManager = FindAnyObjectByType<CardEffectAndCulDuringManager>();
        playerJobSkill = FindAnyObjectByType<PlayerJobSkill>();
        stageManager = FindAnyObjectByType<StageManager>();
        networkSessionManager = FindAnyObjectByType<NetworkSessionManager>();
        turnManager = FindAnyObjectByType<TurnManager>();
        enemyCulGroup = FindAnyObjectByType<EnemyCulGroup>();

        playerJobs = new NetworkList<FixedString64Bytes>(
            new List<FixedString64Bytes>{new("0"), new("1"), new("2")},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        playerReady = new NetworkList<bool>(
            new List<bool>{false, false, false},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    }

    // Start 대신 OnNetworkSpawn을 쓰는 게 네트워크 스크립트의 국룰이야!
    public override void OnNetworkSpawn()
    {
        // 리스너 등록
        playerJobs.OnListChanged += OnPlayerJobsChanged;
        playerReady.OnListChanged += OnPlayerReadyChanged;
    }


    public override void OnNetworkDespawn()
    {
        // 리스너 해제
        playerJobs.OnListChanged -= OnPlayerJobsChanged;
        playerReady.OnListChanged -= OnPlayerReadyChanged;
    }

    
    
    void OnPlayerJobsChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        jobManager.ReciveJobsDataPublic(playerJobs, networkSessionManager.playerTotalNum.Value);
    }

    void OnPlayerReadyChanged(NetworkListEvent<bool> changeEvent)
    {
        jobManager.ReciveReadySignPublic(playerReady);
    }
    // REQUSET ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    // 카드 배치 신호 받기 및 카드 생성 함수 호출
    public void RequsetMakeSameCardPublic(JobManager.Jobs job, int index ,ulong id)
    {
        if(!IsServer) return;

        RequsetMakeSameCardPublicClientRpc(job, index, id);
    }
    [ClientRpc]
    private void RequsetMakeSameCardPublicClientRpc(JobManager.Jobs job, int index ,ulong id)
    {
        print("ClineRpc");
        cardSpaceCheck.MakeCardPublicSame(job, index, id);
    }
    // 플레이어가 죽음 신호
    public void RequsetDieSingal(int userId, bool isDie)
    {
        if(!IsServer) return;

        networkSessionManager.ChangeStateAilive(userId, isDie);   
    }
    public void RequsetMakeInsteadPlayer(int who, int whom, float howMuch)
    {
        if(!IsServer) return;

        networkSessionManager.ChangeStateInsteadPlayer(who, whom, howMuch);
    }
    public void RequsetNormalizationInsteadPlayer(int who)
    {
        if(!IsServer) return;

        networkSessionManager.NormalizationInsteadPlayer(who);
    }
    public void RequsetNextStage()
    {
        if(!IsServer) return;

        turnManager.TurnManagerInit();
        stageManager.RequsetUpStageNum();
        NextStageForClientRpc();
    }
    [ClientRpc]
    private void NextStageForClientRpc()
    {
        print("ClineRpc: 모든 클라이언트 다음 스테이지 이동!");
        //몬스터 생성
        enemyCulGroup.MakeSameInit(stageManager.GiveStageNum(), UnityEngine.Random.Range(0,enemyCulGroup.stageMonsters[stageManager.GiveStageNum()].monsters.Count()));
        enemyCulGroup.MakeSameTotalState();
    
        // 선택에 따른 기초 영구 능력치 증가 및 적용
        durManager.UserStatInit(); // 능력치 초기화
        // 카드 초기화

        cardSpaceCheck.StartACoroutine(); // 강제 카드 내기 위한 코루틴 실행
    }
    //IN(About DATA) ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    // 직업 선택 
    public void InDicUserJobValue(ulong index, JobManager.Jobs job)
    {
        playerJobs[(int)index] = job.ToString(); 
    }

    // 준비 신호 받기
    public void InPlayerReadySign(ulong index, bool isReady)
    {
        playerReady[(int)index] = isReady; 
    }

    // 게임 시작 신호 받기
    public void InGameStartSign()
    {
        if(!IsServer) return;
        
        for(int i = 0; i < networkSessionManager.GivePlayerTotalNum(); i++)
        {
            if(!playerReady[i])
            {
                UnityEngine.Debug.LogWarning("아직 준비 안된 플레이어가 있습니다!");
                return;
            }
        }
        InGameStartSignClientRpc();
    }
    [ClientRpc]
    private void InGameStartSignClientRpc()
    {
        print("ClineRpc: 모든 클라이언트 게임 시작!");
        // 플레이어 초기 설정
        GameSetUIManager.instance.CloseSettingsPanel();
        jobManager.RequestGameStartSign();
        cardSpaceCheck.StartACoroutine(); // 강제 카드 내기 위한 코루틴 실행
        // 일반 몬스터 생성
        enemyCulGroup.MakeSameInit(stageManager.GiveStageNum(), UnityEngine.Random.Range(0,enemyCulGroup.stageMonsters[stageManager.GiveStageNum()].monsters.Count()));
        enemyCulGroup.MakeSameTotalState();
    }

    
    // 유저의 직업 스텟 정보 동기화용 함수
    public void InGameUserJobStatSame(float hp, float dg, float crip, float damageFromTakenDg, float beneficialEffectMultiplier , ulong userId)
    {
        if(!IsServer) return;
        
        durManager.ReciveUsersStat(hp, dg, crip, damageFromTakenDg, beneficialEffectMultiplier, userId);
    }

    // 카드 배치 완료 및 효과 발동 
    public void InCardEffectReady(bool isReady, ulong id)
    {
        if(!IsServer) return;

        turnManager.canActivePlayer[(int)id] = isReady;
        CheckCardEffectReady();
    }
    private void CheckCardEffectReady()
    {
        for(int i = 0; i < networkSessionManager.GivePlayerTotalNum(); i ++)
        {
            if(turnManager.GiveAblePlayerBoolValue(i))
                return;
        }

        print("받은 임시 데이터 보내기!");
        playerJobSkill.ReciveUpUserStateByBufferCard();
        playerJobSkill.ReciveUpUserDamage();
        playerJobSkill.RequsetHealDataSend();
        print("턴의 변화 확인!");
        stageManager.ReciveSignToChangeTrun(); // UI 효과
        turnManager.CheckTurnNumForWhoseTurn();
        if(enemyCulGroup.CheckEnemyAliveAllByBoolValue())
        {
            print("모든 몬스터가 죽었습니다! 다음 스테이지로 이동합니다!");
            RequsetNextStage();
        }
        else
            turnManager.ChangeTurnNumValue(); // 턴 증가
    }

    // 유저의 버프 스텟을 임시 저장
    public void InStorUserUpStatTemproy(UpStateData upStateData ,ulong sendUserID)
    {
        if(!IsServer) return;

        print("버프 카드 사용 감지!");
        playerJobSkill.ReciveUpUserStatTemproy(upStateData, sendUserID);
    }
    public void InStorUserDamageTemproy(UserHitDamage userHitDamage ,ulong sendUserID)
    {
        if(!IsServer) return;

        playerJobSkill.ReciveUpDamageTemproy(userHitDamage, sendUserID);
    }
    // 유저 스텟 상승 효과 정보 저장
    public void InUserUpStat(UpStateData package)
    {
        if(!IsServer) return;
        print("버프 정보 저장 트리거 발동!");
        durManager.ReciveUpStatUserByBuffer(package);
    }

    // 유저가 입힐 / 입을 데미지 저장
    public void InDamageToUser(UserHitDamage package, ulong sendUserID) // isToUser -> 유저에게 입힐 데미지 즉 받을 데미지 냐?
    {
        if(!IsServer) return;

        print("데미지 정보 발동!");
        durManager.ReciveDamageDataFromTemproy(package, sendUserID);
    }

    // 유저가 줄 힐 임시 저장
    public void InComeHealTemproy(UserHealData healData)
    {
        if(!IsServer) return;

        playerJobSkill.ReciveUpHealDataTemproy(healData);
    }
    // 몬스터 공격 정보 저장
    public void InStoreMonsterAttackDamage(AboutDamages aboutDamages, int enemyId)
    {
        if(!IsServer) return;

        enemyCulGroup.StoreDataAboutAttack(aboutDamages, enemyId);
        UserAoubtDamage userAoubtDamage = new()
        {
            targetMonsterID = (ulong)aboutDamages.targetID,
            hitDGDamage = aboutDamages.damageAmount,
            numberOfHits = aboutDamages.numberOfHits
        };
        durManager.RequsetDownUserCurrentStatFromDamage(userAoubtDamage);
    }
    // 유저의 카드 사용 신호
    public void InUseCardSignal(ulong id)
    {
        if(!IsServer) return;

        turnManager.SetAblePlayerBoolValue((int)id);
    }
    // 턴 넘기기 지연 신호
    public void InDelayForNextTurn(int delayTime, ulong id)
    {
        if(!IsServer) return;

        turnManager.RequsetDelayAblePlayerTurnNum(delayTime, (int)id);
    }
    ////// send data from this ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    public int SendPlayerTotalNum()
    {
        return networkSessionManager.GivePlayerTotalNum();
    }
    public override void OnDestroy()
    {
        // NetworkVariable도 Dispose가 필요합니다.
        playerJobs?.Dispose();
        playerReady?.Dispose();
    }
}