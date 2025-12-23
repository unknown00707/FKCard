using Unity.Netcode;
using UnityEngine;

public class NetworkSessionManager : NetworkBehaviour
{
    public NetworkVariable<int> playerTotalNum = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        playerTotalNum.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
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
}
