using UnityEngine;

public class ClimbAttributes : MonoBehaviour
{
    [SerializeField] private float m_climbingDifficulty = 1.0f;

    public float ClimbingDifficulty => m_climbingDifficulty;
}
