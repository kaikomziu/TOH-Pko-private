using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using TownOfHost.Patches;
using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] カメラマン
/// ペットを撫でると、その瞬間に近くにいたプレイヤーを撮影する。
/// 撮った写真は次の会議の開始時に公開される(設定で本人だけにもできる)。
/// </summary>
public sealed class Photographer : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Photographer),
            player => new Photographer(player),
            CustomRoles.Photographer,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            37900,
            SetupOptionItem,
            "pho",
            "#e8c170",
            (6, 10),
            from: From.TownOfHost_PP
        );
    public Photographer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Cooldown = OptionCooldown.GetFloat();
        Range = OptionRange.GetFloat();
        PublicPhotos = OptionPublic.GetBool();
        RemainingShots = OptionShotCount.GetInt();
        lastShotTime = -999f;
        photos.Clear();
    }
    private static OptionItem OptionCooldown;
    private static OptionItem OptionShotCount;
    private static OptionItem OptionRange;
    private static OptionItem OptionPublic;
    private static float Cooldown;
    private static float Range;
    private static bool PublicPhotos;

    int RemainingShots;
    float lastShotTime;
    /// <summary>次の会議で公開する写真(撮影した部屋, 写っていた人)</summary>
    private readonly List<(string room, List<byte> players)> photos = new();

    enum OptionName
    {
        PhotographerShotCount,
        PhotographerRange,
        PhotographerPublic
    }
    private static void SetupOptionItem()
    {
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.Cooldown, new(5f, 120f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionShotCount = IntegerOptionItem.Create(RoleInfo, 11, OptionName.PhotographerShotCount, new(1, 15, 1), 3, false)
            .SetValueFormat(OptionFormat.Times);
        OptionRange = FloatOptionItem.Create(RoleInfo, 12, OptionName.PhotographerRange, new(1f, 6f, 0.5f), 3f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionPublic = BooleanOptionItem.Create(RoleInfo, 13, OptionName.PhotographerPublic, true, false);
    }

    public override void Add() => PetActionManager.Register(Player.PlayerId, OnPet);
    public override void OnDestroy() => PetActionManager.Unregister(Player.PlayerId);

    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        using var sender = CreateSender();
        sender.Writer.Write(RemainingShots);
        sender.Writer.Write(photos.Count);
    }
    int pendingPhotoCount;
    public override void ReceiveRPC(MessageReader reader)
    {
        RemainingShots = reader.ReadInt32();
        pendingPhotoCount = reader.ReadInt32();
    }

    private void OnPet()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!Player.IsAlive() || GameStates.IsMeeting || !GameStates.IsInTask) return;
        if (RemainingShots <= 0) return;
        if (Time.time - lastShotTime < Cooldown) return;

        lastShotTime = Time.time;
        RemainingShots--;

        var pos = Player.GetTruePosition();
        var inShot = PlayerCatch.AllAlivePlayerControls
            .Where(pc => !Is(pc) && Vector2.Distance(pos, pc.GetTruePosition()) <= Range)
            .Select(pc => pc.PlayerId)
            .ToList();
        var room = Player.GetPlainShipRoom();
        var roomName = room == null ? GetString("PhotographerUnknownRoom") : DestroyableSingleton<TranslationController>.Instance.GetString(room.RoomId);
        photos.Add((roomName, inShot));

        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} が撮影({roomName}): {string.Join(",", inShot.Select(id => Main.AllPlayerNames.TryGetValue(id, out var n) ? n : id.ToString()))}", "Photographer");
        SendRPC();
        UtilsNotifyRoles.NotifyRoles(OnlyMeName: true, SpecifySeer: Player);
    }

    private string BuildPhotoText()
    {
        var lines = photos.Select(p =>
            string.Format(GetString("PhotographerPhotoLine"), p.room,
                p.players.Count == 0 ? GetString("PhotographerNobody") : string.Join("、", p.players.Select(id => UtilsName.GetPlayerColor(id)))));
        return string.Join("\n", lines);
    }
    public override void OnStartMeeting()
    {
        if (!AmongUsClient.Instance.AmHost || photos.Count == 0) return;
        // 公開しない設定のときは本人にだけ見せる
        if (!PublicPhotos)
        {
            MeetingHudPatch.EnqueuePostDayMeetingMessage(Player.PlayerId, BuildPhotoText(),
                Utils.ColorString(RoleInfo.RoleColor, GetString("PhotographerTitle")));
            photos.Clear();
            SendRPC();
        }
    }
    public override string MeetingAddMessage()
    {
        if (!PublicPhotos || photos.Count == 0) return "";
        var text = Utils.ColorString(RoleInfo.RoleColor, GetString("PhotographerTitle")) + "\n" + BuildPhotoText();
        photos.Clear();
        SendRPC();
        return text;
    }

    public override string GetProgressText(bool comms = false, bool gamelog = false)
        => Utils.ColorString(RemainingShots > 0 ? RoleInfo.RoleColor : Color.gray, $"({RemainingShots})");
    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        seen ??= seer;
        if (isForMeeting || !Is(seer) || !Is(seen) || !Player.IsAlive()) return "";
        var count = AmongUsClient.Instance.AmHost ? photos.Count : pendingPhotoCount;
        return count > 0 ? string.Format(GetString("PhotographerPending"), count) : "";
    }
}
