using UnityEngine;

public class BurningStatus : MonoBehaviour
{
    public float tickDamage = 5f;       // 每跳伤害
    public float duration = 5f;         // 总持续时间
    public float tickInterval = 1f;     // 每多少秒跳一次

    private CharacterHealth _health;
    private float _elapsed = 0f;
    private float _tickTimer = 0f;

    void Awake()
    {
        _health = GetComponent<CharacterHealth>();
    }

    public void Refresh()
    {
        _elapsed = 0f;
        _tickTimer = 0f;
    }

    void OnEnable()
    {
        Refresh();
    }

    void Update()
    {
        if (_health == null)
        {
            Destroy(this);
            return;
        }

        _elapsed += Time.deltaTime;
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= tickInterval)
        {
            _tickTimer -= tickInterval;
            _health.TakeDamage(tickDamage);
            Debug.Log($"[Burning] {name} takes {tickDamage} burn damage");
        }

        if (_elapsed >= duration)
        {
            Destroy(this);
        }
    }
}
