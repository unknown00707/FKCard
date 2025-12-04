using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    // 기본적으로 NetworkVariable은 서버만 쓸 수 있고(Write), 모두가 읽을 수 있어(Read).
    public NetworkList<FixedString64Bytes> playerJobs;
    public NetworkVariable<int> playerTotalNum = new();
    public NetworkVariable<int> totalTrunNum = new();
    public NetworkList<bool> playerReady = new(); // 직업 선택 준비
    public NetworkList<bool> playerCardSetReady = new(); // 턴 넘길 준비
    public NetworkVariable<bool> isPlayerTrun = new();

    JobManager jobManager;
    CardSpaceCheck cardSpaceCheck;
    CardEffectAndCulDuringManager durManager;
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
            playerTotalNum.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
            isPlayerTrun.Value = true;
            totalTrunNum.Value = 0;
        }

        // 리스너 등록
        playerTotalNum.OnValueChanged += OnPlayerTotalNumChanged;
        isPlayerTrun.OnValueChanged += OnPlayerTrunIsOn;
        playerJobs.OnListChanged += OnPlayerJobsChanged;
        playerReady.OnListChanged += OnPlayerReadyChanged;
        playerCardSetReady.OnListChanged += OnPlayerTrunCulChanged;

    }


    public override void OnNetworkDespawn()
    {
        // 리스너 해제
        playerTotalNum.OnValueChanged -= OnPlayerTotalNumChanged;
        isPlayerTrun.OnValueChanged -= OnPlayerTrunIsOn;
        playerJobs.OnListChanged -= OnPlayerJobsChanged;
        playerReady.OnListChanged -= OnPlayerReadyChanged;
        playerCardSetReady.OnListChanged -= OnPlayerTrunCulChanged;
    }

    
    void OnPlayerTotalNumChanged(int oldValue, int newValue)
    {
        Debug.Log("플레이어 수 조정  {oldValue} -> {newValue} . . . 난이도 조정 중. . .");
    }
    void OnPlayerJobsChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        jobManager.ReciveJobsDataPublic(playerJobs, playerTotalNum.Value);
    }

    void OnPlayerReadyChanged(NetworkListEvent<bool> changeEvent)
    {
        jobManager.ReciveReadySignPublic(playerReady);
    }

    void OnPlayerTrunCulChanged(NetworkListEvent<bool> changeEvent)
    {
        for(int i = 0; i < playerTotalNum.Value; i ++)
        {
            if(playerCardSetReady[i])
            {
                isPlayerTrun.Value = true;
                return;
            }
        }

        isPlayerTrun.Value = false; // 턴 넘어갈 타이밍 , 보스/몬스터 턴
    }

    void OnPlayerTrunIsOn(bool oldValue, bool newValue)
    {
        if(!isPlayerTrun.Value)
        {
            StartCoroutine(WaitForDamageAndEffect(false)); // 몬스터 턴
        }
        else
            StartCoroutine(WaitForDamageAndEffect(true)); // 플레이어 턴
    }

    IEnumerator WaitForDamageAndEffect(bool isPlayerTrun)
    {
        if(!isPlayerTrun)
        {
            // 턴에 해당하는 보스에게 데미지 주는 코드 . . .
        }

        yield return new WaitForSeconds(5f);

        cardSpaceCheck.CardSpacePrefabsInit(isPlayerTrun); // 몬스터 턴
        totalTrunNum.Value++;
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
        
        InGameStartSignClientRpc();
    }
    [ClientRpc]
    private void InGameStartSignClientRpc()
    {
        for(int i = 0; i < playerTotalNum.Value; i++)
        {
            if(!playerReady[i])
                return;
        }

        jobManager.RequestGameStartSign();
    }

    
    // 유저의 직업 스텟 정보 동기화용 함수
    public void InGameUserJobStatSame(float hp, float dg, float crip, ulong userId)
    {
        if(!IsServer) return;
        
        GameUserJobClientRpc(hp, dg, crip, userId);
    }

    [ClientRpc]
    private void GameUserJobClientRpc(float hp, float dg, float crip, ulong userId)
    {
        durManager.ReciveUsersStat(hp, dg, crip, userId);
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
        cardSpaceCheck.MakeCardPublicSame(job, index, id);
    }

    // 카드 배치 완료 및 효과 발동 
    public void InCardEffectReady(bool isReady, ulong id)
    {
        playerCardSetReady[(int)id] = isReady;
    }
    // 유저 스텟 상승 효과 정보 저장
    public void InUserUpStat(int durTime, float hpIng, float dGing, float criticalIng, ulong id)
    {
        if(!IsServer) return;

        RequsetUpUserStatClientRpc(durTime, hpIng, dGing, criticalIng, id);
    }
    [ClientRpc]
    private void RequsetUpUserStatClientRpc(int durTime, float hpIng, float dGing, float criticalIng, ulong id)
    {
        durManager.ReciveCardEffect(durTime, hpIng, dGing, criticalIng, id);
    }
    // 유저가 입힐 / 입을 데미지 저장
    public void InDamageToUser(bool isToUser , float damage, ulong id, int durTime) // isToUser -> 유저에게 입힐 데미지 즉 받을 데미지 냐?
    {
        if(!IsServer) return;

        RequsetUpDamageIFOClientRpc(isToUser, damage, id, totalTrunNum.Value , durTime);
    }
    [ClientRpc]
    private void RequsetUpDamageIFOClientRpc(bool isToUser, float damage, ulong id, int currentTrunNum , int durTime)
    {
        durManager.ReciveCardEffectDamage(isToUser, damage, id, currentTrunNum , durTime);
    }

    public override void OnDestroy()
    {
        playerJobs?.Dispose();
        isPlayerTrun?.Dispose();
        playerTotalNum?.Dispose(); // NetworkVariable도 Dispose가 필요합니다.
        playerReady?.Dispose();
        playerCardSetReady?.Dispose();
    }

    
}