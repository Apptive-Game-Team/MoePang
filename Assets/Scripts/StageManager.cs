using UnityEngine;

public class StageManager : SingletonObject<StageManager>
{
    [Header("스테이지 설정")]
    [SerializeField] private int currentStage;

    public int CurrentStage => currentStage;

    public void SetStage(int num)
    {
        currentStage += num;
    }
}
