using System.Collections.Generic;
using Unity.Netcode;

public class NetworkSessionManager : NetworkBehaviour
{
    public NetworkVariable<int> playerTotalNum = new();
    public NetworkList<bool> isPlayerAlive = new();
    public NetworkList<int> insteadPlayer = new();
    public NetworkList<float> insteadPlayerMagnification = new();
    private readonly int INSTEAD_PLAYER_MANIFICATION_VALUE = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        playerTotalNum.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
        isPlayerAlive = new NetworkList<bool>(
            new List<bool>{true, true, true},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        insteadPlayer = new NetworkList<int>(
            new List<int>{0, 1, 2},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        insteadPlayerMagnification = new NetworkList<float>(
            new List<float>{INSTEAD_PLAYER_MANIFICATION_VALUE, INSTEAD_PLAYER_MANIFICATION_VALUE, INSTEAD_PLAYER_MANIFICATION_VALUE},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        playerTotalNum.OnValueChanged += OnPlayerTotalNumChanged;
    }

    // Update is called once per frame
    public void DisInit()
    {
        playerTotalNum.OnValueChanged -= OnPlayerTotalNumChanged;
        
    }
    public void DisposeFounction()
    {
        playerTotalNum?.Dispose();
    }

    void OnPlayerTotalNumChanged(int oldValue, int newValue)
    {
        UnityEngine.Debug.Log("플레이어 수 조정  {oldValue} -> {newValue} . . . 난이도 조정 중. . .");
    }

    public int GivePlayerTotalNum()
    {
        return playerTotalNum.Value;
    }

    public bool GivePlayerAliveBool(int userId)
    {
        return isPlayerAlive[userId];
    }

    public void ChangeStateAilive(int userId, bool isDie)
    {
        if(isDie)
            isPlayerAlive[userId] = false;
        else
            isPlayerAlive[userId] = true;
    }

    public void ChangeStateInsteadPlayer(int who, int whom, float howMuch)
    {
        insteadPlayer[who] = whom;
        insteadPlayerMagnification[who] = howMuch;
    }

    public void NormalizationInsteadPlayer(int who)
    {
        insteadPlayer[who] = who;
        insteadPlayerMagnification[who] = INSTEAD_PLAYER_MANIFICATION_VALUE;
    }
}
