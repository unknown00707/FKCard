using UnityEngine;
using UnityEngine.UI;

public class CardData : MonoBehaviour
{
   
    public string cname;
    public float hp;
    public float damage;
    public float cRIP;
    public float heal;
    
    public JobManager.Jobs type;
    public int index;
    public Sprite image;


    void OnEnable()
    {
        
    }

}
