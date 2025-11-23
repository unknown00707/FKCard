using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StateCulManager : MonoBehaviour
{
    public JobManager jobManager;
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

    [Header("JobCardPrefab")]
    public Button[] initEmptyObjs = new Button[MAXCARDNUM];
    public Button[] defenderObjPrefabs;
    public Button[] attackerObjPrefabs;
    public Button[] wizardObjPrefabs;
    public Button[] healderObjPrefabs;
    public Button[] bufferObjPrefabs;
    public Button[] jokerObjPrefabs;
    public Button[] unemployedObjPrefabs;
    public Button[] convictObjPrefabs;

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

        InitCardPlayer(); // test
    }
    public void InitCardPlayer()
    {
        
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
                    int knightIndex = RandomInitCardNum(attackerObjPrefabs.Length);
                    ReciveValueDataCard(attackerObjPrefabs[knightIndex].GetComponent<CardData>());
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
            case JobManager.Jobs.unemployed:
                for (int i = 0; i < initalCardN; i++)
                {
                    int unemployedIndex = RandomInitCardNum(unemployedObjPrefabs.Length);
                    ReciveValueDataCard(unemployedObjPrefabs[unemployedIndex].GetComponent<CardData>());
                    CardInitEmptyToJobs(unemployedIndex);
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

                //데이터 동기화
                CardData card = initEmptyObjs[i].GetComponent<CardData>();
                card.hp = cardHp;
                card.damage = cardDamage;
                card.cRIP = cardCRIP;
                card.heal = cardHeal;
                card.index = cardIndex;
                card.type = cardeType;
                card.image = cardImage;

                initEmptyObjs[i].gameObject.SetActive(true);

                switch(cardeType)
                {
                    case JobManager.Jobs.defender:
                        initEmptyObjs[i].gameObject.name = defenderObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.darkGreen;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.green;
                        break;
                    case JobManager.Jobs.knight:
                        initEmptyObjs[i].gameObject.name = attackerObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.darkRed;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.red;
                        break;
                    case JobManager.Jobs.wizard:
                        initEmptyObjs[i].gameObject.name = wizardObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.darkRed;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.red;
                        break;
                    case JobManager.Jobs.healler:
                        initEmptyObjs[i].gameObject.name = healderObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.lightGreen;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.green;
                        break;
                    case JobManager.Jobs.buffer:
                        initEmptyObjs[i].gameObject.name = bufferObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.navyBlue;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.skyBlue;
                        break;
                    case JobManager.Jobs.joker:
                        initEmptyObjs[i].gameObject.name = jokerObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.darkCyan;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.darkCyan;
                        break;
                    case JobManager.Jobs.unemployed:
                        initEmptyObjs[i].gameObject.name = unemployedObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.darkOrchid;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.gray;
                        break;
                    case JobManager.Jobs.convict:
                        initEmptyObjs[i].gameObject.name = convictObjPrefabs[index].gameObject.name;
                        textpro[0].color = Color.darkSlateGray;
                        textpro[1].text = CheckedNoZeoValue().ToString();
                        textpro[1].color = Color.gray4;
                        break;
                }
                break;
            }
        }
    }

    float CheckedNoZeoValue()
    {
        if(cardHp != 0)
            return cardHp;

        if(cardDamage != 0)
            return cardDamage;

        if(cardCRIP != 0)
            return cardCRIP;

        if(cardHeal != 0)
            return cardHeal;

        return 0;   
    }
    
    int RandomInitCardNum(int max)
    {
        return UnityEngine.Random.Range(0, max);
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
                break;
        }
    }
}
