using AmongUs.GameOptions;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Madmate;

/// <summary>
/// [TOH-P-P] マッドスター
/// スター(<see cref="Crewmate.Star"/>)のインポスター陣営版。能力なし。
/// ゲーム開始時から全員視点で名前が黄色く見え、インポスター陣営が勝つと一緒に勝利する(マッドメイト扱い)。
/// </summary>
public sealed class MadStar : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(MadStar),
            player => new MadStar(player),
            CustomRoles.MadStar,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Madmate,
            21500,
            null,
            "mdst",
            "#ffff00",
            (1, 2),
            introSound: () => GetIntroSound(RoleTypes.Impostor),
            from: From.TownOfHost_PP
        );
    public MadStar(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    { }
}
