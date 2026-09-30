using AmongUs.GameOptions;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Impostor;

/// <summary>
/// [TOH-P-P] イビルサイエンティスト(SuperNewRoles より)
/// 好きなタイミングで一定時間透明になれるインポスター。
/// Among Us 本来のファントムの「消える」をそのまま使う(RoleAbilityPatch の CheckVanish で素通しにしている)。
/// </summary>
public sealed class EvilScientist : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(EvilScientist),
            player => new EvilScientist(player),
            CustomRoles.EvilScientist,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            11300,
            SetupOptionItem,
            "esc",
            OptionSort: (6, 19),
            from: From.SuperNewRoles
        );
    public EvilScientist(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        Cooldown = OptionCooldown.GetFloat();
        Duration = OptionDuration.GetFloat();
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionCooldown;
    private static OptionItem OptionDuration;
    private static float KillCooldown;
    private static float Cooldown;
    private static float Duration;

    enum OptionName
    {
        EvilScientistDuration
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 11, GeneralOption.Cooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionDuration = FloatOptionItem.Create(RoleInfo, 12, OptionName.EvilScientistDuration, new(2.5f, 60f, 2.5f), 10f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public float CalculateKillCooldown() => KillCooldown;
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.PhantomCooldown = Cooldown;
        AURoleOptions.PhantomDuration = Duration;
    }
}
