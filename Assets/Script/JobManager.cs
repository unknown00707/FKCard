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
    CardPlayer player;

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
    public Dictionary<int, string> arryValueList = new()
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

    //탱커 : 자기카드 쓰면 최대체력의 3%씩 1턴 추가체력
    //근딜 : 자기카드 쓰면 치명타 확률 10% 추가적용
    //광딜 : 자기카드 쓰면 공격력 +10%
    //버퍼 : 자기카드 쓸 때마다 그 스테이지에서 아군 스탯 +1
    //힐러 : 자기카드 쓰면 카드 스탯 2배로 증가
    //전략가 : 자기카드 쓰면 25%확률로 카드 반환
    //빈털털이 : 없음
    //죄수 : 자기카드 쓰면 100%확률로 아군의 카드중 1개를 복사해서 50%위력으로 발동

    /*
    몹 수식어 (후반가면 2개이상도 적용되는 식으로)
딱총새 : 모든 단일공격의 위력이 30% 증가
교주 : 15%의 공/체를 지닌 분신 2마리 소환 (본체 죽으면 클리어)
대마법사 : 모든 공격의 위력이 30% 감소하지만 광역 공격으로 변경
사냥꾼 : 매턴마다 한명의 턴을 금지 
괴물 : 공/체 +20%
영양사 : 모든 이로운 효과의 50%를 갈취해서 자신한테 적용
작살잡이 : 모든 타겟관련 효과 무시 (고기방패 무시)
약자 : 플레이어들의 모든 공격이 30%추가위력 / 30%확률로 아군에게 향함
버섯 : 매 턴마다 모두에게 최대체력 5% 피해
강약약강 : 현재체력이 가장 낮은 플레이어 공격
도둑 : 최대체력이 50/30/10%가 될때마다 플레이어의 카드하나를 뺏어 사용 (뺏기전에 죽이기 가능)
전사 : 최대체력이 가장 높은 플레이어 공격 / 매 공격마다 플레이어의 체력의 숫자 반전 (63이면 36으로 십의자리까지만) / 클리어하면 몹이 죽을 때 체력상태 그대로 진행
임금님 : 옆에 잡몹들 죽기전까지 무적
양손잡이 : 플레이어와 보스의 턴 모두 2번씩 (턴 +1)
광대 : 모든 플레이어의 카드를 매턴 서로서로 변경 (클리어하면 쓴거 빼고 원상복구)

보스 능력
4턴마다 각 플레이어의 직업을 각자의 직업중에서 랜덤하게 변경
클리어시 플레이어의 남은 체력 50%를 차감 (자폭)
플레이어중 한명이 3턴이상이 지난 이후 랜덤한 턴마다 보스가 받을 피해의 50%를 대신 맞음
모든 공격이 광역공격
모든 플레이어가 자신의 카드를 확인할 수 없게 됌 (카드가 뒤섞이고 모든 표기가 가려짐 / 전략가를 통해 해결가능)
2턴마다 모든 플레이어의 카드를 1장 삭제
직업카드가 모두 소진된 플레이어를 즉시 사망처리
보스가 받는 모든피해의 20%를 1명에게 반사

탱커 : 1턴동안 최대체력의 15% 추가체력 / 최대체력의 10%만큼 공격 / 클리어할 때까지 매턴 5%씩 중첩되는 추가체력 / 1턴동안 받은 피해의 50%를 반사 / 1턴동안 받은 피해의 30%를 다음턴의 추가체력으로 전환 / 3턴동안 받는 피해 20% 증가 아군의 공격력도 15% 증가
근딜 : 공격력의 50%로 2회 공격 / 공격력의 120%로 1회 공격 / 매턴 공격력의 20% 독공격 / 3턴간 행동불가,이후 100%공격력으로 3회 공격 / 1턴간 행동불가,다음공격 치명타 확률 40% 증가 / 2턴동안 적이 받은 모든 피해의 30%로 공격
광역딜 : 공격력의 70%로 광역공격 / 3턴간 공격력 30% 증가 / 공격력의 30%로 5턴간 지속공격 (단일) / 최대체력의 50%를 자해하고 이후 공격력의 150%로 광역공격 / 공격력의 300%로 광역공격,아군 전체도 최대체력의 30%로 피해
힐러 : 최대체력의 15%단일 힐 /최대체력의 7.5% 광역 힐 / 자신이 받을 데미지를 아군 한명에게 75%위력으로 대타세우기 / 랜덤하게 3%~15% 광역 힐 / 추가체력 부여 / 아군 한명 30%체력으로 부활 / 매 턴 최대체력 5% 치유 / 3턴간 아군 1명이 받는 모든 이로운 효과 1.5배
전략가 : 다음 턴 모든 아군이 낼 수 있는 카드 +1 / 아군이 서로의 카드를 확인할 수 있음,2턴간 적의 다음 공격을 확인할 수 있음 / 아군 1명을 지정하여 아군 전체가 받을 피해를 75%로 감소하여 받음 / 사망한 아군의 카드 일부를 복사하여 일시적으로 살아있는 아군전원에게 지급 / 다음 몹의 수식어를 +1~2부여하는 대신 보상선택지를 +1 고를 수 있게 됌 / 모든 아군이 무작위 직업카드를 1장 즉시 획득 / 다음턴부터 아군이 선제공격권을 얻게 됌
죄수 : 트롤카드 - 아군 한명에게 공50%로 피해입히기 / 이번턴에 받는 모든 이로운 효과를 자신이 받음 / 50%확률로 모든 아군의 공격을 자신이 받음 혹은 150%의 위력으로 적에게 가함 / 30%확률로 모든 아군의 공격이 빗나감 / 최대체력이 가장 높은아군과 낮은 아군의 수치 변경 / 아군 2명이 다음 턴을 넘김 / 다음 스테이지를 건너뛰고 보상도 건너뜀                    일반카드 - 최근 5턴간 아군적군 구분하지 않고 자신이 입힌 피해의 50%만큼 공격
버퍼 : 아군 한명에게 딱총새/괴물/버섯중 무작위로 하나의 수식어를  2턴간 적용 / 3턴동안 아군 1명이 받은피해의 30%를 반사 / 1턴동안 무작위아군이 받은 피해의 x0.5만큼 아군 스탯 증가 / 무작위 카드 3장 소실,4턴간 아군이 받는 이로운 효과 1.5배 / 무작위 아군의 카드 2장을 강화시킴 / 모든 아군의 공격력 25% 증가 
빈털털이 : 모든 직업카드중 2장 무작위로 획득
    */
    void Awake()
    {
        unJobState = "탱커";
    }
    void Start()
    {
        jobName.text = arryValueList[0];
        jobExplain.text = jobPassive[arryValueList[0]];

        HideStartButton();
        GoToReadyRoom(false);
        seletedObj.SetActive(true);
    }

    void Update()
    {
        if (player == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
            SelectUserJob();
        }
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

        player.ReciveJobs(userJobState);
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
        // 안전장치: index보다 실제 리스트가 작을 경우 에러 방지
        int loopCount = Mathf.Min(playerJobs.Count, index);

        for(int i = 0; i < loopCount; i++)
        {
            Image icon = playerReadyObjs[i].GetComponentInChildren<Image>();
            TextMeshProUGUI txt = playerReadyObjs[i].GetComponentInChildren<TextMeshProUGUI>();

            if(i == (int)NetworkManager.Singleton.LocalClientId)
                txt.text = playerJobs[i].ToString()+ "\n" + "'나'";
            else
                txt.text = playerJobs[i].ToString()+ "\n" + "";

            icon.color = Color.white;
            ChangeImgToIcon(icon ,playerJobs[i].ToString());
        }
        
        if(playerJobs.Count > index)
        {
            for(int i = 0; i < playerJobs.Count - index; i++)
            {
                Image icon = playerReadyObjs[2-i].GetComponentInChildren<Image>();
                TextMeshProUGUI txt = playerReadyObjs[2-i].GetComponentInChildren<TextMeshProUGUI>();

                icon.color = Color.black;
                txt.text = "none";
            }
        }
    }
    public void ChangeImgToIcon(Image icon, string str)
    {
        switch(str)
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

    public void ReadyBTNFouction()
    {
        isReady = !isReady;
        player.ReciveReadySign(isReady);
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
        player.ReciveGameStartSign();
    }

    public void RequestGameStartSign()
    {
        GoToReadyRoom(false);
        seletedObj.SetActive(false);
        stateCulManager.InitCardPlayer();
    }
}
