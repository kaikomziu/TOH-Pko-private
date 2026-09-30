using System.Collections.Generic;
using AmongUs.GameOptions;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Impostor;

/// <summary>
/// [TOH-P-P] 足止め屋
/// シェイプシフトボタンで選んだ人を、その場で数秒間動けなくする(変身はしない)。
/// </summary>
public sealed class Binder : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Binder),
            player => new Binder(player),
            CustomRoles.Binder,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            10700,
            SetupOptionItem,
            "bnd",
            OptionSort: (6, 18),
            from: From.TownOfHost_PP
        );
    public Binder(PlayerControl player)
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

    /// <summary>足止め中のプレイヤーと、元の移動速度</summary>
    private static readonly Dictionary<byte, float> frozen = new();

    enum OptionName
    {
        BinderDuration
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 11, GeneralOption.Cooldown, OptionBaseCoolTime, 25f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionDuration = FloatOptionItem.Create(RoleInfo, 12, OptionName.BinderDuration, new(1f, 10f, 0.5f), 3f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public float CalculateKillCooldown() => KillCooldown;
    public override void ApplyGameOptions(IGameOptions opt) => AURoleOptions.ShapeshifterCooldown = Cooldown;
    public override string GetAbilityButtonText() => GetString("BinderButton");

    public override bool CheckShapeshift(PlayerControl target, ref bool animate)
    {
        // 変身はさせず、足止めだけ行う
        animate = false;
        if (!AmongUsClient.Instance.AmHost) return false;
        if (target == null || !target.IsAlive() || target.PlayerId == Player.PlayerId || target.IsTeammate(Player)) return false;
        if (frozen.ContainsKey(target.PlayerId)) return false;

        Freeze(target);
        Player.RpcResetAbilityCooldown();
        return false;
    }
    private static void Freeze(PlayerControl target)
    {
        var original = Main.AllPlayerSpeed.TryGetValue(target.PlayerId, out var s) ? s : Main.NormalOptions.PlayerSpeedMod;
        frozen[target.PlayerId] = original;
        Main.AllPlayerSpeed[target.PlayerId] = Main.MinSpeed;
        target.MarkDirtySettings();
        Logger.Info($"{target.GetNameWithRole().RemoveHtmlTags()} を {Duration}秒 足止め", "Binder");

        _ = new LateTask(() => Unfreeze(target.PlayerId), Duration, $"Binder.Unfreeze.{target.PlayerId}", true);
    }
    private static void Unfreeze(byte playerId)
    {
        if (!frozen.TryGetValue(playerId, out var original)) return;
        frozen.Remove(playerId);
        Main.AllPlayerSpeed[playerId] = original;
        PlayerCatch.GetPlayerById(playerId)?.MarkDirtySettings();
    }
    public override void OnStartMeeting()
    {
        // 会議が始まったら足止めは全部解除する
        foreach (var id in new List<byte>(frozen.Keys))
            Unfreeze(id);
    }
    public override void OnDestroy() => frozen.Clear();
}
