using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public EnemyCulGroup enemyCulGroup;
    public NetworkVariable<int> totalTrunNum = new(0);
    public NetworkList<bool> canActivePlayer = new(
            new List<bool>{true, true, true},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public NetworkList<int> canActivePlayerTurnNum = new(
            new List<int>{0, 0, 0},
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    CardPlayer player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void TurnManagerInit()
    {
        totalTrunNum.Value = 0;
        for(int i = 0; i < GameManager.Instance.SendPlayerTotalNum(); i++)
        {
            canActivePlayer[i] = true;
            canActivePlayerTurnNum[i] = 0;
        }
    }
    void Update()
    {
        if (player == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
        }
    }
    public override void OnNetworkSpawn()
    {
        totalTrunNum.OnValueChanged += OnTotalTurnNumChanged;
    }
    // Update is called once per frame
    public override void OnNetworkDespawn()
    {
        totalTrunNum.OnValueChanged -= OnTotalTurnNumChanged;
    }
    public override void OnDestroy()
    {
        totalTrunNum?.Dispose();
        canActivePlayer?.Dispose();
        canActivePlayerTurnNum?.Dispose();
    }
    void OnTotalTurnNumChanged(int oldValue, int newValue)
    {
        // UI 이펙트

        GameManager.Instance.RequsetNextStage();
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
        if(canActivePlayer[userId])
            canActivePlayer[userId] = false;
        else
            canActivePlayer[userId] = true;
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
    public bool CheckTurnNumForWhoseTurn()
    {
        if(totalTrunNum.Value % 2 == 0)
        {
            print("플레이어 턴입니다.");
            enemyCulGroup.RequsetTheDamageToMonster(); // 몬스터가 피해를 받음
            return true;
        }
        else
        {
            print("몬스터 턴입니다.");
            enemyCulGroup.AttackAllEnemyCulOnStage(); // 몬스터가 데미지를 줌
            return false;
        }
    }
    public void RequsetDelayTurn(int delayTime)
    {
        player.RequsetDelayForNextTurn(delayTime);
    }
}
