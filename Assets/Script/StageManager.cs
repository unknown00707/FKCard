using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class StageManager : NetworkBehaviour
{
    public TurnManager turnManager;
    public NetworkVariable<int> stageNum = new(0); // 스테이지 큰 수
    public NetworkVariable<int> sideStageNum = new(0); // 스테이지 작은 수
    public EnemyCulGroup enemyCulGroup;
    public CardSpaceCheck cardSpaceCheck;

    public TextMeshProUGUI stageTxt;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        stageNum.OnValueChanged += OnStageNumChanged;
        sideStageNum.OnValueChanged += OnSideStageNumChanged;
    }

    // Update is called once per frame
    public override void OnNetworkDespawn()
    {
        stageNum.OnValueChanged -= OnStageNumChanged;
        sideStageNum.OnValueChanged -= OnSideStageNumChanged;
    }
    public override void OnDestroy()
    {
        stageNum?.Dispose();
        sideStageNum?.Dispose();
    }
    void OnStageNumChanged(int oldValue, int newValue)
    {
        MakeSameTextStage();
    }
    void OnSideStageNumChanged(int oldValue, int newValue)
    {
        
    }

    public void MakeSameTextStage()
    {
        stageTxt.text = stageNum.Value + " : " + sideStageNum.Value;
    }
    public void ReciveSignToChangeTrun()
    {
        StartCoroutine(WaitForDamageAndEffect(turnManager.isPlayerTrun.Value));
    }
    IEnumerator WaitForDamageAndEffect(bool isPlayerTrun)
    {
        if(!isPlayerTrun)
        {
            print("몬스터의 턴!");
            
            enemyCulGroup.RequsetTheDamageToMonster();
            // 턴에 해당하는 보스에게 데미지 주는 코드 . . .
        }
        else
            print("플레이어 턴!");
        yield return new WaitForSecondsRealtime(5f);

        cardSpaceCheck.CardSpacePrefabsInit(isPlayerTrun); // 카드 초기 상태로 변경

        print("턴 효과 적용 완료!");
    }
    public int GiveStageNum()
    {
        return stageNum.Value;
    }
}
