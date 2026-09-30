using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using TownOfHost.Patches;
using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] ナイスボタナー
/// ペットを撫でると、どこからでも緊急会議ボタンを押せる(回数制限あり)。
/// </summary>
public sealed class NiceButtoner : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(NiceButtoner),
            player => new NiceButtoner(player),
            CustomRoles.NiceButtoner,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            38300,
            SetupOptionItem,
            "nbt",
            "#7fb8e0",
            (3, 11),
            from: From.TownOfHost_PP
        );
    public NiceButtoner(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Cooldown = OptionCooldown.GetFloat();
        RemainingUses = OptionUseCount.GetInt();
        roundStartTime = Time.time;
    }
    private static OptionItem OptionUseCount;
    private static OptionItem OptionCooldown;
    private static float Cooldown;

    int RemainingUses;
    /// <summary>タスクターン開始時刻(ここから Cooldown 秒たつまで使えない)</summary>
    float roundStartTime;

    enum OptionName
    {
        ButtonerUseCount,
        ButtonerCooldown
    }
    private static void SetupOptionItem()
    {
        OptionUseCount = IntegerOptionItem.Create(RoleInfo, 10, OptionName.ButtonerUseCount, new(1, 5, 1), 1, false)
            .SetValueFormat(OptionFormat.Times);
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 11, OptionName.ButtonerCooldown, new(0f, 120f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Add() => PetActionManager.Register(Player.PlayerId, OnPet);
    public override void OnDestroy() => PetActionManager.Unregister(Player.PlayerId);
    public override void StartGameTasks() => roundStartTime = Time.time;
    public override void AfterMeetingTasks() => roundStartTime = Time.time;

    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        using var sender = CreateSender();
        sender.Writer.Write(RemainingUses);
    }
    public override void ReceiveRPC(MessageReader reader) => RemainingUses = reader.ReadInt32();

    private void OnPet()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!Player.IsAlive() || !GameStates.IsInTask || GameStates.IsMeeting) return;
        if (RemainingUses <= 0) return;
        if (Time.time - roundStartTime < Cooldown) return;
        if (Impostor.EvilButtoner.IsCriticalSabotageActive()) return;

        RemainingUses--;
        SendRPC();
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} が遠隔で緊急ボタンを押した(残り{RemainingUses})", "NiceButtoner");
        ReportDeadBodyPatch.ExReportDeadBody(Player, null, false);
    }

    public override string GetProgressText(bool comms = false, bool gamelog = false)
        => Utils.ColorString(RemainingUses > 0 ? RoleInfo.RoleColor : Color.gray, $"({RemainingUses})");
}
