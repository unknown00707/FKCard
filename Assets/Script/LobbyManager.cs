using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro; // [중요] TextMeshPro 사용 (UI용)

public class LobbyManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField joinCodeInput; // 코드를 입력할 인풋 필드
    public TMP_Text lobbyCodeText;       // 방장에게 코드를 보여줄 텍스트

    private Lobby currentLobby;
    private const int MAX_PLAYERS = 3;
    private const string KEY_JOIN_CODE = "RelayJoinCode";

    async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("로그인 완료: " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // ====================================================
    //  1. 랜덤 매칭 (기존 기능)
    // ====================================================
    public async void FindMatch()
    {
        Debug.Log("랜덤 매칭 시작...");
        try
        {
            QuickJoinLobbyOptions options = new()
            {
                Filter = new List<QueryFilter>()
                {
                    new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };

            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            JoinRelay(currentLobby.Data[KEY_JOIN_CODE].Value);
        }
        catch (LobbyServiceException)
        {
            Debug.Log("랜덤 방이 없어서 새로 만듭니다.");
            CreateLobbyAndRelay(isPrivate: false); // 공개 방 생성
        }
    }

    // ====================================================
    //  2. 친구와 하기 (비공개 방 생성)
    // ====================================================
    public void CreatePrivateGame()
    {
        // 비공개 방은 목록에 안 뜨고 코드만으로 입장 가능
        CreateLobbyAndRelay(isPrivate: true);
    }

    // ====================================================
    //  3. 친구 방 입장 (코드로 입장)
    // ====================================================
    public async void JoinByCode()
    {
        string code = joinCodeInput.text; // 입력창에 쓴 코드 가져오기
        if (string.IsNullOrEmpty(code)) return;

        Debug.Log($"코드 {code}로 입장을 시도합니다...");
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
            JoinRelay(currentLobby.Data[KEY_JOIN_CODE].Value);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("방 입장에 실패했습니다: " + e);
        }
    }

    // ====================================================
    //  공통 로직 (방 생성 및 Relay 연결)
    // ====================================================
    async void CreateLobbyAndRelay(bool isPrivate)
    {
        try
        {
            // A. Relay 생성
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // B. 로비 생성
            CreateLobbyOptions options = new()
            {
                IsPrivate = isPrivate, // 여기서 공개/비공개 설정
                Data = new Dictionary<string, DataObject>()
                {
                    { KEY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyGameLobby", MAX_PLAYERS, options);
            
            NetworkManager.Singleton.StartHost();

            // C. 방 코드 화면에 표시 (친구 초대용)
            Debug.Log($"방 코드: {currentLobby.LobbyCode}");
            if(lobbyCodeText != null)
            {
                lobbyCodeText.text = $"방 코드: {currentLobby.LobbyCode}";
            }

            StartCoroutine(HeartbeatLobbyCoroutine(currentLobby.Id, 15));
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    // ... (JoinRelay와 Heartbeat 코드는 기존과 동일하므로 생략해도 되지만, 필요하면 넣어줄게!)
    async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e) { Debug.LogError(e); }
    }

    System.Collections.IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (currentLobby != null)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}