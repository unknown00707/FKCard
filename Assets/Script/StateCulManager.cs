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

public class StateCulManager : MonoBehaviour
{
    public JobManager jobManager;
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

    [Header("CardEp")] //설명 explain
    public GameObject cardEpObj;
    public TextMeshProUGUI cardEpNameTxt;
    public TextMeshProUGUI cardEpTxt;
    public TextMeshProUGUI cardEpHPTxt;
    public TextMeshProUGUI cardEpDMTxt; 
    public Image cardEpSprite;

    [Header("JobCardPrefab")]
    public Button[] initEmptyObjs = new Button[MAXCARDNUM];
    public Button[] defenderObjPrefabs;
    public Button[] knightObjPrefabs;
    public Button[] wizardObjPrefabs;
    public Button[] healderObjPrefabs;
    public Button[] bufferObjPrefabs;
    public Button[] jokerObjPrefabs;
    public Button[] convictObjPrefabs;
    public Button[] unemployedObjPrefabs;

    void Awake()
    {
        if(NetworkManager.Singleton != null)
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartInit();
    }

    void StartInit()
    {
        // pool sys
        for(int i = 0; i < MAXCARDNUM; i++)
        {
            GameObject obj = Instantiate(minJCardsPerfab, userT.position, Quaternion.identity);
            obj.transform.SetParent(userT);
            obj.SetActive(false);

            initEmptyObjs[i] = obj.GetComponent<Button>();
        }
    }
    public void InitCardPlayer()
    {
        float[] userStatArray = jobManager.SendTheJobStat(); // userStat Init
        userHP = userStatArray[0];
        userDamge = userStatArray[1];
        userCRIP = userStatArray[2];

        // 초기 카드 활성화
        switch (jobManager.userJobState)
        {
            case JobManager.Jobs.defender:
                for (int i = 0; i < initalCardN; i++)
                {
                    int defenderIndex = RandomInitCardNum(defenderObjPrefabs.Length);
                    ReciveValueDataCard(defenderObjPrefabs[defenderIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(defenderIndex);    
                }
                break;
            case JobManager.Jobs.knight:
                for (int i = 0; i < initalCardN; i++)
                {
                    int knightIndex = RandomInitCardNum(knightObjPrefabs.Length);
                    ReciveValueDataCard(knightObjPrefabs[knightIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(knightIndex);
                }
                break;
            case JobManager.Jobs.wizard:
                for (int i = 0; i < initalCardN; i++)
                {
                    int wizardIndex = RandomInitCardNum(wizardObjPrefabs.Length);
                    ReciveValueDataCard(wizardObjPrefabs[wizardIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(wizardIndex);
                }
                break;
            case JobManager.Jobs.healler:
                for (int i = 0; i < initalCardN; i++)
                {
                    int heallerIndex = RandomInitCardNum(healderObjPrefabs.Length);
                    ReciveValueDataCard(healderObjPrefabs[heallerIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(heallerIndex);
                }
                break;
            case JobManager.Jobs.buffer:
                for (int i = 0; i < initalCardN; i++)
                {
                    int bufferIndex = RandomInitCardNum(bufferObjPrefabs.Length);
                    ReciveValueDataCard(bufferObjPrefabs[bufferIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(bufferIndex);
                }
                break;
            case JobManager.Jobs.joker:
                for (int i = 0; i < initalCardN; i++)
                {
                    int jokerIndex = RandomInitCardNum(jokerObjPrefabs.Length);
                    ReciveValueDataCard(jokerObjPrefabs[jokerIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(jokerIndex);
                }
                break;
            case JobManager.Jobs.convict:
                for (int i = 0; i < initalCardN; i++)
                {
                    int convictIndex = RandomInitCardNum(convictObjPrefabs.Length);
                    ReciveValueDataCard(convictObjPrefabs[convictIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(convictIndex);
                }
                break;
            case JobManager.Jobs.unemployed:
                for (int i = 0; i < 2; i++)
                {
                    int unemployedIndex = RandomInitCardNum(8);
                    switch(unemployedIndex)
                    {
                        case 0:
                            int defenderIndex = RandomInitCardNum(defenderObjPrefabs.Length);
                            ReciveValueDataCard(defenderObjPrefabs[defenderIndex].GetComponent<CardData>());
                            CardInitEmptyToJobs(defenderIndex);  
                            break; 
                        case 1:
                            int knightIndex = RandomInitCardNum(knightObjPrefabs.Length);
                            ReciveValueDataCard(knightObjPrefabs[knightIndex].GetComponent<CardData>());
                            CardInitEmptyToJobs(knightIndex);  
                            break; 
                        case 2:
                            int wizardIndex = RandomInitCardNum(wizardObjPrefabs.Length);
                            ReciveValueDataCard(wizardObjPrefabs[wizardIndex].GetComponent<CardData>());
                            CardInitEmptyToJobs(wizardIndex);  
                            break; 
                        case 3:
                            int heallerIndex = RandomInitCardNum(healderObjPrefabs.Length);
                            ReciveValueDataCard(healderObjPrefabs[heallerIndex].GetComponent<CardData>());
                            CardInitEmptyToJobs(heallerIndex);  
                            break; 
                        case 4:
                            int bufferIndex = RandomInitCardNum(bufferObjPrefabs.Length);
                            ReciveValueDataCard(bufferObjPrefabs[bufferIndex].GetComponent<CardData>());
                            CardInitEmptyToJobs(bufferIndex); 
                            break; 
                        case 5:
                            int jokerIndex = RandomInitCardNum(jokerObjPrefabs.Length);
                            ReciveValueDataCard(jokerObjPrefabs[jokerIndex].GetComponent<CardData>());
                            CardInitEmptyToJobs(jokerIndex);  
                            break; 
                        case 6:
                            int convictIndex = RandomInitCardNum(convictObjPrefabs.Length);
                            ReciveValueDataCard(convictObjPrefabs[convictIndex].GetComponent<CardData>());
                            CardInitEmptyToJobs(convictIndex);  
                            break; 
                    }
                    
                }
                break;
        }
    }
    void CardInitEmptyToJobs(int index)
    {
        for(int i = 0; i < MAXCARDNUM; i++)
        {      
            if(!initEmptyObjs[i].gameObject.activeInHierarchy)
            {
                // 이미지 설정
                initEmptyObjs[i].image.sprite = cardImage;

                // 이름 / 수치 설정
                TextMeshProUGUI[] textpro = initEmptyObjs[i].GetComponentsInChildren<TextMeshProUGUI>();
                textpro[0].text = cardName;  
                textpro[1].text = cardHp.ToString();  
                textpro[2].text = cardDamage.ToString();  
                textpro[0].color = Color.white;
                textpro[1].color = Color.white;

                //데이터 동기화
                CardData card = initEmptyObjs[i].GetComponent<CardData>();
                card.hp = cardHp;
                card.damage = cardDamage;
                card.cRIP = cardCRIP;
                card.heal = cardHeal;
                card.index = cardIndex;
                card.type = cardeType;
                card.image = cardImage;
                card.cname  = cardName;
                card.cardExplain = cardExplain;

                initEmptyObjs[i].gameObject.SetActive(true);

                switch(cardeType)
                {
                    case JobManager.Jobs.defender:
                        initEmptyObjs[i].gameObject.name = defenderObjPrefabs[index].gameObject.name;
                        break;
                    case JobManager.Jobs.knight:
                        initEmptyObjs[i].gameObject.name = knightObjPrefabs[index].gameObject.name;
                        break;
                    case JobManager.Jobs.wizard:
                        initEmptyObjs[i].gameObject.name = wizardObjPrefabs[index].gameObject.name;
                        break;
                    case JobManager.Jobs.healler:
                        initEmptyObjs[i].gameObject.name = healderObjPrefabs[index].gameObject.name;
                        break;
                    case JobManager.Jobs.buffer:
                        initEmptyObjs[i].gameObject.name = bufferObjPrefabs[index].gameObject.name;
                        break;
                    case JobManager.Jobs.joker:
                        initEmptyObjs[i].gameObject.name = jokerObjPrefabs[index].gameObject.name;
                        break;
                    case JobManager.Jobs.convict:
                        initEmptyObjs[i].gameObject.name = convictObjPrefabs[index].gameObject.name;
                        break;
                }
                break;
            }
        }
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

    void OnDragin()
    {
        DragFocution(true); // Drag in
    }

    void OnDragout()
    {
        DragFocution(false); // Dray out

        CardSearchMatch();
        //plyaer.Damage(cardDamage);
    }

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
                raycastResult.gameObject.GetComponent<CardData>().PosSameThePoint(isIn);
            }
        }
    }

    // 카드 종류 확인해서 type 에 해당하는 함수 실행
    void CardSearchMatch()
    {
        switch (cardeType)
        {
            case JobManager.Jobs.defender:
                DefenderSkills(cardIndex);
                break;
        }
    }

    // index에 따른 탱커 스킬
    void DefenderSkills(int index)
    {
        switch(index)
        {
            case 0:
                userHP += userHP*(15/100);
                break;
            case 1:
                //player.Damage(userHP*(10/100));
                break;
        }
    }
    
}
