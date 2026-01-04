using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerElementHandler : MonoBehaviourPun
{
    [Header("References")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("Pickup")]
    public float pickupRange = 5f;
    public LayerMask pickupLayer;

    [Header("Element Configs")]
    public ElementDefinition[] elementDefinitions;

    [Header("UI")]
    public Image currentElementIcon;

    [Header("Animation & Audio")]
    public Animator animator;
    public string attackTriggerName = "Attack";
    public AudioSource sfxSource;
    public AudioClip pickupClip;
    public AudioClip fireClip;

    private ElementDefinition currentElement;
    private ElementType currentType = ElementType.None;
    private GameObject heldInstance;
    private float nextFireTime = 0f;
    private int fireShotCount = 0;

    void Start()
    {
        if (PhotonNetwork.IsConnected && photonView != null && !photonView.IsMine)
            return;

        // [MOD-1] 自动兜底绑定相机，避免 playerCamera 没拖导致 Raycast 发不出去
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
            if (playerCamera == null) playerCamera = Camera.main;

            Debug.Log($"[PlayerElementHandler] Auto bind playerCamera = {(playerCamera != null ? playerCamera.name : "NULL")}");
        }

        if (currentElementIcon == null)
        {
            GameObject uiObj = GameObject.Find("element");
            if (uiObj != null)
            {
                currentElementIcon = uiObj.GetComponent<Image>();
                Debug.Log("[ElementUI] 通过 GameObject.Find(\"element\") 自动绑定元素图标");
            }
            else
            {
                Debug.LogWarning("[ElementUI] 找不到名为 'element' 的 UI Image，无法显示元素图标");
            }
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
            return;

        HandlePickup();
        HandleEquip();
        HandleFire();
    }

    ElementDefinition GetDefinition(ElementType type)
    {
        foreach (var def in elementDefinitions)
        {
            if (def != null && def.type == type)
                return def;
        }
        Debug.LogWarning($"[PlayerElementHandler] 没找到元素配置：{type}");
        return null;
    }

    void HandlePickup()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        // [MOD-2] 再次兜底（防止运行时相机被禁用/替换）
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
            if (playerCamera == null) playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning("[Pickup] playerCamera 仍然为 null，无法 Raycast");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // [MOD-3] Debug 可视化射线（Scene 里看得到）
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.yellow, 0.6f);

        // [MOD-4] LayerMask=0 时给一个兜底（很多人忘了在 Inspector 选层）
        int mask = pickupLayer.value == 0 ? ~0 : pickupLayer.value;

        // [MOD-5] 强制打到 Trigger（否则 trigger collider 有时会被忽略）
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, mask, QueryTriggerInteraction.Collide))
        {
            // [MOD-6] 先查命中物体，再查父物体（ElementPickup 很多时候挂在父节点）
            ElementPickup pickup = null;

            if (!hit.collider.TryGetComponent<ElementPickup>(out pickup))
                pickup = hit.collider.GetComponentInParent<ElementPickup>();

            if (pickup != null)
            {
                Debug.Log($"[Pickup] Hit: {hit.collider.name}, pickupRoot: {pickup.gameObject.name}, type={pickup.elementType}");

                SetCurrentElement(pickup.elementType);

                // Destroy 整个 pickup 的根（而不是只删 collider 子物体）
                Destroy(pickup.gameObject);

                if (sfxSource != null && pickupClip != null)
                    sfxSource.PlayOneShot(pickupClip);
            }
            else
            {
                Debug.Log($"[Pickup] Hit {hit.collider.name} but NO ElementPickup found on self/parent.");
            }
        }
        else
        {
            Debug.Log("[Pickup] Raycast hit nothing (check range/layer/collider).");
        }
    }

    void SetCurrentElement(ElementType type)
    {
        currentElement = GetDefinition(type);
        currentType = type;
        fireShotCount = 0;

        if (currentElement == null)
        {
            Debug.LogWarning($"[PlayerElementHandler] SetCurrentElement 失败：没找到类型为 {type} 的配置");
            return;
        }

        string displayName = currentElement.displayPrefab != null ? currentElement.displayPrefab.name : "NULL";
        Debug.Log($"[PlayerElementHandler] 设置当前元素 -> type = {currentElement.type}, displayPrefab = {displayName}");

        if (currentElementIcon != null)
        {
            currentElementIcon.sprite = currentElement.icon;
            currentElementIcon.enabled = currentElement.icon != null;
        }
    }

    void HandleEquip()
    {
        if (!Input.GetKeyDown(KeyCode.Alpha1)) return;

        string typeInfo = currentElement != null ? currentElement.type.ToString() : "NULL";
        string displayInfo = (currentElement != null && currentElement.displayPrefab != null)
            ? currentElement.displayPrefab.name
            : "NULL";

        Debug.Log($"[Equip] 按下 1 键，currentElement = {typeInfo}, displayPrefab = {displayInfo}");

        if (currentElement == null || currentElement.displayPrefab == null)
        {
            Debug.LogWarning("[Equip] 当前元素没有 displayPrefab 或尚未拾取元素");
            return;
        }

        if (heldInstance != null)
            Destroy(heldInstance);

        heldInstance = Instantiate(currentElement.displayPrefab);

        HoldPose pose = heldInstance.GetComponent<HoldPose>();
        if (pose != null)
        {
            pose.ApplyPose(holdPoint);
        }
        else
        {
            heldInstance.transform.SetParent(holdPoint);
            heldInstance.transform.localPosition = Vector3.zero;
            heldInstance.transform.localRotation = Quaternion.identity;
            heldInstance.transform.localScale = Vector3.one;
        }

        var pickup = heldInstance.GetComponent<ElementPickup>();
        if (pickup != null) Destroy(pickup);

        var rb = heldInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("[Equip] 已装备元素：" + currentElement.type);
    }

    void HandleFire()
    {
        if (!Input.GetButtonDown("Fire1")) return;

        Debug.Log($"[Fire] 点击鼠标，currentType = {currentType}");

        if (currentElement == null)
        {
            Debug.LogWarning("[Fire] currentElement 为 null，说明还没拾取任何元素");
            return;
        }

        if (heldInstance == null)
        {
            Debug.LogWarning("[Fire] heldInstance 为 null，说明还没按 1 装备到手上");
            return;
        }

        if (Time.time < nextFireTime)
        {
            Debug.Log($"[Fire] 冷却中，剩余时间 {nextFireTime - Time.time:F2}");
            return;
        }
        nextFireTime = Time.time + currentElement.fireCooldown;

        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        if (sfxSource != null && fireClip != null)
            sfxSource.PlayOneShot(fireClip);

        GameObject prefabToUse = currentElement.projectilePrefab;
        bool useAlt = false;

        if (currentType == ElementType.Fire &&
            currentElement.useAltEveryNShots &&
            currentElement.altProjectilePrefab != null)
        {
            fireShotCount++;
            useAlt = (fireShotCount % currentElement.altShotInterval == 0);
            if (useAlt) prefabToUse = currentElement.altProjectilePrefab;

            Debug.Log($"[Fire] 火元素射击，第 {fireShotCount} 发，useAlt={useAlt}, prefab={prefabToUse.name}");
        }
        else
        {
            Debug.Log($"[Fire] 普通射击，元素={currentType}, prefab={prefabToUse.name}");
        }

        if (prefabToUse == null)
        {
            Debug.LogWarning("[Fire] prefabToUse 为 null，检查 ElementDefinition 的 projectilePrefab/altProjectilePrefab 是否没填");
            return;
        }

        GameObject proj = Instantiate(prefabToUse, holdPoint.position, holdPoint.rotation);
        Debug.Log($"[Fire] 实例化子弹: {proj.name}");

        DirectHitProjectile direct = proj.GetComponent<DirectHitProjectile>();
        if (direct != null)
        {
            direct.damage = currentElement.damage;
            direct.speed = currentElement.projectileSpeed;
            direct.lifeTime = currentElement.range / Mathf.Max(currentElement.projectileSpeed, 0.01f);
            direct.elementType = currentType;
            direct.Init(holdPoint.forward, transform);
        }

        ElementProjectile area = proj.GetComponent<ElementProjectile>();
        if (area != null)
        {
            area.damageAmount = currentElement.damage;
            area.speed = currentElement.projectileSpeed;
            area.moveDuration = currentElement.moveDuration;
            area.lingerDuration = currentElement.lingerDuration;
            area.damageRadius = currentElement.areaRadius;
            area.Init(holdPoint.forward, transform);
        }
    }
}
