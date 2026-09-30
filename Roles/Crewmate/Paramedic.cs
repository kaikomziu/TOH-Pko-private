using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] 救急隊員
/// 自分で死体を通報すると、その人が死んでから何秒たっていたかが「25〜30秒」のような幅で本人に届く。
/// </summary>
public sealed class Paramedic : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Paramedic),
            player => new Paramedic(player),
            CustomRoles.Paramedic,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            40000,
            SetupOptionItem,
            "pmd",
            "#ff8a8a",
            (6, 12),
            from: From.TownOfHost_PP
        );
    public Paramedic(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Width = OptionWidth.GetInt();
        deathTimes.Clear();
        CustomRoleManager.OnMurderPlayerOthers.Add(HandleMurder);
    }
    private static OptionItem OptionWidth;
    private static int Width;

    /// <summary>死んだ時刻(Time.time)</summary>
    private static readonly Dictionary<byte, float> deathTimes = new();

    enum OptionName
    {
        ParamedicWidth
    }
    private static void SetupOptionItem()
    {
        OptionWidth = IntegerOptionItem.Create(RoleInfo, 10, OptionName.ParamedicWidth, new(1, 30, 1), 5, false)
            .SetValueFormat(OptionFormat.Seconds);
    }

    private static void HandleMurder(MurderInfo info)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var target = info.AttemptTarget;
        if (target == null) return;
        deathTimes[target.PlayerId] = Time.time;
    }

    public override void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (reporter == null || target == null || !Is(reporter) || !Player.IsAlive()) return;
        if (!deathTimes.TryGetValue(target.PlayerId, out var diedAt)) return;

        // 例: 幅5秒で28秒経過 → 「25〜30秒」
        var elapsed = Mathf.Max(0f, Time.time - diedAt);
        var low = Mathf.FloorToInt(elapsed / Width) * Width;
        var high = low + Width;
        MeetingHudPatch.EnqueuePostDayMeetingMessage(Player.PlayerId,
            string.Format(GetString("ParamedicResult"), UtilsName.GetPlayerColor(target.PlayerId), low, high),
            Utils.ColorString(RoleInfo.RoleColor, GetString("Paramedic")));
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} が通報: 死後 {elapsed:0.0}秒 → {low}〜{high}秒", "Paramedic");
    }
}
