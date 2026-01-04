using System;
using UnityEngine;

[Serializable]
public class ElementDefinition
{
    public ElementType type;

    [Header("UI")]
    public Sprite icon;

    [Header("Prefabs")]
    public GameObject displayPrefab;       // 手上展示
    public GameObject projectilePrefab;    // 普通子弹 prefab
    public GameObject altProjectilePrefab; // 特殊子弹 prefab（比如水龙卷，其他元素留空）

    [Header("Stats")]
    public float damage = 20f;             // 基础伤害
    public float fireCooldown = 0.5f;      // 开火 CD
    public float range = 30f;              // 射程（用于计算直线子弹 lifeTime 等）
    public float projectileSpeed = 20f;    // 飞行速度

    [Header("Area Projectile (土 / 水龙卷等)")]
    public float moveDuration = 3f;        // 飞行时间
    public float lingerDuration = 3f;      // 停留时间
    public float areaRadius = 3f;          // AoE 半径

    [Header("Fire Special")]
    public bool useAltEveryNShots = false;
    public int altShotInterval = 5;        // 每多少发触发一次 altProjectile（比如 5）
}
