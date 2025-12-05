using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChooseEnemyOrTeam : MonoBehaviour
{
    public StateCulManager stateCulManager;
    public JobManager jobManager;
    public EnemyCulGroup enemyCulGroup;
    public GameObject chooseObj;
    public Button[] seletedBTN; 
    public bool isForPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        chooseObj.SetActive(false);
        foreach(Button btn in seletedBTN)
        {
            btn.gameObject.SetActive(false);
        }
    }

    public void SetUpOnChooseEnemyOrTeam(bool isToPlayer, bool isMe) // 아군 및 적군 선택 화면 및 선택 기능
    {
        isForPlayer = isToPlayer;

        if(isForPlayer)
        {
            if(isMe || (GameManager.Instance.playerTotalNum.Value == 1))
            {
                stateCulManager.ReciveAnotherUserID(GameManager.Instance.playerTotalNum.Value, isForPlayer);
                return;
            }
            else
            {
                for(int i = 0; i < GameManager.Instance.playerTotalNum.Value; i++)
                {
                    if((ulong)i != NetworkManager.Singleton.LocalClientId)
                    {
                        Image icon = seletedBTN[i].image;
                        string iconString = GameManager.Instance.playerJobs[i].ToString();
                        jobManager.ChangeImgToIcon(icon, iconString);
                        seletedBTN[i].gameObject.SetActive(true);
                    }
                }    
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
                stateCulManager.ReciveAnotherUserID(0, isForPlayer);
                return;
            }
            for(int i = 0; i < enemyCulGroup.enemyPrefabs.Count(); i++)
            {
                if(enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                {
                    seletedBTN[i].image.sprite = enemyCulGroup.enemyPrefabs[i].image.sprite;
                }
            }
        }
        chooseObj.SetActive(true);
    }

    public void EndOfSelect(int index) // 선택이 끝났을 때
    {
        stateCulManager.ReciveAnotherUserID(index, isForPlayer);
    }
}
