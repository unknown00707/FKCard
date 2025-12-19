using UnityEngine;
using Unity.Netcode;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class CardSpaceCheck : MonoBehaviour
{
    private static WaitForSecondsRealtime _waitForSecondsRealtime10 = new(10f);
    public StateCulManager stateCulManager;
    public Transform[] spaces;
    public GameObject[] cardPrefabs;
    public GameObject canvers;
    public bool isOkToGoInFC = true; // 들어갈 공간이 없다
    public Coroutine runningCoroutine;
    CardPlayer player;
    private string initName;
    void Awake()
    {
        isOkToGoInFC = true;
    }

    void Update()
    {
        if(NetworkManager.Singleton == null) return;
        
        if (player == null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
            initName = cardPrefabs[NetworkManager.Singleton.LocalClientId].name;
            CardSpacePrefabsInit(true);
        }
        
        if((cardPrefabs[NetworkManager.Singleton.LocalClientId].name == initName) && isOkToGoInFC)
            isOkToGoInFC = true;
        else
            isOkToGoInFC = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isOkToGoInFC) return; 

        if(collision.gameObject.CompareTag("Card"))
        {
            //collision.gameObject.transform.SetParent(spaces[NetworkManager.Singleton.LocalClientId]);// 들어온 놈
            CardData cardData =  collision.gameObject.GetComponent<CardData>();
            player.ReciveSignCardBatchOnStage(cardData.type, cardData.index);

            collision.gameObject.SetActive(false);
            stateCulManager.playerEmptyObjs.Remove(collision.GetComponent<Button>());
            collision.transform.SetParent(stateCulManager.transform);

            isOkToGoInFC = false;
            // 카드 효과 저장 함수 발동
            print("카드 발동!");
            stateCulManager.CardSearchMatch();
            
            print("턴 넘길 준비!");
            StopCoroutine(runningCoroutine);
        }
    }
    public void StartACoroutine()
    {
        runningCoroutine = StartCoroutine(WaitTenForCardOn());
    }
    private IEnumerator WaitTenForCardOn()
    {
        print("카드 내기 대기..!");
        yield return _waitForSecondsRealtime10;
        isOkToGoInFC = false; // -> 카드 내기 실패!
        print("카드 강제로 냄!");
        player.ReciveSignCardEffectReady(isOkToGoInFC);
    }
    public void CardSpacePrefabsInit(bool isFrist)
    {
        foreach(GameObject obj in cardPrefabs)
        {
            if(isFrist)
                obj.name = initName;

            obj.SetActive(false);
        }
    }
    public void WaitToPrivateAblieTrunFC(int correspondingTurn)
    {
        print("턴 이용 불가 코루틴 실행 ! - !");
        StartCoroutine(WaitToPrivateAblieTrun(correspondingTurn));
    }
    private IEnumerator WaitToPrivateAblieTrun(int correspondingTurn)
    {  
        print("턴 진행 불가!!");
        isOkToGoInFC = false;
        yield return new WaitUntil(() => GameManager.Instance.totalTrunNum.Value == correspondingTurn);
        print("턴 진행 가능!! - - - 턴을 정상화 중 . . .!");
        isOkToGoInFC = true;
    }
    public void MakeCardPublicSame(JobManager.Jobs job, int index ,ulong spaceId)
    {
        stateCulManager.CardSameMakeObjPublic(job, index, cardPrefabs[(int)spaceId]);
        cardPrefabs[(int)spaceId].SetActive(true);
    }
}
