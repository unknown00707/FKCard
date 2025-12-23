using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChooseEnemyOrTeam : MonoBehaviour
{
    public JobManager jobManager;
    public EnemyCulGroup enemyCulGroup;
    public PlayerJobSkill playerJobSkill;
    public TurnManager turnManager;
    public GameObject chooseObj;
    public Button[] seletedBTN; 
    public bool isForPlayer;
    public UnityEvent OnReadyToAttack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        BasciInit();
        OnReadyToAttack.AddListener(playerJobSkill.TriggerSkillFromChoosEnemyOrTeam);
    }
    void BasciInit()
    {
        foreach(Button btn in seletedBTN)
        {
            btn.gameObject.SetActive(false);
        }
        chooseObj.SetActive(false);
    }

    public void SetUpOnChooseEnemyOrTeam(bool isToPlayer, bool isMe) // 아군 및 적군 선택 화면 및 선택 기능
    {
        chooseObj.SetActive(true);
        isForPlayer = isToPlayer;

        if(isForPlayer)
        {
            if(isMe || (turnManager.GiveTurnValue() == 1))
            {
                print("유저가 한 명! 강제 실행 작동. . .");
                BasciInit();
                playerJobSkill.ReciveTargetUserIDFromChoose(NetworkManager.Singleton.LocalClientId, isForPlayer);
                OnReadyToAttack.Invoke();
                return;
            }
            else
            {
                for(int i = 0; i < turnManager.GiveTurnValue(); i++)
                {
                    
                    Image icon = seletedBTN[i].image;
                    string iconString = GameManager.Instance.playerJobs[i].ToString();
                    jobManager.ChangeImgToIcon(icon, iconString);
                    seletedBTN[i].gameObject.SetActive(true);
               
                }  
                print("아군이 다수! 섹션을 제작. . ."); 
            }
        }
        else
        {
            int activeNum = 0;
            foreach(Button btn in enemyCulGroup.enemyPrefabs)
            {
                if(btn.gameObject.activeInHierarchy)
                {
                    activeNum++;
                }
            }
            if(activeNum == 1)
            {
                print("적군이 한 명! 강제 실행 작동. . .");
                playerJobSkill.ReciveTargetUserIDFromChoose(0, isForPlayer);
                BasciInit();
                OnReadyToAttack.Invoke();
                return;
            }
            for(int i = 0; i < enemyCulGroup.enemyPrefabs.Count(); i++)
            {
                if(enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                {
                    seletedBTN[i].image.sprite = enemyCulGroup.enemyPrefabs[i].image.sprite;
                    seletedBTN[i].gameObject.SetActive(true);
                    print("적군이 다수! 섹션을 제작. . .");
                }
            }
        }
    }

    public void EndOfSelect(int index) // 선택이 끝났을 때
    {
        print("섹션 트리거 확인!");
        playerJobSkill.ReciveTargetUserIDFromChoose((ulong)index, isForPlayer);
        BasciInit();
        OnReadyToAttack.Invoke();
    }

    void OnDestroy()
    {
        OnReadyToAttack.RemoveListener(playerJobSkill.TriggerSkillFromChoosEnemyOrTeam);
    }
}
