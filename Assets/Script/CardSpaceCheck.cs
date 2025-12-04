using UnityEngine;
using Unity.Netcode;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;
using TMPro;

public class CardSpaceCheck : MonoBehaviour
{
    public StateCulManager stateCulManager;
    public Transform[] spaces;
    public GameObject[] cardPrefabs;
    public GameObject canvers;
    public bool isOkToGoInFC = true; // 들어갈 공간이 없다
    CardPlayer player;
    private string initName;
    void Awake()
    {
    
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
        
        if(cardPrefabs[NetworkManager.Singleton.LocalClientId].name == initName)
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
            stateCulManager.CardSearchMatch();
            
            player.ReciveSignCardEffectReady(isOkToGoInFC); // -> false
        }
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
