using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class StateCulManager : MonoBehaviour
{
    public int initalCardN = 4;
    public int MAXCARDNUM = 200;
    public Transform userT;
    public GameObject minJCardsPerfab;
    [Header("stat")]
    public float userHP = 100;
    public float userDamge = 10;
    public float userCRIP = 1; // 치명타 퍼센트 100%
    [Header("Card")]
    public float cardHp;
    public float cardDamage;
    public float cardCRIP;
    public float cardHeal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartInit();
    }

    void StartInit()
    {
        GameObject[] objs = new GameObject[MAXCARDNUM];
        int ij = 0;
        // pool sys
        for(int i = 0; i < MAXCARDNUM; i++)
        {
            GameObject obj = Instantiate(minJCardsPerfab, userT.position, Quaternion.identity);
            obj.transform.SetParent(userT);
            obj.SetActive(false);

            objs[i] = obj;
        }

        // 초기 카드 활성화
        for(int i = 0; i < MAXCARDNUM; i++)
        {
            if(objs[i].activeInHierarchy == false)
            {
                objs[i].SetActive(true);
                ij++;
            }

            if(ij == initalCardN)
                break;
        }
    }
}
