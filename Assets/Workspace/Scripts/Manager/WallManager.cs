using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class WallManager : MonoBehaviour
{
    private static WallManager instance;
    public static WallManager Instance => instance;
    
    public PlayerMoveController playerMoveController;
    
    // 벽 목록을 저장하는 리스트
    public List<Wall> walls;
    
    // 떨어지는 속도 조정
    // 이 변수로 모든 벽의 떨어지는 속도를 설정함
    public float fallSpeed;
    
    // 현재 활성화된 벽 저장
    public Wall currentWall;
    
    // 점수 저장
    public int score = 0;
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (instance is null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 스코어 초기화
        score = 0;
        scoreText.text = $"Score: {score.ToString()}";
    }

    /// <summary>
    /// 랜덤 벽 선택
    /// </summary>
    public void ChooseWall()
    {
        // 무작위 벽 선택
        int randomIndex = Random.Range(0, walls.Count);
        currentWall = walls[randomIndex];
        
        // 선택한 벽 활성화
        currentWall.gameObject.SetActive(true);
        
        // 선택한 벽 초기화
        currentWall.Initialize(fallSpeed, NextWall);
    }
    
    /// <summary>
    /// 다음 벽 선택
    /// </summary>
    public void NextWall()
    {
        // 현재 벽을 비활성화 시킴
        currentWall.gameObject.SetActive(false);
        currentWall = null;
        
        // 스코어 추가
        ++score;
        scoreText.text = $"Score: {score.ToString()}";
        
        // 다음 벽 선택
        ChooseWall();
        
        // Reward 추가
        playerMoveController.AddReward(1f);
    }

    /// <summary>
    /// 게임 초기화
    /// </summary>
    public void Initialize()
    {
        score = 0;
        scoreText.text = $"Score: {score.ToString()}";
    }

    public void StartGame()
    {
        // StartCoroutine(GameStart());
        ChooseWall();
    }
    
    private IEnumerator GameStart()
    {
        // 게임 시작하면 1초간 대기한 다음 게임을 시작함
        yield return new WaitForSeconds(1f);
        ChooseWall();
    }
}