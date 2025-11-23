using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // 기본적으로 NetworkVariable은 서버만 쓸 수 있고(Write), 모두가 읽을 수 있어(Read).
    public NetworkVariable<int> TurnNumber = new(1);
    public NetworkVariable<JobManager.Jobs> playerJobs = new();
    public NetworkVariable<ulong> playerID = new();

    // Start 대신 OnNetworkSpawn을 쓰는 게 네트워크 스크립트의 국룰이야!
    public override void OnNetworkSpawn()
    {
        // 값이 바뀔 때마다 로그를 찍어보자 (서버, 클라 모두 실행됨)
        TurnNumber.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"[턴 변경] {oldValue} -> {newValue}");
        };
    }

    void Update()
    {
        // 1. (중요) 내가 '내 캐릭터'를 가진 주인일 때만 입력을 받거나,
        // GameManager처럼 공용 객체라면 'IsOwner' 체크가 필요 없을 수도 있지만,
        // 보통 입력을 서버로 보내는 건 'ServerRpc'를 통해야 해.

        if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바를 누르면
        {
            // 내가 서버(Host)라면? -> 바로 변경
            if (IsServer)
            {
                ChangeValue();
            }
            // 내가 클라이언트라면? -> 서버한테 바꿔달라고 요청 (RPC)
            else
            {
                RequestChangeTurnServerRpc();
            }
        }
    }

    // [ServerRpc] : 클라이언트가 호출하지만, 실제 실행은 서버에서 되는 함수
    [ServerRpc] // 누구나 요청할 수 있게 허용
    void RequestChangeTurnServerRpc()
    {
        ChangeValue();
    }

    void ChangeValue()
    {
        // 값 변경은 오직 서버에서만 일어나야 에러가 안 남!
        TurnNumber.Value += 1;
    }

    public void InDicUserJobValue(ulong index, JobManager.Jobs job)
    {
        
        playerID.Value = index;
        playerJobs.Value = job;
        print(playerID.Value + " , " + playerJobs.Value);
        
    }
}