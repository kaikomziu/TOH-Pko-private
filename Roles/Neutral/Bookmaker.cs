using AmongUs.GameOptions;
using Hazel;

using TownOfHost.Modules;
using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;
using static TownOfHost.Modules.SelfVoteManager;

namespace TownOfHost.Roles.Neutral;

/// <summary>
/// [TOH-P-P] ブックメーカー
/// 会議で自投票 → 予想モード。次に投票した人を「この会議で追放される人」として予想する(その投票は数えない)。
/// 予想が当たった回数が規定数に達すると単独勝利。
/// </summary>
public sealed class Bookmaker : RoleBase, ISelfVoter
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Bookmaker),
            player => new Bookmaker(player),
            CustomRoles.Bookmaker,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Neutral,
            57400,
            SetupOptionItem,
            "bkm",
            "#c9a227",
            (4, 8),
            from: From.TownOfHost_PP
        );
    public Bookmaker(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        NeedHits = OptionNeedHits.GetInt();
        Hits = 0;
        PredictionId = byte.MaxValue;
    }
    private static OptionItem OptionNeedHits;
    private static int NeedHits;

    int Hits;
    /// <summary>この会議での予想(追放されると思う人)</summary>
    byte PredictionId;

    enum OptionName
    {
        BookmakerNeedHits
    }
    private static void SetupOptionItem()
    {
        OptionNeedHits = IntegerOptionItem.Create(RoleInfo, 10, OptionName.BookmakerNeedHits, new(1, 10, 1), 3, false)
            .SetValueFormat(OptionFormat.Times);
    }
    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        using var sender = CreateSender();
        sender.Writer.Write(Hits);
        sender.Writer.Write(PredictionId);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        Hits = reader.ReadInt32();
        PredictionId = reader.ReadByte();
    }

    bool ISelfVoter.CanUseVoted() => CanPredict();
    private bool CanPredict() => Canuseability() && Player.IsAlive();

    public override bool CheckVoteAsVoter(byte votedForId, PlayerControl voter)
    {
        if (!Is(voter) || !CanPredict()) return true;

        if (CheckSelfVoteMode(Player, votedForId, out var status))
        {
            switch (status)
            {
                case VoteStatus.Self:
                    Utils.SendMessage(GetString("BookmakerModeOn"), Player.PlayerId);
                    SetMode(Player, true);
                    break;
                case VoteStatus.Skip:
                    Utils.SendMessage(GetString("VoteSkillFin"), Player.PlayerId);
                    SetMode(Player, false);
                    break;
                case VoteStatus.Vote:
                    var target = PlayerCatch.GetPlayerById(votedForId);
                    if (target != null && target.IsAlive())
                    {
                        PredictionId = votedForId;
                        Utils.SendMessage(string.Format(GetString("BookmakerPredicted"), UtilsName.GetPlayerColor(votedForId)), Player.PlayerId);
                        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} の予想: {target.GetNameWithRole().RemoveHtmlTags()}", "Bookmaker");
                    }
                    SetMode(Player, false);
                    break;
            }
            SendRPC();
            return false;
        }
        return true;
    }

    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!Player.IsAlive() || exiled == null) return;
        if (PredictionId == byte.MaxValue || exiled.PlayerId != PredictionId) return;

        Hits++;
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} 予想的中 {Hits}/{NeedHits}", "Bookmaker");
        SendRPC();

        if (Hits < NeedHits || DecidedWinner) return;
        if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default) return;
        if (CustomWinnerHolder.ResetAndSetAndChWinner(CustomWinner.Bookmaker, Player.PlayerId))
        {
            CustomWinnerHolder.NeutralWinnerIds.Add(Player.PlayerId);
            CustomWinnerHolder.WinnerIds.Add(Player.PlayerId);
        }
    }
    public override void AfterMeetingTasks()
    {
        PredictionId = byte.MaxValue;
        SendRPC();
    }

    public override string GetProgressText(bool comms = false, bool gamelog = false)
        => Utils.ColorString(RoleInfo.RoleColor, $"({Hits}/{NeedHits})");
    public override string GetMark(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        return isForMeeting && PredictionId != byte.MaxValue && seen.PlayerId == PredictionId
            ? Utils.ColorString(RoleInfo.RoleColor, "$") : "";
    }
}
