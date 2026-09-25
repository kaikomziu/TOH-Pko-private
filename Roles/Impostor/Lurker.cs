using AmongUs.GameOptions;
using UnityEngine;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Impostor;

/// <summary>
/// [TOH-P-P] 潜伏者
/// ベントに潜っている間、キルクールが通常より速く進む。
/// ベントから出た時点で「潜っていた時間 × 倍率」だけ残りキルクールを減らす。
/// </summary>
public sealed class Lurker : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Lurker),
            player => new Lurker(player),
            CustomRoles.Lurker,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            8500,
            SetupOptionItem,
            "lur",
            OptionSort: (7, 13),
            from: From.TownOfHost_PP
        );
    public Lurker(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        VentSpeed = OptionVentSpeed.GetFloat();

        cooldownStartTime = -1f;
        cooldownLength = 0f;
        ventEnterTime = -1f;
        ventEnterRemaining = 0f;
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionVentSpeed;
    private static float KillCooldown;
    private static float VentSpeed;

    // ホスト側で追跡しているキルクール(開始時刻と長さ)
    float cooldownStartTime;
    float cooldownLength;
    // ベントに入った時刻と、その時点の残りキルクール
    float ventEnterTime;
    float ventEnterRemaining;

    // ゲーム開始直後のキルクール(バニラ仕様)
    const float FirstKillCooldown = 10f;
    // これより短い潜伏は無視する
    const float MinLurkTime = 0.5f;

    enum OptionName
    {
        LurkerVentSpeed
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionVentSpeed = FloatOptionItem.Create(RoleInfo, 11, OptionName.LurkerVentSpeed, new(1.5f, 10f, 0.5f), 3f, false)
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
        ventEnterTime = -1f;
        StartTracking(KillCooldown);
    }
    public override void OnStartMeeting() => ventEnterTime = -1f;

    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return;
        StartTracking(KillCooldown);
    }

    public override bool OnEnterVent(PlayerPhysics physics, int ventId)
    {
        MarkEnter();
        return true;
    }
    public override void OnVentilationSystemUpdate(PlayerControl user, VentilationSystem.Operation Operation, int ventId)
    {
        if (!AmongUsClient.Instance.AmHost || !Is(user)) return;

        if (Operation is VentilationSystem.Operation.Enter)
            MarkEnter();
        if (Operation is VentilationSystem.Operation.Exit or VentilationSystem.Operation.BootImpostors)
            OnExitVent();
    }
    void MarkEnter()
    {
        if (!AmongUsClient.Instance.AmHost || ventEnterTime >= 0f) return;
        ventEnterTime = Time.time;
        ventEnterRemaining = TrackedRemaining;
    }
    void OnExitVent()
    {
        if (ventEnterTime < 0f) return;
        var lurkTime = Time.time - ventEnterTime;
        ventEnterTime = -1f;

        if (!Player.IsAlive() || GameStates.IsMeeting) return;
        if (lurkTime < MinLurkTime || ventEnterRemaining <= 0.1f) return;

        // 潜っていた時間 × 倍率 だけ、入った時点の残りキルクールから減らす
        var newCooldown = Mathf.Max(0.1f, ventEnterRemaining - lurkTime * VentSpeed);
        Player.SetKillCooldown(newCooldown);
        StartTracking(newCooldown);
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 潜伏{lurkTime:0.0}秒 → キルクール {ventEnterRemaining:0.0} → {newCooldown:0.0}", "Lurker");
    }
}
