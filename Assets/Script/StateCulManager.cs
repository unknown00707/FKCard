using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using System.Collections;

public class StateCulManager : MonoBehaviour
{
    public JobManager jobManager;
    public CardSpaceCheck cardSpaceCheck;
    public CardEffectAndCulDuringManager cEACDManager;
    public ChooseEnemyOrTeam chooseEnemyOrTeam;
    public PlayerJobSkill playerJobSkill;
    CardPlayer player;
    public GameObject canvers;
    public int initalCardN = 4;
    public static int MAXCARDNUM = 200;
    public Transform userT;
    public GameObject minJCardsPerfab;

    [Header("stat")]
    public float userHP = 100;
    public float userDamge = 10;
    public float userCRIP = 1; // 치명타 퍼센트 100%
    public ulong userID;
    [Header("Card")]
    public string cardName;
    public float cardHp;
    public float cardDamage;
    public float cardCRIP;
    public float cardHeal;
    public int cardIndex;
    public JobManager.Jobs cardeType;
    public Sprite cardImage;
    public string cardExplain;
    public bool isAttackCard;
    [Header("CardEp")] //설명 explain
    public GameObject cardEpObj;
    public TextMeshProUGUI cardEpNameTxt;
    public TextMeshProUGUI cardEpTxt;
    public TextMeshProUGUI cardEpHPTxt;
    public TextMeshProUGUI cardEpDMTxt; 
    public Image cardEpSprite;
    [Header("EnemyEp")]
    public string enemyCardName;
    public string enemyCardEpTxt;
    public string enemyCardEpHPTxt;
    public string enemyCardEpDMTxt; 
    public Sprite enemyEpSprite;

    [Header("JobCardPrefab")]
    public Button[] initEmptyObjs = new Button[MAXCARDNUM];
    public List<Button> playerEmptyObjs = new();
    public Button[] defenderObjPrefabs;
    public Button[] knightObjPrefabs;
    public Button[] wizardObjPrefabs;
    public Button[] healderObjPrefabs;
    public Button[] bufferObjPrefabs;
    public Button[] jokerObjPrefabs;
    public Button[] convictObjPrefabs;
    public Button[] unemployedObjPrefabs;
    private Dictionary<JobManager.Jobs, Button[]> _jobCardPrefabs;

    void Awake()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeJobCardDictionary();
        StartInit();
    }
    void Update()
    {
        if (player == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
        }
    }
    void InitializeJobCardDictionary()
    {
        _jobCardPrefabs = new Dictionary<JobManager.Jobs, Button[]>
        {
            { JobManager.Jobs.defender, defenderObjPrefabs },
            { JobManager.Jobs.knight, knightObjPrefabs },
            { JobManager.Jobs.wizard, wizardObjPrefabs },
            { JobManager.Jobs.healler, healderObjPrefabs },
            { JobManager.Jobs.buffer, bufferObjPrefabs },
            { JobManager.Jobs.joker, jokerObjPrefabs },
            { JobManager.Jobs.convict, convictObjPrefabs }
            // 'unemployed'는 로직이 특이해서 여기서는 제외합니다.
        };
    }
    void StartInit()
    {
        // pool sys
        for(int i = 0; i < MAXCARDNUM; i++)
        {
            GameObject obj = Instantiate(minJCardsPerfab, userT.position, Quaternion.identity);
            obj.transform.SetParent(this.transform);
            obj.SetActive(false);

            initEmptyObjs[i] = obj.GetComponent<Button>();
        }

        userID = NetworkManager.Singleton.LocalClientId;
    }
    public void InitCardPlayer()
    {
        float[] userStatArray = jobManager.SendTheJobStat(); // userStat Init
        userHP = userStatArray[0];
        userDamge = userStatArray[1];
        userCRIP = userStatArray[2];

        // '백수'는 로직이 완전히 다르므로 별도 처리
        if (jobManager.userJobState == JobManager.Jobs.unemployed)
        {
            for (int i = 0; i < 2; i++)
            {
                int unemployedIndex = RandomInitCardNum(8);
                switch(unemployedIndex)
                {
                    case 0:
                        int defenderIndex = RandomInitCardNum(defenderObjPrefabs.Length);
                        ReciveValueDataCard(defenderObjPrefabs[defenderIndex].GetComponent<CardData>());
                        CardInitEmptyToJobs(true, defenderIndex);  
                        break; 
                    case 1:
                        int knightIndex = RandomInitCardNum(knightObjPrefabs.Length);
                        ReciveValueDataCard(knightObjPrefabs[knightIndex].GetComponent<CardData>());
                        CardInitEmptyToJobs(true, knightIndex);  
                        break; 
                    case 2:
                        int wizardIndex = RandomInitCardNum(wizardObjPrefabs.Length);
                        ReciveValueDataCard(wizardObjPrefabs[wizardIndex].GetComponent<CardData>());
                        CardInitEmptyToJobs(true, wizardIndex);  
                        break; 
                    case 3:
                        int heallerIndex = RandomInitCardNum(healderObjPrefabs.Length);
                        ReciveValueDataCard(healderObjPrefabs[heallerIndex].GetComponent<CardData>());
                        CardInitEmptyToJobs(true, heallerIndex);  
                        break; 
                    case 4:
                        int bufferIndex = RandomInitCardNum(bufferObjPrefabs.Length);
                        ReciveValueDataCard(bufferObjPrefabs[bufferIndex].GetComponent<CardData>());
                        CardInitEmptyToJobs(true, bufferIndex); 
                        break; 
                    case 5:
                        int jokerIndex = RandomInitCardNum(jokerObjPrefabs.Length);
                        ReciveValueDataCard(jokerObjPrefabs[jokerIndex].GetComponent<CardData>());
                        CardInitEmptyToJobs(true, jokerIndex);  
                        break; 
                    case 6:
                        int convictIndex = RandomInitCardNum(convictObjPrefabs.Length);
                        ReciveValueDataCard(convictObjPrefabs[convictIndex].GetComponent<CardData>());
                        CardInitEmptyToJobs(true, convictIndex);  
                        break; 
                }
            }
        }
        // '백수'를 제외한 모든 직업은 이 통합 로직을 사용
        else if (_jobCardPrefabs.TryGetValue(jobManager.userJobState, out Button[] cardPrefabs))
        {
            for (int i = 0; i < initalCardN; i++)
            {
                int cardIndex = RandomInitCardNum(cardPrefabs.Length);
                ReciveValueDataCard(cardPrefabs[cardIndex].GetComponent<CardData>());
                CardInitEmptyToJobs(true, cardIndex);
            }
        }

        player.ReciveGameSetReadySing(userHP, userDamge, userCRIP);
    }
    void CardInitEmptyToJobs(bool isInit , int index)
    {
        for(int i = 0; i < MAXCARDNUM; i++)
        {      
            if(!initEmptyObjs[i].gameObject.activeInHierarchy)
            {
                DataMakeSameByObj(initEmptyObjs[i], index);
                playerEmptyObjs.Add(initEmptyObjs[i]);
                initEmptyObjs[i].gameObject.transform.SetParent(userT);
                initEmptyObjs[i].gameObject.SetActive(true);
                break;
            }
        }
    }
    
    // 데이터 동기화 함수
    void DataMakeSameByObj(Button obj, int index)
    {
        // 이미지 설정
        obj.image.sprite = cardImage;
        // 이름 / 수치 설정
        TextMeshProUGUI[] textpro = obj.GetComponentsInChildren<TextMeshProUGUI>();
        textpro[0].text = cardName;  
        textpro[1].text = cardHp.ToString();  
        textpro[2].text = cardDamage.ToString();  
        textpro[0].color = Color.black;
        textpro[1].color = Color.white;
        textpro[2].color = Color.white;
        //데이터 동기화
        CardData card = obj.GetComponent<CardData>();
        card.hp = cardHp;
        card.damage = cardDamage;
        card.cRIP = cardCRIP;
        card.heal = cardHeal;
        card.index = cardIndex;
        card.type = cardeType;
        card.image = cardImage;
        card.cname  = cardName;
        card.cardExplain = cardExplain;
        // 이름 변경
        switch(cardeType)
        {
            case JobManager.Jobs.defender:
                obj.gameObject.name = defenderObjPrefabs[index].gameObject.name;
                break;
            case JobManager.Jobs.knight:
                obj.gameObject.name = knightObjPrefabs[index].gameObject.name;
                break;
            case JobManager.Jobs.wizard:
                obj.gameObject.name = wizardObjPrefabs[index].gameObject.name;
                break;
            case JobManager.Jobs.healler:
                obj.gameObject.name = healderObjPrefabs[index].gameObject.name;
                break;
            case JobManager.Jobs.buffer:
                obj.gameObject.name = bufferObjPrefabs[index].gameObject.name;
                break;
            case JobManager.Jobs.joker:
                obj.gameObject.name = jokerObjPrefabs[index].gameObject.name;
                break;
            case JobManager.Jobs.convict:
                obj.gameObject.name = convictObjPrefabs[index].gameObject.name;
                break;
        }
    }
    public void CardSameMakeObjPublic(JobManager.Jobs job, int cardID, GameObject obj)
    {
        // prefab에서 데이터 추출
        switch(job)
        {
            case JobManager.Jobs.defender:
                ReciveValueDataCard(defenderObjPrefabs[cardID].GetComponent<CardData>());
                break;
            case JobManager.Jobs.knight:
                ReciveValueDataCard(knightObjPrefabs[cardID].GetComponent<CardData>());
                break;
            case JobManager.Jobs.wizard:
                ReciveValueDataCard(wizardObjPrefabs[cardID].GetComponent<CardData>());
                break;
            case JobManager.Jobs.healler:
                ReciveValueDataCard(healderObjPrefabs[cardID].GetComponent<CardData>());
                break;
            case JobManager.Jobs.buffer:
                ReciveValueDataCard(bufferObjPrefabs[cardID].GetComponent<CardData>());
                break;
            case JobManager.Jobs.joker:
                ReciveValueDataCard(jokerObjPrefabs[cardID].GetComponent<CardData>());
                break;
            case JobManager.Jobs.convict:
                ReciveValueDataCard(convictObjPrefabs[cardID].GetComponent<CardData>());
                break;
            case JobManager.Jobs.unemployed:
                ReciveValueDataCard(unemployedObjPrefabs[cardID].GetComponent<CardData>());
                break;
        }

        // 데이터 동기화
        Button changingObjData = obj.GetComponent<Button>();
        DataMakeSameByObj(changingObjData, cardID);
    }

    int RandomInitCardNum(int max)
    {
        return UnityEngine.Random.Range(0, max);
    }

    public void ShowTheDetailCard(CardData cardData, bool isTure)
    {
        ReciveValueDataCard(cardData);

        cardEpNameTxt.text = cardName;
        cardEpTxt.text = cardExplain;
        cardEpHPTxt.text = cardHp.ToString();
        cardEpDMTxt.text = cardDamage.ToString();
        cardEpSprite.sprite = cardImage;

        cardEpObj.SetActive(isTure);
    }

    // 카드 데이터에서 정보 가지고 오기
    public void ReciveValueDataCard(CardData cardData)
    {
        cardName = cardData.cname;
        cardHp = cardData.hp;
        cardDamage = cardData.damage;
        cardCRIP = cardData.cRIP;
        cardHeal = cardData.heal;
        cardIndex = cardData.index;
        cardeType = cardData.type;
        cardImage = cardData.image;
        cardExplain = cardData.cardExplain;
    }
    // 적 카드 데이터 정보 가지고 오기
    public void ReciveValueEnemyCardData(EnemyCardData enemyCardData)
    {
        enemyCardName = enemyCardData.enemyName;
        enemyCardEpTxt = enemyCardData.ep;
        enemyCardEpHPTxt = enemyCardData.enemyHP.ToString();
        enemyCardEpDMTxt = enemyCardData.enemyDamage.ToString();
        enemyEpSprite = enemyCardData.img;
    }
    public void ShowTheDetailEnemyCard(EnemyCardData enemyCardData, bool isTure)
    {
        ReciveValueEnemyCardData(enemyCardData);
        cardEpNameTxt.text = enemyCardName;
        cardEpTxt.text = enemyCardEpTxt;
        cardEpHPTxt.text = enemyCardEpHPTxt;
        cardEpDMTxt.text = enemyCardEpDMTxt;
        cardEpSprite.sprite = enemyEpSprite;

        cardEpObj.SetActive(isTure);
    }

    void OnDragin()
    {
        DragFocution(true); // Drag in
    }

    void OnDragout()
    {
        DragFocution(false); // Dray out

        //plyaer.Damage(cardDamage);
    }

    // false -> drag out / true -> drag in
    void DragFocution(bool isIn)
    {
        GraphicRaycaster graphicRaycaster = canvers.GetComponent<GraphicRaycaster>();

        PointerEventData pointerEventData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new();

        graphicRaycaster.Raycast(pointerEventData, raycastResults);
        
        foreach(RaycastResult raycastResult in raycastResults)
        {
            if(raycastResult.gameObject.CompareTag("Card"))
            {
                Vector3 pos = raycastResult.gameObject.transform.position;
                Transform tr = raycastResult.gameObject.transform;

                raycastResult.gameObject.GetComponent<CardData>().PosSameThePoint(isIn);

                if(tr.parent == userT && isIn == false)
                {
                    for(int i = 0; i < playerEmptyObjs.Count(); i++)
                    {
                        if(raycastResult.gameObject.GetComponent<Button>() == playerEmptyObjs[i])
                        {
                            // 카드 위치를 원 위치로 돌리기
                            tr.SetParent(transform);
                            tr.position = pos;
                            tr.SetParent(userT);

                            tr.SetSiblingIndex(i);
                            print("카드 패로 돌아가기!");
                        }
                    }
                }
            }
        }
    }

    // 카드 종류 확인해서 type 에 해당하는 함수 실행
    public void CardSearchMatch()
    {
        switch (cardeType)
        {
            case JobManager.Jobs.defender:
                playerJobSkill.DefenderSkills(cardIndex);
                break;
        }
    }
}
