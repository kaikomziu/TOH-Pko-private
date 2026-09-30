using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Impostor;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] 目撃者
/// 自分の近くでキルが起きると、次の会議で「キラーの色が明るいか暗いか」だけ本人に通知される。
/// </summary>
public sealed class Witness : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Witness),
            player => new Witness(player),
            CustomRoles.Witness,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            37600,
            SetupOptionItem,
            "wit",
            "#9fd4a3",
            (6, 9),
            from: From.TownOfHost_PP
        );
    public Witness(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Range = OptionRange.GetFloat();
        TellVictim = OptionTellVictim.GetBool();
        sightings.Clear();

        CustomRoleManager.OnMurderPlayerOthers.Add(HandleMurder);
        instances.Add(this);
    }
    private static OptionItem OptionRange;
    private static OptionItem OptionTellVictim;
    private static float Range;
    private static bool TellVictim;

    private static readonly HashSet<Witness> instances = new();
    /// <summary>次の会議で伝える目撃情報(被害者ID, 明るい色か, 見分けられなかったか)</summary>
    private readonly List<(byte victimId, bool isLight, bool unknown)> sightings = new();

    enum OptionName
    {
        WitnessRange,
        WitnessTellVictim
    }
    private static void SetupOptionItem()
    {
        OptionRange = FloatOptionItem.Create(RoleInfo, 10, OptionName.WitnessRange, new(1f, 6f, 0.5f), 3f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionTellVictim = BooleanOptionItem.Create(RoleInfo, 11, OptionName.WitnessTellVictim, true, false);
    }
    public override void OnDestroy() => instances.Remove(this);

    private static void HandleMurder(MurderInfo info)
    {
        if (!AmongUsClient.Instance.AmHost || info.IsSuicide) return;
        foreach (var witness in instances)
            witness.OnMurder(info);
    }
    private void OnMurder(MurderInfo info)
    {
        var killer = info.AppearanceKiller ?? info.AttemptKiller;
        var target = info.AttemptTarget;
        if (killer == null || target == null) return;
        if (!Player.IsAlive() || Is(killer) || Is(target)) return;
        if (Vector2.Distance(Player.GetTruePosition(), target.GetTruePosition()) > Range) return;

        var unknown = Camouflage.IsCamouflage || Camouflager.NowUse;
        var isLight = !unknown && IsLightColor(killer.CurrentOutfit.ColorId);
        sightings.Add((target.PlayerId, isLight, unknown));
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} がキルを目撃: 被害者 {target.GetNameWithRole().RemoveHtmlTags()} / {(unknown ? "不明" : isLight ? "明るい色" : "暗い色")}", "Witness");
    }
    /// <summary>プレイヤーの色が明るい色かどうか(相対輝度で判定)</summary>
    private static bool IsLightColor(int colorId)
    {
        if (colorId < 0 || colorId >= Palette.PlayerColors.Length) return false;
        Color32 c = Palette.PlayerColors[colorId];
        return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b >= 140f;
    }

    public override void OnStartMeeting()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (sightings.Count == 0) return;
        if (Player.IsAlive())
        {
            var lines = new List<string>();
            foreach (var (victimId, isLight, unknown) in sightings)
            {
                var color = unknown ? GetString("WitnessUnknownColor") : GetString(isLight ? "WitnessLightColor" : "WitnessDarkColor");
                lines.Add(TellVictim
                    ? string.Format(GetString("WitnessReportWithVictim"), UtilsName.GetPlayerColor(victimId), color)
                    : string.Format(GetString("WitnessReport"), color));
            }
            MeetingHudPatch.EnqueuePostDayMeetingMessage(Player.PlayerId, string.Join("\n", lines),
                Utils.ColorString(RoleInfo.RoleColor, GetString("Witness")));
        }
        sightings.Clear();
    }
    public override string GetProgressText(bool comms = false, bool gamelog = false)
        => sightings.Count > 0 ? Utils.ColorString(RoleInfo.RoleColor, "◉") : "";
}
