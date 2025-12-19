using System.Collections;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public EnemyCulGroup enemyCulGroup;
    public CardSpaceCheck cardSpaceCheck;

    public TextMeshProUGUI stageTxt;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeSameTextStage(int bigStageNum, int smallStageNum)
    {
        
    }
    public void ReciveSignToChangeTrun()
    {
        StartCoroutine(WaitForDamageAndEffect(GameManager.Instance.isPlayerTrun.Value));
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

        cardSpaceCheck.CardSpacePrefabsInit(isPlayerTrun); // 몬스터 턴
        GameManager.Instance.totalTrunNum.Value++;
        print("턴 증가!");
    }
}
