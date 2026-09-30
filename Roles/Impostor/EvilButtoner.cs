using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using TownOfHost.Patches;
using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Impostor;

/// <summary>
/// [TOH-P-P] イビルボタナー
/// ワンクリックボタン(ファントムボタン)で、どこからでも緊急会議ボタンを押せる(回数制限あり)。
/// </summary>
public sealed class EvilButtoner : RoleBase, IImpostor, IUsePhantomButton
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(EvilButtoner),
            player => new EvilButtoner(player),
            CustomRoles.EvilButtoner,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            9700,
            SetupOptionItem,
            "ebt",
            OptionSort: (6, 17),
            from: From.TownOfHost_PP
        );
    public EvilButtoner(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        Cooldown = OptionCooldown.GetFloat();
        RemainingUses = OptionUseCount.GetInt();
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionUseCount;
    private static OptionItem OptionCooldown;
    private static float KillCooldown;
    private static float Cooldown;

    int RemainingUses;

    enum OptionName
    {
        ButtonerUseCount,
        ButtonerCooldown
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionUseCount = IntegerOptionItem.Create(RoleInfo, 11, OptionName.ButtonerUseCount, new(1, 5, 1), 1, false)
            .SetValueFormat(OptionFormat.Times);
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 12, OptionName.ButtonerCooldown, new(0f, 120f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public float CalculateKillCooldown() => KillCooldown;
    public override void ApplyGameOptions(IGameOptions opt)
        => AURoleOptions.PhantomCooldown = RemainingUses > 0 ? Cooldown : 200f;

    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        using var sender = CreateSender();
        sender.Writer.Write(RemainingUses);
    }
    public override void ReceiveRPC(MessageReader reader) => RemainingUses = reader.ReadInt32();

    /// <summary>リアクター・停電など、緊急ボタンが押せないサボタージュ中か</summary>
    public static bool IsCriticalSabotageActive()
        => Utils.IsActive(SystemTypes.Reactor)
        || Utils.IsActive(SystemTypes.Electrical)
        || Utils.IsActive(SystemTypes.Laboratory)
        || Utils.IsActive(SystemTypes.Comms)
        || Utils.IsActive(SystemTypes.LifeSupp)
        || Utils.IsActive(SystemTypes.HeliSabotage);

    public void OnClick(ref bool AdjustKillCooldown, ref bool? ResetCooldown)
    {
        AdjustKillCooldown = true;
        ResetCooldown = false;
        if (!AmongUsClient.Instance.AmHost) return;
        if (!Player.IsAlive() || RemainingUses <= 0 || IsCriticalSabotageActive()) return;

        ResetCooldown = true;
        RemainingUses--;
        SendRPC();
        Player.MarkDirtySettings();
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} が遠隔で緊急ボタンを押した(残り{RemainingUses})", "EvilButtoner");
        ReportDeadBodyPatch.ExReportDeadBody(Player, null, false);
    }
    public override bool CanUseAbilityButton() => RemainingUses > 0;
    bool IUsePhantomButton.IsPhantomRole => RemainingUses > 0;
    bool IUsePhantomButton.IsresetAfterKill => false;

    public override string GetProgressText(bool comms = false, bool gamelog = false)
        => Utils.ColorString(RemainingUses > 0 ? Palette.ImpostorRed : Color.gray, $"({RemainingUses})");
    public override string GetAbilityButtonText() => GetString("ButtonerButton");
}
