using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardData : MonoBehaviour
{
    StateCulManager stateCulManager;
    public string cname;
    public float hp;
    public float damage;
    public float cRIP;
    public float heal;
    
    public JobManager.Jobs type;
    public int index;
    public Sprite image;

    public string cardExplain;

    private bool isDarg;

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
            OnPointerExit();

        if(isDarg)
        {
            transform.position = Input.mousePosition;
        }
    }
    public void OnPointerClick()
    {
        stateCulManager.ShowTheDetailCard(this.gameObject.GetComponent<CardData>(), true);
    }

    void OnPointerExit()
    {   
        stateCulManager.ShowTheDetailCard(this.gameObject.GetComponent<CardData>(), false);
    }

    void OnEnable()
    {
        stateCulManager = FindAnyObjectByType<StateCulManager>();
    }

    public void PosSameThePoint (bool isIn)
    {
        isDarg = isIn;
    }
}
