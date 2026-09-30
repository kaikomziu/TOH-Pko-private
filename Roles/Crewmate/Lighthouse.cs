using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using TownOfHost.Patches;
using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] 灯台守
/// ペットを撫でると数秒間、その瞬間に自分の近くにいた人全員(自分も含む)の視界が広がる。停電中にも効く。
/// </summary>
public sealed class Lighthouse : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Lighthouse),
            player => new Lighthouse(player),
            CustomRoles.Lighthouse,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            39400,
            SetupOptionItem,
            "lhk",
            "#ffd966",
            (7, 6),
            from: From.TownOfHost_PP
        );
    public Lighthouse(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Cooldown = OptionCooldown.GetFloat();
        Duration = OptionDuration.GetFloat();
        Range = OptionRange.GetFloat();
        Multiplier = OptionMultiplier.GetFloat();
        lastUseTime = -999f;
        boosted.Clear();
    }
    private static OptionItem OptionCooldown;
    private static OptionItem OptionDuration;
    private static OptionItem OptionRange;
    private static OptionItem OptionMultiplier;
    private static float Cooldown;
    private static float Duration;
    private static float Range;
    private static float Multiplier;

    float lastUseTime;
    /// <summary>視界ブースト中のプレイヤーと、ブーストが終わる時刻</summary>
    private static readonly Dictionary<byte, float> boosted = new();

    enum OptionName
    {
        LighthouseDuration,
        LighthouseRange,
        LighthouseMultiplier
    }
    private static void SetupOptionItem()
    {
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.Cooldown, new(5f, 120f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionDuration = FloatOptionItem.Create(RoleInfo, 11, OptionName.LighthouseDuration, new(1f, 30f, 1f), 5f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionRange = FloatOptionItem.Create(RoleInfo, 12, OptionName.LighthouseRange, new(1f, 6f, 0.5f), 3f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionMultiplier = FloatOptionItem.Create(RoleInfo, 13, OptionName.LighthouseMultiplier, new(1.25f, 5f, 0.25f), 2f, false)
            .SetValueFormat(OptionFormat.Multiplier);
    }

    public override void Add() => PetActionManager.Register(Player.PlayerId, OnPet);
    public override void OnDestroy()
    {
        PetActionManager.Unregister(Player.PlayerId);
        boosted.Clear();
    }

    private void OnPet()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!Player.IsAlive() || !GameStates.IsInTask || GameStates.IsMeeting) return;
        if (Time.time - lastUseTime < Cooldown) return;
        lastUseTime = Time.time;

        var pos = Player.GetTruePosition();
        var targets = PlayerCatch.AllAlivePlayerControls
            .Where(pc => Is(pc) || Vector2.Distance(pos, pc.GetTruePosition()) <= Range)
            .ToArray();
        var endTime = Time.time + Duration;
        foreach (var pc in targets)
        {
            boosted[pc.PlayerId] = endTime;
            pc.MarkDirtySettings();
        }
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} が灯台を点灯: {targets.Length}人の視界を{Duration}秒広げる", "Lighthouse");
    }

    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !Is(player) || boosted.Count == 0) return;
        foreach (var (id, end) in boosted.ToArray())
        {
            if (Time.time < end) continue;
            boosted.Remove(id);
            PlayerCatch.GetPlayerById(id)?.MarkDirtySettings();
        }
    }
    public override void OnStartMeeting()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var id in boosted.Keys.ToArray())
        {
            boosted.Remove(id);
            PlayerCatch.GetPlayerById(id)?.MarkDirtySettings();
        }
    }

    /// <summary>ブースト中のプレイヤーの視界を広げる(PlayerGameOptionsSender から呼ぶ)</summary>
    public static void ApplyVisionBoost(PlayerControl player, IGameOptions opt)
    {
        if (player == null || !boosted.ContainsKey(player.PlayerId)) return;
        opt.SetFloat(FloatOptionNames.CrewLightMod, Mathf.Min(5f, opt.GetFloat(FloatOptionNames.CrewLightMod) * Multiplier));
        opt.SetFloat(FloatOptionNames.ImpostorLightMod, Mathf.Min(5f, opt.GetFloat(FloatOptionNames.ImpostorLightMod) * Multiplier));
    }
}
