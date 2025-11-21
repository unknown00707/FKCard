using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class StateCulManager : MonoBehaviour
{
    public int initalCardN = 4;
    public int MAXCARDNUM = 200;
    public Transform userT;
    public GameObject minJCardsPerfab;
    [Header("sdfa")]
    public int userHP = 100;
    public int userDamge = 10;
    public int userCRIP = 1; // 치명타 퍼센트 100%

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartInit();
    }

    // Update is called once per frame
    void Update()
    {
        
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
