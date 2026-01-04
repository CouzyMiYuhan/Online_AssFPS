using UnityEngine;

public static class GameResult
{
    /// <summary>本机是否获胜。EndScene 根据这个来显示 Victory / Defeat</summary>
    public static bool IsVictory = false;

    /// <summary>整局游戏的胜者 ActorNumber（仅调试 / 记录用）</summary>
    public static int WinnerActorNumber = -1;

    public static void Reset()
    {
        IsVictory = false;
        WinnerActorNumber = -1;
    }
}
