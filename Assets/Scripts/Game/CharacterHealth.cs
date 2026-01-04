using System;
using UnityEngine;
using Photon.Pun;   // ★ 新增

public class CharacterHealth : MonoBehaviourPun, IDamageable
{
    [Header("生命值")]
    public float maxHealth = 400f;
    public float currentHealth;

    // 让 UI 订阅用的事件（你 PlayerHealthUI 已经在用）
    public event Action<float, float> onHealthChanged;

    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        // 一开始满血
        currentHealth = maxHealth;

        // 通知一次 UI 初始化
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ========= RPC 入口 =========

    /// <summary>网络扣血入口：所有客户端都会调用本地 TakeDamage</summary>
    [PunRPC]
    public void RPC_TakeDamage(float amount)
    {
        TakeDamage(amount);
    }

    /// <summary>网络回血入口：所有客户端都会调用本地 Heal</summary>
    [PunRPC]
    public void RPC_Heal(float amount)
    {
        Heal(amount);
    }

    // ========= 本地逻辑（单机 / RPC 内部都走这里） =========

    // 受伤
    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead) return;

        currentHealth -= amount;
        if (currentHealth < 0f)
            currentHealth = 0f;

        // 通知 UI 更新
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsDead)
        {
            Die();
        }
    }

    // 血包用的回复函数
    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // 同样要通知 UI
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        // 这里写你原来死亡的逻辑（播放动画、禁用控制等等）
        Debug.Log("[CharacterHealth] 死亡");
    }
}
