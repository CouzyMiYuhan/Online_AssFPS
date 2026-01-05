using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerFreezeReceiver : MonoBehaviourPun
{
    [Header("Freeze Target")]
    public PlayerController controller;

    [Header("Animator (可不填，会自动找)")]
    public Animator animator;
    public string frozenBoolName = "IsFrozen";

    [Header("Frozen Color")]
    public bool enableBlueTint = true;
    public Color frozenTint = new Color(0.35f, 0.75f, 1f, 1f);
    public Renderer[] targetRenderers; // 不填会自动抓

    [Header("Optional: 冻结时也一起禁用的脚本（只在本机禁用）")]
    public Behaviour[] extraDisable;

    private Coroutine co;
    private float frozenUntil = 0f;
    private bool frozenBroadcasted = false;

    // 缓存每个 Renderer 的原始色（按材质槽）
    private struct ColorSlot
    {
        public Renderer r;
        public int index;
        public int colorPropId;
        public Color original;
    }
    private readonly List<ColorSlot> _slots = new List<ColorSlot>();
    private MaterialPropertyBlock _mpb;
    private static readonly int ID_BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int ID_Color = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (controller == null) controller = GetComponentInParent<PlayerController>();

        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (enableBlueTint)
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
                targetRenderers = GetComponentsInChildren<Renderer>(true);

            BuildColorCache();
        }
    }

    private void BuildColorCache()
    {
        _slots.Clear();
        _mpb ??= new MaterialPropertyBlock();

        foreach (var r in targetRenderers)
        {
            if (r == null) continue;

            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;

                if (m.HasProperty(ID_BaseColor))
                    _slots.Add(new ColorSlot { r = r, index = i, colorPropId = ID_BaseColor, original = m.GetColor(ID_BaseColor) });
                else if (m.HasProperty(ID_Color))
                    _slots.Add(new ColorSlot { r = r, index = i, colorPropId = ID_Color, original = m.GetColor(ID_Color) });
            }
        }
    }

    [PunRPC]
    public void RPC_Freeze(float duration)
    {
        // 只在被影响者自己的本机执行“冻结逻辑”(禁用移动/延长时间)
        if (!photonView.IsMine) return;

        frozenUntil = Mathf.Max(frozenUntil, Time.time + duration);

        if (co == null) co = StartCoroutine(CoFreeze());
    }

    private IEnumerator CoFreeze()
    {
        if (controller == null)
        {
            Debug.LogWarning("[PlayerFreezeReceiver] PlayerController not found.");
            co = null;
            yield break;
        }

        // === Freeze Start ===
        bool prevEnabled = controller.enabled;
        controller.enabled = false;

        if (extraDisable != null)
        {
            foreach (var b in extraDisable)
                if (b != null) b.enabled = false;
        }

        // ✅ 关键：动画同步给所有人（不再依赖 PhotonAnimatorView）
        if (!frozenBroadcasted)
        {
            frozenBroadcasted = true;
            photonView.RPC(nameof(RPC_SetFrozenAnim), RpcTarget.All, true);
            if (enableBlueTint) photonView.RPC(nameof(RPC_SetFrozenTint), RpcTarget.All, true);
        }

        // 持续到时间结束（期间再次被命中会延长 frozenUntil）
        while (Time.time < frozenUntil)
            yield return null;

        // === Freeze End ===
        photonView.RPC(nameof(RPC_SetFrozenAnim), RpcTarget.All, false);
        if (enableBlueTint) photonView.RPC(nameof(RPC_SetFrozenTint), RpcTarget.All, false);

        frozenBroadcasted = false;

        controller.enabled = prevEnabled;

        if (extraDisable != null)
        {
            foreach (var b in extraDisable)
                if (b != null) b.enabled = true;
        }

        frozenUntil = 0f;
        co = null;
    }

    [PunRPC]
    private void RPC_SetFrozenAnim(bool on)
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        animator.SetBool(frozenBoolName, on);
    }

    [PunRPC]
    private void RPC_SetFrozenTint(bool on)
    {
        if (!enableBlueTint) return;
        if (_slots.Count == 0) BuildColorCache();

        _mpb ??= new MaterialPropertyBlock();

        foreach (var s in _slots)
        {
            if (s.r == null) continue;

            s.r.GetPropertyBlock(_mpb, s.index);
            _mpb.SetColor(s.colorPropId, on ? frozenTint : s.original);
            s.r.SetPropertyBlock(_mpb, s.index);
        }
    }
}
