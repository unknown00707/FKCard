using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSessionManager : NetworkBehaviour
{
    public NetworkVariable<int> playerTotalNum = new();
    public NetworkList<bool> isPlayerAlive = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        playerTotalNum.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
        isPlayerAlive = new NetworkList<bool>(
            new List<bool>{true, true, true},
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

    public void ChangeStateAilive(int userId)
    {
        isPlayerAlive[userId] = !isPlayerAlive[userId];
    }
}
