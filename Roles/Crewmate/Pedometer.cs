using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using UnityEngine;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] 万歩計
/// 会議の最初に、前のターンで「一番たくさん歩いた人」と「一番動かなかった人」が本人に届く。
/// </summary>
public sealed class Pedometer : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Pedometer),
            player => new Pedometer(player),
            CustomRoles.Pedometer,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            41000,
            SetupOptionItem,
            "ped",
            "#7fb77e",
            (6, 12),
            from: From.TownOfHost_PP
        );
    public Pedometer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        ShowLeast = OptionShowLeast.GetBool();
        walked.Clear();
        lastPos.Clear();
        CustomRoleManager.OnFixedUpdateOthers.Add(TrackMovement);
    }
    private static OptionItem OptionShowLeast;
    private static bool ShowLeast;

    /// <summary>このターンに歩いた距離。万歩計全員で共有</summary>
    private static readonly Dictionary<byte, float> walked = new();
    private static readonly Dictionary<byte, Vector2> lastPos = new();
    /// <summary>1フレームでこれ以上動いたらワープ(ベント・梯子・スポーン等)とみなして数えない</summary>
    const float MaxStep = 1f;

    enum OptionName
    {
        PedometerShowLeast
    }
    private static void SetupOptionItem()
    {
        OptionShowLeast = BooleanOptionItem.Create(RoleInfo, 10, OptionName.PedometerShowLeast, true, false);
    }

    private static void TrackMovement(PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost || pc == null) return;
        if (!GameStates.IsInTask || GameStates.IsMeeting) return;
        if (!pc.IsAlive())
        {
            lastPos.Remove(pc.PlayerId);
            return;
        }
        Vector2 pos = pc.transform.position;
        if (lastPos.TryGetValue(pc.PlayerId, out var prev))
        {
            var step = Vector2.Distance(prev, pos);
            if (step < MaxStep)
                walked[pc.PlayerId] = (walked.TryGetValue(pc.PlayerId, out var d) ? d : 0f) + step;
        }
        lastPos[pc.PlayerId] = pos;
    }

    public override void OnStartMeeting()
    {
        if (!AmongUsClient.Instance.AmHost || !Player.IsAlive()) return;
        var others = PlayerCatch.AllAlivePlayerControls
            .Where(pc => pc.PlayerId != Player.PlayerId)
            .Select(pc => (id: pc.PlayerId, dist: walked.TryGetValue(pc.PlayerId, out var d) ? d : 0f))
            .ToList();
        if (others.Count < 2) return;

        var most = others.OrderByDescending(x => x.dist).First();
        var lines = new List<string> { string.Format(GetString("PedometerMost"), UtilsName.GetPlayerColor(most.id)) };
        if (ShowLeast)
        {
            var least = others.OrderBy(x => x.dist).First();
            lines.Add(string.Format(GetString("PedometerLeast"), UtilsName.GetPlayerColor(least.id)));
        }
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 歩数: " + string.Join(", ", others.Select(x => $"{x.id}:{x.dist:0.0}")), "Pedometer");
        MeetingHudPatch.EnqueuePostDayMeetingMessage(Player.PlayerId, string.Join("\n", lines),
            Utils.ColorString(RoleInfo.RoleColor, GetString("PedometerTitle")));
    }
    // 会議後は全員の位置が飛ぶので、記録をリセットして次のターンを数え直す
    public override void AfterMeetingTasks()
    {
        walked.Clear();
        lastPos.Clear();
    }
}
