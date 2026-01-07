using UnityEngine;

public interface ISkillCooldownReadable
{
    float CooldownDuration { get; }
    float CooldownRemaining { get; }
    bool IsReady { get; }
    Sprite Icon { get; }
}
