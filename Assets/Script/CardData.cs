using UnityEngine;

public class CardData : MonoBehaviour
{
    public StateCulManager stateCulManager;
    public float cardHp;
    public float cardDamage;
    public float cardCRIP;
    public float cardHeal;
    
    public JobManager.Jobs type;
    public int index;

    public void ReStateSend()
    {
        stateCulManager.ReciveValueDataCard(cardHp, cardDamage, cardCRIP, cardHeal, index, type);
    }

    void OnEnable()
    {
        
    }

}
