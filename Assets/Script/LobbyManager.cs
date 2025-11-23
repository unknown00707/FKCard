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
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;  // 로그인/매칭 버튼이 있는 패널
    public GameObject roomPanel;  // 대기실 패널
    public GameObject selectedWay;
    public GameObject createRoomUI;
    public GameObject joinRoomUI;

    [Header("UI References")]
    public TMP_InputField joinCodeInput; // 코드를 입력할 인풋 필드
    public TMP_Text playerListText;  // 플레이어 목록 표시 (지금은 숫자로 대체)
    public TMP_Text roomInfoText;    // 방 코드 및 인원 수 표시
    public Button startGameButton;   // 게임 시작 버튼 (방장만 보임)

    private Lobby currentLobby;
    private const int MAX_PLAYERS = 3;
    private const string KEY_JOIN_CODE = "RelayJoinCode";
    private float heartbeatTimer;


    async void Start()
    {
        // 1. 초기화 옵션 생성
        InitializationOptions options = new InitializationOptions();

#if UNITY_EDITOR
        // 에디터에서 실행 중이라면 "EditorUser"라는 이름표를 붇임
        options.SetProfile("EditorUser");
#else
        // 빌드된 파일에서 실행 중이라면, 실행할 때마다 랜덤한 이름표를 붙임
        // (이렇게 하면 빌드 파일을 2개, 3개 켜도 서로 다른 사람으로 인식됨)
        string randomProfile = "BuildUser_" + UnityEngine.Random.Range(0, 10000);
        options.SetProfile(randomProfile);
#endif

        // 2. 옵션을 넣어서 유니티 서비스 초기화 (이게 가장 중요!)
        await UnityServices.InitializeAsync(options);

        // 3. 로그인 진행
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log($"로그인 완료! 내 ID: {AuthenticationService.Instance.PlayerId}");

        // 4. 이벤트 연결 및 초기화
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) => UpdateLobbyUI();
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) => UpdateLobbyUI();

        SwitchToMainPanel();
    }

    // ==================================================================================
    // 2. UI 패널 교체 및 업데이트 (시각적 기능)
    // ==================================================================================
    
    // 메인 화면으로 전환
    void SwitchToMainPanel()
    {
        selectedWay.SetActive(true);
        createRoomUI.SetActive(false);
        joinRoomUI.SetActive(false);

        mainPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    // 방 만들기 UI 활성화
    public void CreateRoomUI()
    {
        selectedWay.SetActive(false);
        createRoomUI.SetActive(false);
        joinRoomUI.SetActive(true);
    }

    // 대기실 화면으로 전환
    void SwitchToRoomPanel()
    {
        mainPanel.SetActive(false);
        roomPanel.SetActive(true);
        UpdateLobbyUI();
    }

    // 대기실 정보를 갱신하는 함수 (누가 들어오거나 나갈 때마다 호출)
    void UpdateLobbyUI()
    {
        // 아직 연결 상태가 아니라면 무시
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) return;

        int playerCount = NetworkManager.Singleton.ConnectedClients.Count; // 현재 접속자 수
        string roomCode = currentLobby != null ? currentLobby.LobbyCode : "Loading...";

        // 1. 방 정보 텍스트 갱신
        roomInfoText.text = $"방 코드: {roomCode}\n현재 인원: {playerCount} / {MAX_PLAYERS}";

        // 2. 플레이어 목록 텍스트 갱신 (단순 리스트)
        string playerList = "참가자 목록:\n";
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // client.ClientId는 0, 1, 2... 식의 고유 번호
            string role = client.ClientId == NetworkManager.Singleton.LocalClientId ? "(나)" : "";
            playerList += $"Player {client.ClientId} {role}\n";
        }
        playerListText.text = playerList;

        // 3. 게임 시작 버튼 처리 (방장만 보여야 함!)
        if (NetworkManager.Singleton.IsHost && startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true);
            // 3명이 다 모여야 버튼이 활성화되게 하려면:
            // startGameButton.interactable = (playerCount == MAX_PLAYERS); 
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }
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
            Debug.LogError($"로비 입장 실패: {e.Message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"일반 오류: {e.Message}");
        }
    }

    // ====================================================
    //  공통 로직 (방 생성 및 Relay 연결)
    // ====================================================
    async void CreateLobbyAndRelay(bool isPrivate)
    {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = isPrivate;
            options.Data = new Dictionary<string, DataObject>() { { KEY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Public, relayCode) } };
            
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyGame", MAX_PLAYERS, options);
            
            NetworkManager.Singleton.StartHost();
            SwitchToRoomPanel(); // <--- UI 전환!
        } catch (System.Exception e) { Debug.LogError(e); }
    }
    private async System.Threading.Tasks.Task QuickJoinLogic()
    {
        try {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            options.Filter = new List<QueryFilter>() { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) };
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            JoinRelay(currentLobby.Data[KEY_JOIN_CODE].Value);
        } catch { 
            CreateLobbyAndRelay(false); 
        }
    }

    
  // [공통] 입장 로직
    async void JoinRelay(string joinCode)
    {
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);
            
            NetworkManager.Singleton.StartClient();
            SwitchToRoomPanel(); // <--- UI 전환!
        } catch (System.Exception e) { Debug.LogError(e); }
    }

    // [버튼] 방 나가기
    public async void LeaveRoom()
    {
        try
        {
            // 1. Netcode 연결 끊기
            NetworkManager.Singleton.Shutdown();

            // 2. 로비에서 플레이어 제거 (UGS 업데이트)
            if (currentLobby != null)
            {
                // 내가 방장이라면 로비 자체를 삭제할 수도 있고, 그냥 나갈 수도 있음.
                // 여기서는 간단하게 '플레이어 제거'만 수행
                string playerId = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
                currentLobby = null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("나가는 중 오류 발생 (이미 끊겼을 수도 있음): " + e.Message);
        }
        finally
        {
            // 3. UI 메인으로 복귀
            SwitchToMainPanel();
            Debug.Log("방을 나갔습니다.");
        }
    }

    // [버튼] 게임 시작 (방장 전용)
    public void StartGameScene()
    {
        // 방장만 씬을 로드할 수 있음
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null) return;

        HandleLobbyHeartbeat();
    }

    void HandleLobbyHeartbeat()
    {
        // [수정] NetworkManager.Singleton이 존재하는지 먼저 확인해야 해!
        if (NetworkManager.Singleton == null) return;

        // 그 다음 방장인지 확인
        if (currentLobby != null && NetworkManager.Singleton.IsHost) 
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 15;
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }
}