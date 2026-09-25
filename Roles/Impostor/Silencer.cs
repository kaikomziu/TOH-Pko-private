using AmongUs.GameOptions;
using Hazel;

using TownOfHost.Modules;
using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;
using static TownOfHost.Modules.MeetingVoteManager;

namespace TownOfHost.Roles.Impostor;

/// <summary>
/// [TOH-P-P] 口止め屋
/// シェイプシフトボタンで1人を指定すると、次の会議でその人の投票が無効(0票)になる。
/// </summary>
public sealed class Silencer : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Silencer),
            player => new Silencer(player),
            CustomRoles.Silencer,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            8600,
            SetupOptionItem,
            "sil",
            OptionSort: (6, 16),
            from: From.TownOfHost_PP
        );
    public Silencer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        Cooldown = OptionCooldown.GetFloat();
        Announce = OptionAnnounce.GetBool();
        SilenceCount = OptionSilenceCount.GetInt();

        TargetId = byte.MaxValue;
        SilencedId = byte.MaxValue;
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionCooldown;
    private static OptionItem OptionSilenceCount;
    private static OptionItem OptionAnnounce;
    private static float KillCooldown;
    private static float Cooldown;
    private static bool Announce;

    int SilenceCount;
    /// <summary>次の会議で口止めする相手</summary>
    byte TargetId;
    /// <summary>今の会議で口止めされている相手</summary>
    byte SilencedId;

    enum OptionName
    {
        SilencerSilenceCount,
        SilencerAnnounce
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 11, GeneralOption.Cooldown, OptionBaseCoolTime, 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSilenceCount = IntegerOptionItem.Create(RoleInfo, 12, OptionName.SilencerSilenceCount, new(1, 15, 1), 2, false)
            .SetValueFormat(OptionFormat.Times);
        OptionAnnounce = BooleanOptionItem.Create(RoleInfo, 13, OptionName.SilencerAnnounce, true, false);
    }
    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        using var sender = CreateSender();
        sender.Writer.Write(TargetId);
        sender.Writer.Write(SilencedId);
        sender.Writer.Write(SilenceCount);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        TargetId = reader.ReadByte();
        SilencedId = reader.ReadByte();
        SilenceCount = reader.ReadInt32();
    }

    public override bool CheckShapeshift(PlayerControl target, ref bool animate)
    {
        // 変身はさせず、ターゲット指定だけ行う
        animate = false;
        if (target == null || target.PlayerId == Player.PlayerId || target.IsTeammate(Player)) return false;
        if (SilenceCount <= 0 || TargetId != byte.MaxValue) return false;

        TargetId = target.PlayerId;
        SilenceCount--;
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} が {target.GetNameWithRole().RemoveHtmlTags()} を口止め", "Silencer");
        Player.RpcResetAbilityCooldown();
        SendRPC();
        UtilsNotifyRoles.NotifyRoles(SpecifySeer: Player);
        return false;
    }

    public override void OnStartMeeting()
    {
        SilencedId = TargetId;
        TargetId = byte.MaxValue;
        SendRPC();
        if (!AmongUsClient.Instance.AmHost || SilencedId == byte.MaxValue) return;

        var silenced = PlayerCatch.GetPlayerById(SilencedId);
        if (silenced == null || !silenced.IsAlive()) return;
        // 本人には必ず知らせる
        MeetingHudPatch.EnqueuePostDayMeetingMessage(SilencedId,
            GetString("SilencerYouAreSilenced"),
            Utils.ColorString(RoleInfo.RoleColor, GetString("Silencer")));
    }
    public override string MeetingAddMessage()
    {
        if (!Announce || SilencedId == byte.MaxValue) return "";
        var silenced = PlayerCatch.GetPlayerById(SilencedId);
        if (silenced == null || !silenced.IsAlive()) return "";
        return string.Format(GetString("SilencerAnnounceText"), UtilsName.GetPlayerColor(SilencedId));
    }
    public override (byte? votedForId, int? numVotes, bool doVote) ModifyVote(byte voterId, byte sourceVotedForId, bool isIntentional)
    {
        var (votedForId, numVotes, doVote) = base.ModifyVote(voterId, sourceVotedForId, isIntentional);
        if (SilencedId != byte.MaxValue && voterId == SilencedId && sourceVotedForId != NoVote)
        {
            numVotes = 0;
        }
        return (votedForId, numVotes, doVote);
    }
    public override void AfterMeetingTasks()
    {
        SilencedId = byte.MaxValue;
        SendRPC();
    }

    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        seen ??= seer;
        if (isForMeeting || !Player.IsAlive()) return "";
        if (!Is(seer) || !Is(seen)) return "";
        return TargetId != byte.MaxValue ? $"{GetString("SilencerTarget")}:{UtilsName.GetPlayerColor(TargetId)}" : "";
    }
    public override string GetMark(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        var id = isForMeeting ? SilencedId : TargetId;
        return id != byte.MaxValue && seen.PlayerId == id ? Utils.ColorString(Palette.ImpostorRed, "×") : "";
    }
    public override string GetProgressText(bool comms = false, bool gamelog = false) => Utils.ColorString(RoleInfo.RoleColor, $"({SilenceCount})");
    public override void ApplyGameOptions(IGameOptions opt) => AURoleOptions.ShapeshifterCooldown = Cooldown;
    public float CalculateKillCooldown() => KillCooldown;
    public override string GetAbilityButtonText() => GetString("SilencerButton");
}
