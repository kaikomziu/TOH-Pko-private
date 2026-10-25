using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] 逆探知
/// 誰かがサボタージュを起こした瞬間、起こした人がいた部屋の名前が分かる(誰かは分からない)。
/// 結果は画面下に表示され、会議の最初にもまとめて届く。ドア閉めは対象外。
/// </summary>
public sealed class ReverseTracer : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(ReverseTracer),
            player => new ReverseTracer(player),
            CustomRoles.ReverseTracer,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            41400,
            SetupOptionItem,
            "rtr",
            "#5fa8d3",
            (6, 13),
            from: From.TownOfHost_PP
        );
    public ReverseTracer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        MaxUses = OptionMaxUses.GetInt();
        StartTurn = OptionStartTurn.GetInt();
        usesLeft = MaxUses;
        turn = 1;
        lastRoom = "";
        results.Clear();
    }
    private static OptionItem OptionMaxUses;
    private static OptionItem OptionStartTurn;
    private static int MaxUses;
    private static int StartTurn;

    int usesLeft;
    int turn;
    /// <summary>直近のサボタージュの発生源(画面下に表示)</summary>
    string lastRoom;
    /// <summary>このターンの結果(会議で届ける)</summary>
    readonly List<string> results = new();

    enum OptionName
    {
        ReverseTracerMaxUses,
        ReverseTracerStartTurn
    }
    private static void SetupOptionItem()
    {
        OptionMaxUses = IntegerOptionItem.Create(RoleInfo, 10, OptionName.ReverseTracerMaxUses, new(1, 15, 1), 3, false)
            .SetValueFormat(OptionFormat.Times);
        OptionStartTurn = IntegerOptionItem.Create(RoleInfo, 11, OptionName.ReverseTracerStartTurn, new(1, 10, 1), 1, false);
    }

    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        using var sender = CreateSender();
        sender.Writer.Write(usesLeft);
        sender.Writer.Write(lastRoom);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        usesLeft = reader.ReadInt32();
        lastRoom = reader.ReadString();
    }

    public override bool OnSabotage(PlayerControl player, SystemTypes systemType)
    {
        if (!AmongUsClient.Instance.AmHost || player == null || Is(player)) return true;
        if (!Player.IsAlive() || usesLeft <= 0 || turn < StartTurn) return true;

        var room = player.GetShipRoomName();
        if (string.IsNullOrEmpty(room)) room = GetString("ReverseTracerUnknownRoom");
        usesLeft--;
        lastRoom = room;
        results.Add(room);
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 逆探知: {systemType} の発生源 {room} ({player.GetNameWithRole().RemoveHtmlTags()}) 残り{usesLeft}回", "ReverseTracer");
        SendRPC();
        UtilsNotifyRoles.NotifyRoles(OnlyMeName: true, SpecifySeer: Player);
        return true;
    }

    public override void OnStartMeeting()
    {
        if (!AmongUsClient.Instance.AmHost || !Player.IsAlive() || results.Count == 0) return;
        var lines = new List<string>();
        foreach (var room in results)
            lines.Add(string.Format(GetString("ReverseTracerResult"), room));
        MeetingHudPatch.EnqueuePostDayMeetingMessage(Player.PlayerId, string.Join("\n", lines),
            Utils.ColorString(RoleInfo.RoleColor, GetString("ReverseTracerTitle")));
    }
    public override void AfterMeetingTasks()
    {
        turn++;
        lastRoom = "";
        results.Clear();
        SendRPC();
    }

    public override string GetProgressText(bool comms = false, bool gamelog = false)
        => Utils.ColorString(usesLeft > 0 ? RoleInfo.RoleColor : UnityEngine.Color.gray, $"({usesLeft})");
    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        seen ??= seer;
        if (isForMeeting || !Is(seer) || !Is(seen) || !Player.IsAlive() || lastRoom == "") return "";
        return Utils.ColorString(RoleInfo.RoleColor, string.Format(GetString("ReverseTracerResult"), lastRoom));
    }
}
