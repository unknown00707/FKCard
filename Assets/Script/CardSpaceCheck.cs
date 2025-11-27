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
    CardPlayer player;
    void Awake()
    {
        if(NetworkManager.Singleton != null)
            player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CardPlayer>();

        CardSpacePrefabsInit();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Card"))
        {
            collision.gameObject.transform.SetParent(spaces[NetworkManager.Singleton.LocalClientId]);
            player.ReciveSignCardBatchOnStage(collision.transform);
        }
    }

    public void CardSpacePrefabsInit()
    {
        foreach(GameObject obj in cardPrefabs)
        {
            obj.SetActive(false);
        }
    }

    public void MakeCardPublicSame(Transform tr , ulong spaceId)
    {
        CardData card = tr.GetComponent<CardData>();
        CardData changingCard = cardPrefabs[(int)spaceId].GetComponent<CardData>();
        changingCard.cname = card.cname;
        changingCard.hp = card.hp;
        changingCard.damage = card.damage;
        changingCard.cRIP = card.cRIP;
        changingCard.heal = card.heal;
        changingCard.image = card.image;
        changingCard.cardExplain = card.cardExplain;
        changingCard.index = card.index;
        changingCard.type = card.type;
        cardPrefabs[(int)spaceId].GetComponent<Button>().image.sprite = card.image;
        
        // 이름 / 수치 설정
        TextMeshProUGUI[] textpro = cardPrefabs[(int)spaceId].GetComponentsInChildren<TextMeshProUGUI>();
        textpro[0].text = changingCard.cname;  
        textpro[1].text = changingCard.hp.ToString();  
        textpro[2].text = changingCard.damage.ToString();  
        textpro[0].color = Color.white;
        textpro[1].color = Color.white;

        switch(changingCard.type)
        {
            case JobManager.Jobs.defender:
                cardPrefabs[(int)spaceId].name = stateCulManager.defenderObjPrefabs[changingCard.index].gameObject.name;
                break;
            case JobManager.Jobs.knight:
                cardPrefabs[(int)spaceId].name = stateCulManager.knightObjPrefabs[changingCard.index].gameObject.name;
                break;
            case JobManager.Jobs.wizard:
                cardPrefabs[(int)spaceId].name = stateCulManager.wizardObjPrefabs[changingCard.index].gameObject.name;
                break;
            case JobManager.Jobs.healler:
                cardPrefabs[(int)spaceId].name = stateCulManager.healderObjPrefabs[changingCard.index].gameObject.name;
                break;
            case JobManager.Jobs.buffer:
                cardPrefabs[(int)spaceId].name = stateCulManager.bufferObjPrefabs[changingCard.index].gameObject.name;
                break;
            case JobManager.Jobs.joker:
                cardPrefabs[(int)spaceId].name = stateCulManager.jokerObjPrefabs[changingCard.index].gameObject.name;
                break;
            case JobManager.Jobs.convict:
                cardPrefabs[(int)spaceId].name = stateCulManager.convictObjPrefabs[changingCard.index].gameObject.name;
                break;
        }
        
        cardPrefabs[(int)spaceId].SetActive(true);
    }
}
