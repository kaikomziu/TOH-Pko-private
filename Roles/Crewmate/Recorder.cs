using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] 記録係
/// 会議の最初に、前のターンで死んだ人の「死んだ順番」だけが本人に届く。誰が殺したかは分からない。
/// </summary>
public sealed class Recorder : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Recorder),
            player => new Recorder(player),
            CustomRoles.Recorder,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            39700,
            null,
            "rec",
            "#b5c7d3",
            (6, 11),
            from: From.TownOfHost_PP
        );
    public Recorder(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        deathOrder.Clear();
        CustomRoleManager.OnMurderPlayerOthers.Add(HandleMurder);
    }

    /// <summary>このターンに死んだ人(死んだ順)。記録係全員で共有</summary>
    private static readonly List<byte> deathOrder = new();

    private static void HandleMurder(MurderInfo info)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!GameStates.IsInTask || GameStates.IsMeeting) return;
        var target = info.AttemptTarget;
        if (target == null || deathOrder.Contains(target.PlayerId)) return;
        deathOrder.Add(target.PlayerId);
    }

    public override void OnStartMeeting()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (Player.IsAlive() && deathOrder.Count > 0)
        {
            var lines = deathOrder.Select((id, i) => string.Format(GetString("RecorderLine"), i + 1, UtilsName.GetPlayerColor(id)));
            MeetingHudPatch.EnqueuePostDayMeetingMessage(Player.PlayerId, string.Join("\n", lines),
                Utils.ColorString(RoleInfo.RoleColor, GetString("RecorderTitle")));
        }
    }
    // 記録は会議が終わったら次のターン用に空にする(記録係が複数いても全員に届いてから消す)
    public override void AfterMeetingTasks() => deathOrder.Clear();
}
