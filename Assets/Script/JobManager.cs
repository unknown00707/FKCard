using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JobManager : MonoBehaviour
{
    [Header("Explain")]
    public Text jobName;
    public Text jobExplain;
    
    public enum Jobs
    {
        defender, // 탱커
        knight, // 근접 딜러
        magictor, // 광역 딜러
        buffer, // 버퍼
        healler, // 힐러 
        joker, // 전략가
        unemployed, // 백수, 빈털털이  : 카드가 6장
        convict // 죄수 : 트롤  - 성능 카드 2장 
    }

    Dictionary<string , string> jobPassive = new Dictionary<string, string>()
    {
        { "defender", "단일공격을 모두 자신이 받음 / 모든 피해를 일부분 차감시켜 받음 / 체력 높음"},
        { "knight", "치명타 확률 증가 / 일정확률로 피해없음 / 공격력 높음"},
        { "wizard", " 카드를 2장씩 소모가능 / 공격력 높음"},
        { "healler", "아군 전부 사망시 게임당 한번 전부 부활 / 힐 카드 효율 좋음"},
        { "buffer", "턴이 흐를 때마다 스탯 증가 / 카드를 모두 소모하면 아군도 스탯증가 / 체력 높음"},
        { "joker", "카드획득,강화 5회마다 2배로 적용 / 모든 아군이 회마다 리롤 1번씩"},
        { "unemployed", "카드획득 배율이 1.5배 (4개=6개)로 적용 / 전투중에 아군에게 카드양도 가능인데 카드 0개되면 바로 사망"},
        { "convict", "카드획득 배율이 0.5배 (4개=2개)로 적용 / 자기가 뽑은 카드의 수치가 1.5배 / 공격력 높음"},
    };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeName(String index)
    {
        jobName.text = index;
        jobExplain.text = jobPassive[index];
    }
}
