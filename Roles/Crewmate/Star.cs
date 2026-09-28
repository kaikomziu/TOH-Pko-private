using AmongUs.GameOptions;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] スター(SuperNewRoles より)
/// 能力なしのクルーメイト。ゲーム開始時から全員視点で名前が黄色く見える。
/// </summary>
public sealed class Star : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Star),
            player => new Star(player),
            CustomRoles.Star,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            35100,
            null,
            "star",
            "#ffff00",
            (4, 4),
            from: From.SuperNewRoles
        );
    public Star(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    /// <summary>全員視点でスター系(スター・マッドスター・ジャッカルスター)の名前を役職色(黄色)で表示する。NameColorManager から呼ばれる</summary>
    public static bool KnowTargetRoleColor(PlayerControl target)
        => target.Is(CustomRoles.Star) || target.Is(CustomRoles.MadStar) || target.Is(CustomRoles.JackalStar);
}
