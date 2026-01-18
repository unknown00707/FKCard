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
    public TurnManager turnManager;
    public Transform[] spaces;
    public GameObject[] cardPrefabs;
    public GameObject canvers;
    public bool isOkToGoInFC = true; // 들어갈 공간이 없다
    public Coroutine runningCoroutine;
    CardPlayer player;
    int userID;
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
            userID = (int)NetworkManager.Singleton.LocalClientId;
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();
            initName = cardPrefabs[userID].name;
            
            CardSpacePrefabsInit(true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!turnManager.GiveAblePlayerBoolValue((int)NetworkManager.Singleton.LocalClientId)) return; 

        if(collision.gameObject.CompareTag("Card"))
        {
            //collision.gameObject.transform.SetParent(spaces[NetworkManager.Singleton.LocalClientId]);// 들어온 놈
            CardData cardData =  collision.gameObject.GetComponent<CardData>();
            player.ReciveSignCardBatchOnStage(cardData.type, cardData.index);

            collision.gameObject.SetActive(false);
            stateCulManager.playerEmptyObjs.Remove(collision.GetComponent<Button>());
            collision.transform.SetParent(stateCulManager.transform);

            player.RequsetUseCardSignal();
            // 카드 효과 저장 함수 발동
            print("턴 넘길 준비!");
            StopACoroutine();
            
            print("카드 발동!");
            stateCulManager.CardSearchMatch();
        }
    }
    public void StartACoroutine()
    {
        runningCoroutine = StartCoroutine(WaitTenForCardOn());
    }
    public void StopACoroutine()
    {
        StopCoroutine(runningCoroutine);
    }
    private IEnumerator WaitTenForCardOn()
    {
        print("카드 내기 대기..!");
        yield return _waitForSecondsRealtime10;
        if(turnManager.GiveAblePlayerBoolValue(userID))
            player.RequsetUseCardSignal();
        print("카드 강제로 냄!");
        player.ReciveSignCardEffectReady(turnManager.GiveAblePlayerBoolValue(userID));
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
    public void MakeCardPublicSame(JobManager.Jobs job, int index ,ulong spaceId)
    {
        stateCulManager.CardSameMakeObjPublic(job, index, cardPrefabs[(int)spaceId]);
        cardPrefabs[(int)spaceId].SetActive(true);
    }
}
