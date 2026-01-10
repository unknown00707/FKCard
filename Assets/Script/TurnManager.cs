using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public EnemyCulGroup enemyCulGroup;
    public NetworkVariable<int> totalTrunNum = new();
    public NetworkVariable<bool> isPlayerTrun = new();
    public NetworkList<bool> canActivePlayer = new();
    public NetworkList<int> canActivePlayerTurnNum = new();
    CardPlayer player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        isPlayerTrun.Value = true;
        totalTrunNum.Value = 0;
        canActivePlayer = new NetworkList<bool>(
            new List<bool>{true, true, true},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        canActivePlayerTurnNum = new NetworkList<int>(
            new List<int>{0, 0, 0},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        totalTrunNum.OnValueChanged += OnTotalTurnNumChanged;
        isPlayerTrun.OnValueChanged += OnPlayerTrunIsOn;
    }
    void Update()
    {
        if (player == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
        }
    }
    // Update is called once per frame
    public void DisInit()
    {
        totalTrunNum.OnValueChanged -= OnTotalTurnNumChanged;
        isPlayerTrun.OnValueChanged -= OnPlayerTrunIsOn;
    }
    public void DisposeFounction()
    {
        isPlayerTrun?.Dispose();
    }
    void OnTotalTurnNumChanged(int oldValue, int newValue)
    {
        // 턴 변화시 UI 효과
    }   
    void OnPlayerTrunIsOn(bool oldValue, bool newValue)
    {
        // 턴 변화시 UI 효과
    }
    
    public void ChangeTurnBoolValue()
    {
        isPlayerTrun.Value = !isPlayerTrun.Value;
    }
    public void ChangeTurnNumValue()
    {
        totalTrunNum.Value++;
    }
    public int GiveTurnValue()
    {
        return totalTrunNum.Value;
    }

    public bool GiveAblePlayerBoolValue(int userId)
    {
        return canActivePlayer[userId];
    }
    public void SetAblePlayerBoolValue(int userId)
    {
        canActivePlayer[userId] = !canActivePlayer[userId];
    }
    public void RequsetDelayAblePlayerTurnNum(int delayTime, int userId)
    {
        canActivePlayerTurnNum[userId] = delayTime + totalTrunNum.Value;
        ComparisonAblePlayer();
    }
    public void ComparisonAblePlayer()
    {
        for(int i = 0; i < GameManager.Instance.SendPlayerTotalNum(); i++)
        {
            if(canActivePlayerTurnNum[i] > totalTrunNum.Value)
            {
                canActivePlayer[i] = false;
            }
            else
            {
                canActivePlayer[i] = true;
            }
        }
    }
    public void CheckTurnNumForWhoseTurn()
    {
        if(totalTrunNum.Value % 2 == 0)
        {
            print("플레이어 턴입니다.");
        }
        else
        {
            print("몬스터 턴입니다.");
            enemyCulGroup.AttackAllEnemyCulOnStage();
        }
    }
    public void RequsetDelayTurn(int delayTime)
    {
        player.RequsetDelayForNextTurn(delayTime);
    }
}
