using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class JobManager : MonoBehaviour
{
    public StateCulManager stateCulManager;

    [Header("JobStat")]
    public float jobHp;
    public float jobDG;
    public float jobCIRP;

    [Header("Explain")]
    public Text jobName;
    public Text jobExplain;
    [Header("ReadyAbout")]
    public GameObject seletedObj;
    public GameObject jobsObj;
    public GameObject readyRoomObj;
    public GameObject[] playerReadyObjs;
    public GameObject gameStartBTN;
    public bool isReady = false;

    public enum Jobs
    {
        defender, // 탱커
        knight, // 근접 딜러
        wizard, // 광역 딜러
        buffer, // 버퍼
        healler, // 힐러 
        joker, // 전략가
        unemployed, // 백수, 빈털털이  : 카드가 6장
        convict // 죄수 : 트롤 1장  - 성능 카드 2장 
    }
    
    public Jobs userJobState; // 결정한 직업
    private string unJobState; // 임시 직업 , 선택한 직업

    Dictionary<string , string> jobPassive = new()
    {
        { "탱커", "단일공격을 모두 자신이 받음 / 모든 피해를 일부분 차감시켜 받음 / 체력 높음"},
        { "근접 딜러", "치명타 확률 증가 / 일정확률로 피해없음 / 공격력 높음"},
        { "광역 딜러", " 카드를 2장씩 소모가능 / 공격력 높음"},
        { "힐러", "아군 전부 사망시 게임당 한번 전부 부활 / 힐 카드 효율 좋음"},
        { "버퍼", "턴이 흐를 때마다 스탯 증가 / 카드를 모두 소모하면 아군도 스탯증가 / 체력 높음"},
        { "전략가", "카드획득,강화 5회마다 2배로 적용 / 모든 아군이 회마다 리롤 1번씩"},
        { "빈털털이", "카드획득 배율이 1.5배 (4개=6개)로 적용 / 전투중에 아군에게 카드양도 가능인데 카드 0개되면 바로 사망"},
        { "죄수", "카드획득 배율이 0.5배 (4개=2개)로 적용 / 자기가 뽑은 카드의 수치가 1.5배 / 공격력 높음"},
    };
    Dictionary<int, string> arryValueList = new()
    {
        {0, "탱커"},
        {1, "근접 딜러"},
        {2, "광역 딜러"},
        {3, "힐러"},
        {4, "버퍼"},
        {5, "전략가"},
        {6, "빈털털이"},
        {7, "죄수"},
    };
    public Sprite[] jobIcons;
    // Dictionary<string , string> jobPassive = new Dictionary<string, string>()
    // {
    //     { "defender", "단일공격을 모두 자신이 받음 / 모든 피해를 일부분 차감시켜 받음 / 체력 높음"},
    //     { "knight", "치명타 확률 증가 / 일정확률로 피해없음 / 공격력 높음"},
    //     { "wizard", " 카드를 2장씩 소모가능 / 공격력 높음"},
    //     { "healler", "아군 전부 사망시 게임당 한번 전부 부활 / 힐 카드 효율 좋음"},
    //     { "buffer", "턴이 흐를 때마다 스탯 증가 / 카드를 모두 소모하면 아군도 스탯증가 / 체력 높음"},
    //     { "joker", "카드획득,강화 5회마다 2배로 적용 / 모든 아군이 회마다 리롤 1번씩"},
    //     { "unemployed", "카드획득 배율이 1.5배 (4개=6개)로 적용 / 전투중에 아군에게 카드양도 가능인데 카드 0개되면 바로 사망"},
    //     { "convict", "카드획득 배율이 0.5배 (4개=2개)로 적용 / 자기가 뽑은 카드의 수치가 1.5배 / 공격력 높음"},
    // };


    //탱커 : 자기카드 쓰면 최대체력의 3%씩 일시적 추가체력
    //근딜 : 자기카드 쓰면 치명타 확률 10% 추가적용
    //광딜 : 자기카드 쓰면 공격력 +10%
    //버퍼 : 자기카드 쓸 때마다 그 스테이지에서 아군 스탯 +1
    //힐러 : 자기카드 쓰면 카드 스탯 2배로 증가
    //전략가 : 자기카드 쓰면 25%확률로 카드 반환
    //빈털털이 : 없음
    //죄수 : 자기카드 쓰면 100%확률로 아군의 카드중 1개를 복사해서 50%위력으로 발동
    void Start()
    {
        jobName.text = arryValueList[0];
        jobExplain.text = jobPassive[arryValueList[0]];

        HideStartButton();
        GoToReadyRoom(false);
    }

    void HideStartButton()
    {
        if (NetworkManager.Singleton != null)
        {
            // 현재 인스턴스가 Host(서버 + 클라이언트) 또는 Server 전용인지 확인
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                gameStartBTN.SetActive(true);
            }
            else
            {
                gameStartBTN.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("NetworkManager not found. Assuming single player context.");
        }
    }

    public void ChangeName(int index)
    {
        string str = arryValueList[index];
        jobName.text = str;
        jobExplain.text = jobPassive[str];
        unJobState = str;
    }

    // user Type
    // public void SelectUserJob()
    // {
    //     switch(unJobState)
    //     {
    //         case "defender":
    //             userJobState = Jobs.defender;
    //             break; 
    //         case "knight":
    //             userJobState = Jobs.knight;
    //             break; 
    //         case "wizard":
    //             userJobState = Jobs.wizard;
    //             break; 
    //         case "healler":
    //             userJobState = Jobs.healler;
    //             break; 
    //         case "buffer":
    //             userJobState = Jobs.buffer;
    //             break; 
    //         case "joker":
    //             userJobState = Jobs.joker;
    //             break; 
    //         case "unemployed":
    //             userJobState = Jobs.unemployed;
    //             break; 
    //         case "convict":
    //             userJobState = Jobs.convict;
    //             break;
    //     }
    // }
    public void SelectUserJob()
    {
        switch(unJobState)
        {
            case "탱커":
                userJobState = Jobs.defender;
                jobHp = 500;
                jobDG = 100;
                jobCIRP = 1;
                break; 
            case "근접 딜러":
                userJobState = Jobs.knight;
                jobHp = 250;
                jobDG = 450;
                jobCIRP = 30;
                break; 
            case "광역 딜러":
                userJobState = Jobs.wizard;
                jobHp = 250;
                jobDG = 500;
                jobCIRP = 1;
                break; 
            case "힐러":
                userJobState = Jobs.healler;
                jobHp = 350;
                jobDG = 100;
                jobCIRP = 1;
                break; 
            case "버퍼":
                userJobState = Jobs.buffer;
                jobHp = 250;
                jobDG = 100;
                jobCIRP = 1;
                break; 
            case "전략가":
                userJobState = Jobs.joker;
                jobHp = 200;
                jobDG = 100;
                jobCIRP = 1;
                break; 
            case "빈털털이":
                userJobState = Jobs.unemployed;
                jobHp = 100;
                jobDG = 100;
                jobCIRP = 1;
                break; 
            case "죄수":
                userJobState = Jobs.convict;
                jobHp = 50;
                jobDG = 50;
                jobCIRP = 1;
                break;
        }

        var plyaer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
        plyaer.ReciveJobs(userJobState);
    }

    public void GoToReadyRoom(bool isGotoRoom)
    {
        jobsObj.SetActive(!isGotoRoom);
        readyRoomObj.SetActive(isGotoRoom);
    }
    public float[] SendTheJobStat()
    {
        float[] a = new float[3];
        a[0] = jobHp;
        a[1] = jobDG;
        a[2] = jobCIRP;

        return a;
    }

    public void ReciveJobsDataPublic(NetworkList<FixedString64Bytes> playerJobs, int index)
    {
        for(int i = 0; i < index; i++)
        {
            Image icon = playerReadyObjs[i].GetComponentInChildren<Image>();
            TextMeshProUGUI txt = playerReadyObjs[i].GetComponentInChildren<TextMeshProUGUI>();

            if(i == (int)NetworkManager.Singleton.LocalClientId)
                txt.text = playerJobs[i].ToString()+ "\n" + "'나'";
            else
                txt.text = playerJobs[i].ToString()+ "\n" + "";

            switch(playerJobs[i].ToString())
            {
                case "defender":
                    icon.sprite = jobIcons[0];
                    break; 
                case "knight":
                    icon.sprite = jobIcons[1];
                    break; 
                case "wizard":
                    icon.sprite = jobIcons[2];
                    break; 
                case "healler":
                    icon.sprite = jobIcons[3];
                    break; 
                case "buffer":
                    icon.sprite = jobIcons[4];
                    break; 
                case "joker":
                    icon.sprite = jobIcons[5];
                    break; 
                case "unemployed":
                    icon.sprite = jobIcons[6];
                    break; 
                case "convict":
                    icon.sprite = jobIcons[7];
                    break;
            }
        }
        
        if(playerJobs.Count > index)
        {
            for(int i = playerJobs.Count; i < index; i--)
            {
                print(i);
                Image icon = playerReadyObjs[i-1].GetComponentInChildren<Image>();
                TextMeshProUGUI txt = playerReadyObjs[i-1].GetComponentInChildren<TextMeshProUGUI>();

                icon.gameObject.SetActive(false);
                txt.text = "";
            }
        }
    }

    public void ReadyBTNFouction()
    {
        isReady = !isReady;
        var plyaer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
        plyaer.ReciveReadySign(isReady);
    }

    public void ReciveReadySignPublic(NetworkList<bool> playerReady)
    {
        for(int i = 0; i < playerReadyObjs.Count(); i++)
        {
            TextMeshProUGUI txt = playerReadyObjs[i].GetComponentInChildren<TextMeshProUGUI>();
            if(playerReady[i])
                txt.color = Color.green;
            else
                txt.color = Color.black;
        }
    }

    // 게임 시작 관련 기능
    public void GameStartFouction()
    {
        var plyaer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
        plyaer.ReciveGameStartSign();
    }

    [ClientRpc]
    public void RequestGameStartSignClientRpc()
    {
        GoToReadyRoom(false);
        seletedObj.SetActive(false);
        stateCulManager.InitCardPlayer();
    }
}
