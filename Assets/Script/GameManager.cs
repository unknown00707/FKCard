using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // 기본적으로 NetworkVariable은 서버만 쓸 수 있고(Write), 모두가 읽을 수 있어(Read).
    public NetworkList<FixedString64Bytes> playerJobs;
    public NetworkVariable<int> playerTotalNum = new();
    public NetworkList<bool> playerReady = new();

    void Awake()
    {
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
        if(IsServer)
        {
            playerTotalNum.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
        }

        // 리스너 등록
        playerTotalNum.OnValueChanged += OnPlayerTotalNumChanged;
        playerJobs.OnListChanged += OnPlayerJobsChanged;
        playerReady.OnListChanged += OnPlayerReadyChanged;

        print("리스너 등록");
    }


    public override void OnNetworkDespawn()
    {
        // 리스너 해제
        playerTotalNum.OnValueChanged -= OnPlayerTotalNumChanged;
        playerJobs.OnListChanged -= OnPlayerJobsChanged;
        playerReady.OnListChanged -= OnPlayerReadyChanged;
    }

    
    void OnPlayerTotalNumChanged(int oldValue, int newValue)
    {
        Debug.Log("플레이어 수 조정  {oldValue} -> {newValue} . . . 난이도 조정 중. . .");
    }
    void OnPlayerJobsChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        print("리스너 작동");
        JobManager jobManager = FindAnyObjectByType<JobManager>();
        jobManager.ReciveJobsDataPublic(playerJobs, playerTotalNum.Value);
    }

    void OnPlayerReadyChanged(NetworkListEvent<bool> changeEvent)
    {
        JobManager jobManager = FindAnyObjectByType<JobManager>();
        jobManager.ReciveReadySignPublic(playerReady);
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
        for(int i = 0; i < playerTotalNum.Value; i++)
        {
            if(!playerReady[i])
                return;
        }

        JobManager jobManager = FindAnyObjectByType<JobManager>();
        jobManager.RequestGameStartSignClientRpc();
    }

    public override void OnDestroy()
    {
        playerJobs?.Dispose();
        playerTotalNum?.Dispose(); // NetworkVariable도 Dispose가 필요합니다.
        playerReady?.Dispose();
    }

    
}