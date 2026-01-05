using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private Transform topPoint; // 梯子顶部位置
    [SerializeField] private Transform bottomPoint; // 梯子底部位置
    public Transform startPos;
    public Transform endPos;

    private void Start()
    {
        // 确保梯子有触发器
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("Ladder object needs a Collider component");
        }

        // 确保有Tag
        if (gameObject.CompareTag("Untagged"))
        {
            gameObject.tag = "Ladder";
        }
    }

    public Vector3 GetTopPosition()
    {
        return topPoint.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (bottomPoint != null)
        {
            Gizmos.DrawSphere(bottomPoint.position, 0.1f);
        }
        if (topPoint != null)
        {
            Gizmos.DrawSphere(topPoint.position, 0.1f);
        }
        Gizmos.color = Color.green;
        if (startPos != null)
        {
            Gizmos.DrawSphere(startPos.position, 0.1f);
        }
        Gizmos.color = Color.red;
        if (endPos != null)
        {
            Gizmos.DrawSphere(endPos.position, 0.1f);
        }
    }
}