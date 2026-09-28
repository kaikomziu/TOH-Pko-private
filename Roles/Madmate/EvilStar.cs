using AmongUs.GameOptions;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Madmate;

/// <summary>
/// [TOH-P-P] イビルスター
/// スター(<see cref="Crewmate.Star"/>)のインポスター陣営版。能力なし。
/// ゲーム開始時から全員視点で名前が黄色く見え、インポスター陣営が勝つと一緒に勝利する(マッドメイト扱い)。
/// </summary>
public sealed class EvilStar : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(EvilStar),
            player => new EvilStar(player),
            CustomRoles.EvilStar,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Madmate,
            21500,
            null,
            "evst",
            "#ffff00",
            (1, 2),
            introSound: () => GetIntroSound(RoleTypes.Impostor),
            from: From.TownOfHost_PP
        );
    public EvilStar(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    { }
}
