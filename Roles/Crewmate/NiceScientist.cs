using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using TownOfHost.Modules;
using TownOfHost.Patches;
using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Crewmate;

/// <summary>
/// [TOH-P-P] ナイスサイエンティスト(SuperNewRoles より)【試作】
/// 能力ボタン(ファントムの「消える」ボタン)で、好きなタイミングで一定時間透明になれるクルー。
/// ボタンは本人の画面にだけファントムとして見せる方式(ナイスロガーと同じ)。他の人への透明化は、部屋主が「消える/現れる」の通信を直接送って見せる。
/// MOD 未導入の端末で本当に消えて見えるかは要テスト。見えない場合は設定「ファントム役職を一時的に送る」をオンにして試す。
/// </summary>
public sealed class NiceScientist : RoleBase, IKiller, IUsePhantomButton
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(NiceScientist),
            player => new NiceScientist(player),
            CustomRoles.NiceScientist,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Crewmate,
            40300,
            SetupOptionItem,
            "nsc",
            "#7fe0d0",
            (7, 7),
            true,
            from: From.SuperNewRoles
        );
    public NiceScientist(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Cooldown = OptionCooldown.GetFloat();
        Duration = OptionDuration.GetFloat();
        SendPhantomRole = OptionSendPhantomRole.GetBool();
        lastUseTime = -999f;
        isInvisible = false;
    }
    private static OptionItem OptionCooldown;
    private static OptionItem OptionDuration;
    private static OptionItem OptionSendPhantomRole;
    private static float Cooldown;
    private static float Duration;
    private static bool SendPhantomRole;

    float lastUseTime;
    bool isInvisible;
    int invisibleSerial;

    enum OptionName
    {
        EvilScientistDuration,
        NiceScientistSendPhantomRole
    }
    private static void SetupOptionItem()
    {
        OptionCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.Cooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionDuration = FloatOptionItem.Create(RoleInfo, 11, OptionName.EvilScientistDuration, new(2.5f, 60f, 2.5f), 10f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSendPhantomRole = BooleanOptionItem.Create(RoleInfo, 12, OptionName.NiceScientistSendPhantomRole, false, false);
    }

    // キル・ベント・サボタージュはできない(ボタンは透明化専用)
    public bool CanUseKillButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public bool CanUseSabotageButton() => false;
    public float CalculateKillCooldown() => 0f;
    public override void ApplyGameOptions(IGameOptions opt)
    {
        opt.SetVision(false);
        AURoleOptions.PhantomCooldown = Cooldown;
    }
    bool IUsePhantomButton.IsresetAfterKill => false;

    public void OnClick(ref bool AdjustKillCooldown, ref bool? ResetCooldown)
    {
        AdjustKillCooldown = false;
        ResetCooldown = false;
        if (!AmongUsClient.Instance.AmHost) return;
        if (!Player.IsAlive() || !GameStates.IsInTask || GameStates.IsMeeting) return;
        if (isInvisible) return;
        if (Time.time - lastUseTime < Cooldown - 0.5f) return;
        ResetCooldown = true;

        lastUseTime = Time.time;
        SetInvisible(true);
        var serial = ++invisibleSerial;
        _ = new LateTask(() =>
        {
            if (serial != invisibleSerial) return;
            SetInvisible(false);
        }, Duration, "NiceScientist.Appear", true);
    }

    /// <summary>自分以外の全員に「消える/現れる」を送る</summary>
    private void SetInvisible(bool invisible)
    {
        if (isInvisible == invisible) return;
        isInvisible = invisible;
        Logger.Info($"{Player.GetNameWithRole().RemoveHtmlTags()} {(invisible ? "透明化" : "透明解除")}", "NiceScientist");

        foreach (var pc in PlayerCatch.AllPlayerControls)
        {
            if (pc == null || pc.PlayerId == Player.PlayerId) continue;

            // 部屋主の画面はその場で処理する
            if (pc.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                try
                {
                    if (invisible) Player.HandleServerVanish();
                    else Player.HandleServerAppear(true);
                }
                catch (System.Exception ex)
                {
                    Logger.Warn($"部屋主画面での透明処理に失敗: {ex.Message}", "NiceScientist");
                }
                continue;
            }

            var clientId = pc.GetClientId();
            if (clientId == -1) continue;
            var sender = CustomRpcSender.Create("NiceScientistInvisible", SendOption.Reliable);
            sender.StartMessage(clientId);
            if (invisible)
            {
                if (SendPhantomRole)
                {
                    sender.StartRpc(Player.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.Phantom)
                        .Write(true)
                        .EndRpc();
                }
                sender.StartRpc(Player.NetId, RpcCalls.StartVanish)
                    .EndRpc();
            }
            else
            {
                sender.StartRpc(Player.NetId, RpcCalls.StartAppear)
                    .Write(true)
                    .EndRpc();
                if (SendPhantomRole)
                {
                    sender.StartRpc(Player.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.Crewmate)
                        .Write(true)
                        .EndRpc();
                }
            }
            sender.EndMessage();
            sender.SendMessage();
        }
    }

    public override void OnStartMeeting()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        invisibleSerial++;
        SetInvisible(false);
    }
    public override void OnDead(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !Is(player)) return;
        invisibleSerial++;
        SetInvisible(false);
    }
}
