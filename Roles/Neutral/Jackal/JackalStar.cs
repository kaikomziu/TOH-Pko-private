using AmongUs.GameOptions;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Neutral;

/// <summary>
/// [TOH-P-P] ジャッカルスター
/// スター(<see cref="Crewmate.Star"/>)のジャッカル陣営版。能力なし。
/// ゲーム開始時から全員視点で名前が黄色く見え、ジャッカル陣営が勝つと一緒に勝利する。
/// 人数判定ではクルー側として数える(マッドメイトと同じ扱い)。ジャッカル系役職が有効なときだけ配役される。
/// </summary>
public sealed class JackalStar : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(JackalStar),
            player => new JackalStar(player),
            CustomRoles.JackalStar,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Neutral,
            50000,
            null,
            "jast",
            "#ffff00",
            (1, 6),
            assignInfo: new RoleAssignInfo(CustomRoles.JackalStar, CustomRoleTypes.Neutral)
            {
                // ジャッカル系が存在するときだけ排出する
                IsInitiallyAssignableCallBack = () =>
                    CustomRoles.Jackal.IsEnable()
                    || CustomRoles.JackalAlien.IsEnable()
                    || CustomRoles.JackalMafia.IsEnable()
                    || CustomRoles.JackalWolf.IsEnable()
                    || CustomRoles.JackalHadouHo.IsEnable()
            },
            from: From.TownOfHost_PP
        );
    public JackalStar(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    { }

    /// <summary>ジャッカル陣営の勝利時に、ジャッカルスターも勝者に加える(CheckGameEndPatch から呼ぶ)</summary>
    public static void AddWinnersOnJackalWin()
    {
        if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Jackal) return;
        foreach (var pc in PlayerCatch.AllPlayerControls)
        {
            if (pc.Is(CustomRoles.JackalStar))
                CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
        }
    }
}
