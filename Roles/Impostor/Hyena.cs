using AmongUs.GameOptions;
using UnityEngine;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Impostor;

/// <summary>
/// [TOH-P-P] ハイエナ
/// 死体の近くにいる間、キルクールが通常より速く進む。
/// 死体から離れた時点で「近くにいた時間 × 倍率」だけ残りキルクールを減らす(潜伏者と同じ方式)。
/// </summary>
public sealed class Hyena : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Hyena),
            player => new Hyena(player),
            CustomRoles.Hyena,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            10400,
            SetupOptionItem,
            "hye",
            OptionSort: (7, 14),
            from: From.TownOfHost_PP
        );
    public Hyena(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        SpeedRate = OptionSpeedRate.GetFloat();
        Range = OptionRange.GetFloat();

        cooldownStartTime = -1f;
        cooldownLength = 0f;
        nearStartTime = -1f;
        nearStartRemaining = 0f;
        checkTimer = 0f;
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionSpeedRate;
    private static OptionItem OptionRange;
    private static float KillCooldown;
    private static float SpeedRate;
    private static float Range;

    float cooldownStartTime;
    float cooldownLength;
    float nearStartTime;
    float nearStartRemaining;
    float checkTimer;

    const float FirstKillCooldown = 10f;
    const float MinNearTime = 0.5f;
    const float CheckInterval = 0.25f;

    enum OptionName
    {
        HyenaSpeedRate,
        HyenaRange
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSpeedRate = FloatOptionItem.Create(RoleInfo, 11, OptionName.HyenaSpeedRate, new(1.5f, 10f, 0.5f), 3f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionRange = FloatOptionItem.Create(RoleInfo, 12, OptionName.HyenaRange, new(1f, 5f, 0.5f), 2f, false)
            .SetValueFormat(OptionFormat.Multiplier);
    }
    public float CalculateKillCooldown() => KillCooldown;

    void StartTracking(float length)
    {
        cooldownStartTime = Time.time;
        cooldownLength = length;
    }
    float TrackedRemaining => cooldownStartTime < 0f ? 0f : Mathf.Max(0f, cooldownLength - (Time.time - cooldownStartTime));

    public override void StartGameTasks() => StartTracking(FirstKillCooldown);
    public override void AfterMeetingTasks()
    {
        nearStartTime = -1f;
        StartTracking(KillCooldown);
    }
    public override void OnStartMeeting() => nearStartTime = -1f;

    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return;
        // キル直後はクールがリセットされるので、死体のそばにいた時間も数え直す
        nearStartTime = -1f;
        StartTracking(KillCooldown);
    }

    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !Is(player)) return;
        checkTimer += Time.fixedDeltaTime;
        if (checkTimer < CheckInterval) return;
        checkTimer = 0f;

        if (!Player.IsAlive() || !GameStates.IsInTask || GameStates.IsMeeting)
        {
            nearStartTime = -1f;
            return;
        }

        var near = IsNearDeadBody();
        if (near && nearStartTime < 0f)
        {
            nearStartTime = Time.time;
            nearStartRemaining = TrackedRemaining;
        }
        else if (!near && nearStartTime >= 0f)
        {
            ApplyBonus(Time.time - nearStartTime);
            nearStartTime = -1f;
        }
    }
    bool IsNearDeadBody()
    {
        var pos = Player.GetTruePosition();
        foreach (var body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body == null) continue;
            if (Vector2.Distance(pos, body.TruePosition) <= Range) return true;
        }
        return false;
    }
    void ApplyBonus(float nearTime)
    {
        if (nearTime < MinNearTime || nearStartRemaining <= 0.1f) return;

        // 死体のそばにいた時間 × 倍率 だけ、近づいた時点の残りキルクールから減らす
        var newCooldown = Mathf.Max(0.1f, nearStartRemaining - nearTime * SpeedRate);
        if (newCooldown >= TrackedRemaining) return;
        Player.SetKillCooldown(newCooldown);
        StartTracking(newCooldown);
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 死体のそば{nearTime:0.0}秒 → キルクール {nearStartRemaining:0.0} → {newCooldown:0.0}", "Hyena");
    }
}
