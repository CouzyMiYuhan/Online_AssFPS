using Photon.Pun;
using UnityEngine;
using System.Collections;

public class PlayerKnockbackReceiver : MonoBehaviourPun
{
    [Header("CharacterController（不填也行，会自动找）")]
    public CharacterController characterController;

    [Header("Knockback Damping")]
    public float damping = 6f; // 推力衰减速度

    private Coroutine _knockCo;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (characterController == null)
            characterController = GetComponentInChildren<CharacterController>();
    }

    [PunRPC]
    public void RPC_Knockback(Vector3 velocity, float duration)
    {
        if (!photonView.IsMine) return;

        if (_knockCo != null) StopCoroutine(_knockCo);
        _knockCo = StartCoroutine(CoKnock(velocity, duration));
    }

    private IEnumerator CoKnock(Vector3 v, float duration)
    {
        if (characterController == null)
        {
            Debug.LogWarning("[PlayerKnockbackReceiver] No CharacterController found.");
            yield break;
        }

        float t = 0f;
        Vector3 cur = v;

        while (t < duration)
        {
            t += Time.deltaTime;

            // 只移动，不改你自己的移动逻辑（但会有“推开”效果）
            characterController.Move(cur * Time.deltaTime);

            // 衰减，避免一直飘
            cur = Vector3.Lerp(cur, Vector3.zero, Time.deltaTime * damping);

            yield return null;
        }

        _knockCo = null;
    }
}
