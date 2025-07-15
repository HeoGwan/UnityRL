using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// 플레이어의 움직임을 담당하는 컨트롤러
/// </summary>
public class PlayerMoveController : Agent
{
    // 움직이는 속도 조정
    public float moveSpeed = 3f;
    
    // 점프 파워 설정
    public float jumpPower = 2f;
    
    // 시작 위치 저장
    public Vector3 startPosition;
    
    // 움직임을 위한 Rigidbody2D 컴포넌트
    public Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // 좌우 입력
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            rigid.AddForce(Vector2.left * moveSpeed);
            // transform.position += Vector3.left * (moveSpeed * Time.deltaTime);
        }
        
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            rigid.AddForce(Vector2.right * moveSpeed);
            // transform.position += Vector3.right * (moveSpeed * Time.deltaTime);
        }
        
        // 점프
        // if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space))
        // {
        //     rigid.AddForce(Vector2.up * jumpPower);
        // }
    }
    
    /// <summary>
    /// 강화학습 상태
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // 플레이어 x 위치
        sensor.AddObservation(transform.position.x); // (1)

        // 플레이어 속도
        sensor.AddObservation(rigid.linearVelocityX); // (2)

        // 현재 벽 정보
        if (WallManager.Instance.currentWall != null)
        {
            Wall currentWall = WallManager.Instance.currentWall;
            Vector3 wallPos = currentWall.transform.position;

            // 벽 y 위치 (gap 접근 여부 판단에 필요)
            sensor.AddObservation(wallPos.y); // (3)

            // 안전지대의 중심 x 위치
            // float gapX = currentWall.GetHoleX(); // (4)
            Vector3 worldPos = currentWall.transform.TransformPoint(currentWall.safeAreaPosition);
            // float safeAreaX = currentWall.safeAreaPosition.x;
            float safeAreaWorldX = worldPos.x;
            sensor.AddObservation(safeAreaWorldX);

            // gap과의 x 거리 (절대값)
            float distanceToGap = Mathf.Abs(transform.position.x - safeAreaWorldX); // (5)
            sensor.AddObservation(distanceToGap);
        }
        else
        {
            // 벽이 없을 경우 기본값 제공
            sensor.AddObservation(10f); // wallPos.y
            sensor.AddObservation(0f);  // gapX
            sensor.AddObservation(0f);  // distanceToGap
        }
    }
    
    /// <summary>
    /// 강화학습 행동
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // // Continuous
        // float move = actions.ContinuousActions[0]; // -1 ~ 1 범위로 가정
        // Debug.Log(move);
        // Debug.Log(move * moveSpeed);
        //
        // rigid.AddForce(Vector2.right * move * moveSpeed);
        
        // // Discrete
        // int move = actions.DiscreteActions[0]; // 좌, 우, 멈춤 3개만 있기 때문에 Discreate로 진행
        //
        // // 0: 왼쪽
        // // 1: 가만히
        // // 2: 오른쪽
        // int dir = move switch
        // {
        //     0 => -1,
        //     1 => 0,
        //     2 => 1,
        //     _ => throw new ArgumentOutOfRangeException()
        // };
        //
        // rigid.AddForce(Vector2.right * dir * moveSpeed);
        // // transform.position += Vector3.right * dir * moveSpeed * Time.deltaTime;

        // Action Space: 3
        int action = actions.DiscreteActions[0];
        int dir = action switch
        {
            0 => -1,
            2 => 1,
            _ => 0
        };
        rigid.AddForce(Vector2.right * dir * moveSpeed);
        
        // 현재 벽의 gap 위치와 거리 계산
        // Wall currentWall = WallManager.Instance.currentWall;
        // if (currentWall != null)
        // {
        //     // float gapX = currentWall.GetHoleX();
        //     float safeArea = currentWall.safeAreaPosition.x;
        //     float distance = Mathf.Abs(transform.position.x - safeArea);
        //     AddReward(-distance * 0.001f);
        // }
        
        // 보상: 살아있으면 작게 +, 죽으면 -1
        AddReward(0.01f);
        // 추가 보상 계산
        CalculateReward();
        // AddReward(-0.001f);
    }
    
    /// <summary>
    /// 수동 조작
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // // Continuous
        // var continuousActions = actionsOut.ContinuousActions;
        // float input = 0;
        //
        // if (Input.GetKey(KeyCode.A)) input = -1;
        // else if (Input.GetKey(KeyCode.D)) input = 1;
        //
        // continuousActions[0] = input;
        
        // Discrete
        var descreteActions = actionsOut.DiscreteActions;
        int input = 1;
        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input = 0;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input = 2;
        
        descreteActions[0] = input;
    }
    
    /// <summary>
    /// 에피소드 시작 시
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // 플레이어 위치 초기화
        rigid.linearVelocity = Vector2.zero;
        transform.position = startPosition;

        // 벽 재설정
        foreach (var wall in WallManager.Instance.walls)
        {
            wall.gameObject.SetActive(false);
            wall.transform.position = new Vector3(0, wall.y, 0);
        }

        WallManager.Instance.StartGame();
    }
    
    /// <summary>
    /// 보상 계산
    /// </summary>
    private void CalculateReward()
    {
        if (WallManager.Instance.currentWall == null) return;

        // 현재 내려오는 벽 참조
        var currentWall = WallManager.Instance.currentWall;

        // 1. 플레이어와 gap 중심 사이 거리
        //float gapX = currentWall.GetHoleX();
        float distanceToGap = Mathf.Abs(transform.position.x - currentWall.safeAreaPosition.x);
        
        // 2. 벽이 어느 정도 내려왔을 때만 보상 계산
        float wallY = currentWall.transform.position.y;

        // 벽이 어느 정도 내려왔을 때만 계산 (예: 절반 이하로 내려오면 판단 시작)
        if (wallY < 4f)
        {
            // 3. 거리에 따라 감점 (거리가 멀수록 큰 페널티, 가까울수록 0에 수렴)
            float reward = -distanceToGap * 0.05f;

            AddReward(reward);

            // 4. gap에 가까우면 보너스
            if (distanceToGap < 0.5f)
            {
                AddReward(+0.2f);  // gap 잘 맞춘 보너스
            }
        }
    }
}
