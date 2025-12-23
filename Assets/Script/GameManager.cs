using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    // 기본적으로 NetworkVariable은 서버만 쓸 수 있고(Write), 모두가 읽을 수 있어(Read).
    public NetworkList<FixedString64Bytes> playerJobs;
    public NetworkList<bool> playerReady = new(); // 직업 선택 준비
    public NetworkList<bool> playerCardSetReady = new(); // 턴 넘길 준비
    JobManager jobManager;
    CardSpaceCheck cardSpaceCheck;
    CardEffectAndCulDuringManager durManager;
    PlayerJobSkill playerJobSkill;
    StageManager stageManager;
    NetworkSessionManager networkSessionManager;
    TurnManager turnManager;
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

        playerCardSetReady = new NetworkList<bool>(
            new List<bool>{true, true, true},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    }

    // Start 대신 OnNetworkSpawn을 쓰는 게 네트워크 스크립트의 국룰이야!
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            networkSessionManager.Init();
            turnManager.Init();
            stageManager.Init();
        }

        // 리스너 등록
        playerJobs.OnListChanged += OnPlayerJobsChanged;
        playerReady.OnListChanged += OnPlayerReadyChanged;
        playerCardSetReady.OnListChanged += OnPlayerSetCardChanged;
    }


    public override void OnNetworkDespawn()
    {
        // 리스너 해제
        playerJobs.OnListChanged -= OnPlayerJobsChanged;
        playerReady.OnListChanged -= OnPlayerReadyChanged;
        playerCardSetReady.OnListChanged -= OnPlayerSetCardChanged;
        stageManager.DisInit();
        turnManager.DisInit();
        networkSessionManager.DisInit();
    }

    
    
    void OnPlayerJobsChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        jobManager.ReciveJobsDataPublic(playerJobs, networkSessionManager.playerTotalNum.Value);
    }

    void OnPlayerReadyChanged(NetworkListEvent<bool> changeEvent)
    {
        jobManager.ReciveReadySignPublic(playerReady);
    }
    void OnPlayerSetCardChanged(NetworkListEvent<bool> changeEvent)
    {
        // 턴 넘김 효과 . . .
    }
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
        jobManager.RequestGameStartSign();
    }

    
    // 유저의 직업 스텟 정보 동기화용 함수
    public void InGameUserJobStatSame(float hp, float dg, float crip, float damageFromTakenDg, float beneficialEffectMultiplier , ulong userId)
    {
        if(!IsServer) return;
        
        durManager.ReciveUsersStat(hp, dg, crip, damageFromTakenDg, beneficialEffectMultiplier, userId);
    }

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

    // 카드 배치 완료 및 효과 발동 
    public void InCardEffectReady(bool isReady, ulong id)
    {
        if(!IsServer) return;

        playerCardSetReady[(int)id] = isReady;
        CheckCardEffectReady();
    }
    private void CheckCardEffectReady()
    {
        for(int i = 0; i < networkSessionManager.GivePlayerTotalNum(); i ++)
        {
            if(playerCardSetReady[i])
                return;
        }

        print("받은 임시 데이터 보내기!");
        playerJobSkill.ReciveUpUserStateByBufferCardServerRpc();
        playerJobSkill.ReciveUpUserDamageServerRpc();
        playerJobSkill.RequsetHealDataSend();
        print("턴의 변화 확인!");
        turnManager.ChangeTurnBoolValue(); // 턴 넘어갈 타이밍 , 보스/몬스터 턴
        stageManager.ReciveSignToChangeTrun();
        turnManager.ChangeTurnNumValue();
    }

    // 유저의 버프 스텟을 임시 저장
    public void InStorUserUpStatTemproy(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier,ulong sendUserID, JobManager.Jobs cardType, int cardIndex)
    {
        if(!IsServer) return;

        print("버프 카드 사용 감지!");
        playerJobSkill.ReciveUpUserStatTemproy(userId, startDurTimeTrun, endDurTineTrun, hp, dg, critical, damageMultiplier, beneficialEffectMultiplier, sendUserID, cardType, cardIndex);
    }
    public void InStorUserDamageTemproy(ulong userId, bool isForEnemy, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits, ulong sendUserID)
    {
        if(!IsServer) return;

        playerJobSkill.ReciveUpDamageTemproy(userId, isForEnemy, startDurTimeTrun, endDurTineTrun, hp, dg, takenDg, numberOfHits, sendUserID);
    }
    // 유저 스텟 상승 효과 정보 저장
    public void InUserUpStat(float hpIng, float dGing, float criticalIng, float damageMultiplier, float beneficialEffectMultiplier,ulong id, JobManager.Jobs cardType, int cardIndex)
    {
        if(!IsServer) return;
        print("버프 정보 저장 트리거 발동!");
        durManager.ReciveUpStatUserByBuffer(hpIng, dGing, criticalIng, damageMultiplier, beneficialEffectMultiplier,id, cardType, cardIndex);
    }

    // 유저가 입힐 / 입을 데미지 저장
    public void InDamageToUser(ulong enemyID, bool isToEnemy, ulong sendUserID ,float damageFromHp, float damagFromDg, float damageFromTakenDg, int numberOfHits) // isToUser -> 유저에게 입힐 데미지 즉 받을 데미지 냐?
    {
        if(!IsServer) return;

        print("데미지 정보 발동!");
        durManager.ReciveDamageDataFromTemproy(enemyID, isToEnemy, sendUserID, damageFromHp, damagFromDg , damageFromTakenDg, numberOfHits);
    }

    // 유저가 줄 힐 임시 저장
    public void InComeHealTemproy(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        if(!IsServer) return;

        playerJobSkill.ReciveUpHealDataTemproy(userId, startDurTimeTrun, endDurTineTrun, healAmount);
    }
    ////// send data
    public int SendPlayerTotalNum()
    {
        return networkSessionManager.GivePlayerTotalNum();
    }

    public override void OnDestroy()
    {
        // NetworkVariable도 Dispose가 필요합니다.
        playerJobs?.Dispose();
        playerReady?.Dispose();
        playerCardSetReady?.Dispose();
        networkSessionManager.DisposeFounction();
        turnManager.DisposeFounction();
        stageManager.DisposeFounction();
    }
}