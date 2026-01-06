using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public EnemyCulGroup enemyCulGroup;
    public NetworkVariable<int> totalTrunNum = new();
    public NetworkVariable<bool> isPlayerTrun = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        isPlayerTrun.Value = true;
        totalTrunNum.Value = 0;

        isPlayerTrun.OnValueChanged += OnPlayerTrunIsOn;
    }

    // Update is called once per frame
    public void DisInit()
    {
        isPlayerTrun.OnValueChanged -= OnPlayerTrunIsOn;
    }
    public void DisposeFounction()
    {
        isPlayerTrun?.Dispose();
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
