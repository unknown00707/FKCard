using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public EnemyCulGroup enemyCulGroup;
    public NetworkVariable<int> totalTrunNum = new();
    public NetworkVariable<bool> isPlayerTrun = new();
    public Networklist<bool> canActivePlayer = new();
    public Networklist<int> canActivePlayerTurnNum = new();
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
        return canActivePlayer[userId].value;
    }
    public void SetAblePlayerBoolValue(int userId)
    {
        canActivePlayer[userId].value = !canActivePlayer[userId];
    }
    public void RequsetDelayAblePlayerTurnNum(int delayTime, int userId)
    {
        canActivePlayerTurnNum[userId].value = delayTime + totalTrunNum.Value;
        ComparisonAblePlayer();
    }
    public void ComparisonAblePlayer()
    {
        for(int i = 0; i < GameManager.instance.SendPlayerTotalNum(); i++)
        {
            if(canActivePlayerTurnNum[i].value > totalTrunNum.Value)
            {
                canActivePlayer[i].value = false;
            }
            else
            {
                canActivePlayer[i].value = true;
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
}
