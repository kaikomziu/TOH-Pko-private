using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Neutral;

/// <summary>
/// [TOH-P-P] 闇のうどん屋
/// 1ターンの間(会議と会議の間)に規定数キルした状態で、能力ボタン(消える)を押すと「製麺」開始。
/// 製麺が終わるまで会議が始まらなければ単独勝利。会議が始まる(通報・緊急ボタン問わず)とキル数はリセット。
/// </summary>
public sealed class DarkUdon : RoleBase, IKiller, IUsePhantomButton
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(DarkUdon),
            player => new DarkUdon(player),
            CustomRoles.DarkUdon,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Neutral,
            57700,
            SetupOptionItem,
            "udon",
            "#d9c9a3",
            (2, 8),
            true,
            introSound: () => GetIntroSound(RoleTypes.Phantom),
            from: From.TownOfHost_PP
        );
    public DarkUdon(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        NeedKills = OptionNeedKills.GetInt();
        MakeTime = OptionMakeTime.GetFloat();
        kills = 0;
        making = false;
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionNeedKills;
    private static OptionItem OptionMakeTime;
    private static float KillCooldown;
    private static int NeedKills;
    private static float MakeTime;

    /// <summary>このターン(前の会議以降)のキル数</summary>
    int kills;
    /// <summary>製麺中か</summary>
    bool making;
    int makeSerial;

    enum OptionName
    {
        DarkUdonNeedKills,
        DarkUdonMakeTime
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 25f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionNeedKills = IntegerOptionItem.Create(RoleInfo, 11, OptionName.DarkUdonNeedKills, new(1, 10, 1), 3, false)
            .SetValueFormat(OptionFormat.Players);
        OptionMakeTime = FloatOptionItem.Create(RoleInfo, 12, OptionName.DarkUdonMakeTime, new(1f, 30f, 1f), 5f, false)
            .SetValueFormat(OptionFormat.Seconds);
        SoloWinOption.Create(RoleInfo, 13, defo: 1);
    }

    public float CalculateKillCooldown() => KillCooldown;
    public bool CanUseKillButton() => Player.IsAlive();
    public bool CanUseImpostorVentButton() => false;
    public bool CanUseSabotageButton() => false;
    public override void ApplyGameOptions(IGameOptions opt)
    {
        opt.SetVision(false);
        AURoleOptions.PhantomCooldown = 1f;
    }
    bool IUsePhantomButton.IsresetAfterKill => false;

    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        using var sender = CreateSender();
        sender.Writer.Write(kills);
        sender.Writer.Write(making);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        kills = reader.ReadInt32();
        making = reader.ReadBoolean();
    }

    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return;
        kills++;
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} このターン {kills}/{NeedKills} キル", "DarkUdon");
        SendRPC();
        UtilsNotifyRoles.NotifyRoles(OnlyMeName: true, SpecifySeer: Player);
    }

    public void OnClick(ref bool AdjustKillCooldown, ref bool? ResetCooldown)
    {
        AdjustKillCooldown = true;
        ResetCooldown = false;
        if (!AmongUsClient.Instance.AmHost) return;
        if (!Player.IsAlive() || !GameStates.IsInTask || GameStates.IsMeeting) return;
        if (making || kills < NeedKills) return;

        ResetCooldown = true;
        making = true;
        var serial = ++makeSerial;
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 製麺開始({MakeTime}秒)", "DarkUdon");
        SendRPC();
        UtilsNotifyRoles.NotifyRoles(OnlyMeName: true, SpecifySeer: Player);

        _ = new LateTask(() =>
        {
            if (serial != makeSerial || !making) return;
            if (!Player.IsAlive() || GameStates.IsMeeting || !GameStates.IsInTask) return;
            if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default) return;

            Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 製麺完了 → 単独勝利", "DarkUdon");
            if (CustomWinnerHolder.ResetAndSetAndChWinner(CustomWinner.DarkUdon, Player.PlayerId))
            {
                CustomWinnerHolder.NeutralWinnerIds.Add(Player.PlayerId);
            }
        }, MakeTime, "DarkUdon.Make", true);
    }

    /// <summary>会議が始まったら(通報・緊急ボタン問わず)キル数リセット・製麺失敗</summary>
    public override void OnStartMeeting()
    {
        if (kills > 0 || making)
            Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 会議開始でリセット(キル{kills}{(making ? "・製麺失敗" : "")})", "DarkUdon");
        kills = 0;
        making = false;
        makeSerial++;
        SendRPC();
    }
    public override void OnDead(PlayerControl player)
    {
        if (!Is(player)) return;
        making = false;
        makeSerial++;
    }

    public override string GetProgressText(bool comms = false, bool gamelog = false)
        => Utils.ColorString(kills >= NeedKills ? Color.yellow : RoleInfo.RoleColor, $"({kills}/{NeedKills})");
    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        seen ??= seer;
        if (isForMeeting || !Is(seer) || !Is(seen) || !Player.IsAlive()) return "";
        if (making) return Utils.ColorString(RoleInfo.RoleColor, GetString("DarkUdonMaking"));
        if (kills >= NeedKills) return Utils.ColorString(Color.yellow, GetString("DarkUdonReady"));
        return "";
    }
    public override string GetAbilityButtonText() => GetString("DarkUdonButton");
}
