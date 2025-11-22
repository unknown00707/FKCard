using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class StateCulManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    [Header("Basic Set")]
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
    public int cardIndex;
    public JobManager.Jobs cardeType;

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

    // 카드 데이터에서 정보 가지고 오기
    public void ReciveValueDataCard(float hp, float damage, float cRIP, float heal, int index, JobManager.Jobs job)
    {
        cardHp = hp;
        cardDamage = damage;
        cardCRIP = cRIP;
        cardHeal = heal;
        cardIndex = index;
        cardeType = job;
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
