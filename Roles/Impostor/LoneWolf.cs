using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Impostor;

/// <summary>
/// [TOH-P-P] 孤狼
/// 近くに誰もいないときはキルクールが速く進み、近くに2人以上いるとキルクールが止まる。1人だけなら通常。
/// ホスト側で残りキルクールを計算し、ずれが大きくなったら本人のキルクールを合わせ直す。
/// </summary>
public sealed class LoneWolf : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(LoneWolf),
            player => new LoneWolf(player),
            CustomRoles.LoneWolf,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            11600,
            SetupOptionItem,
            "lwf",
            OptionSort: (7, 14),
            from: From.TownOfHost_PP
        );
    public LoneWolf(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        AloneSpeed = OptionAloneSpeed.GetFloat();
        Radius = OptionRadius.GetFloat();

        remaining = -1f;
        vanillaStart = -1f;
        vanillaLength = 0f;
        lastSync = 0f;
        state = State.Normal;
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionAloneSpeed;
    private static OptionItem OptionRadius;
    private static float KillCooldown;
    private static float AloneSpeed;
    private static float Radius;

    enum State : byte { Normal, Alone, Crowd }
    State state;

    /// <summary>孤狼ルールで計算した残りキルクール(負なら未計測)</summary>
    float remaining;
    // 本人の画面で実際に動いているキルクール(最後に合わせた時刻と長さ)
    float vanillaStart;
    float vanillaLength;
    float lastSync;

    // ゲーム開始直後のキルクール(バニラ仕様)
    const float FirstKillCooldown = 10f;
    // 計算と実際のずれがこれ以上になったら合わせ直す
    const float SyncThreshold = 2f;
    // 合わせ直しの最短間隔(合わせるたびに本人の画面に盾エフェクトが出るため)
    const float SyncInterval = 2f;

    enum OptionName
    {
        LoneWolfAloneSpeed,
        LoneWolfRadius
    }
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, OptionBaseCoolTime, 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionAloneSpeed = FloatOptionItem.Create(RoleInfo, 11, OptionName.LoneWolfAloneSpeed, new(1.5f, 5f, 0.5f), 2f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionRadius = FloatOptionItem.Create(RoleInfo, 12, OptionName.LoneWolfRadius, new(1f, 6f, 0.5f), 3f, false);
    }
    public float CalculateKillCooldown() => KillCooldown;

    void Restart(float length)
    {
        remaining = length;
        vanillaStart = Time.time;
        vanillaLength = length;
    }
    float VanillaRemaining => vanillaStart < 0f ? 0f : Mathf.Max(0f, vanillaLength - (Time.time - vanillaStart));

    public override void StartGameTasks() => Restart(FirstKillCooldown);
    public override void AfterMeetingTasks() => Restart(KillCooldown);
    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return;
        Restart(KillCooldown);
    }

    private void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        using var sender = CreateSender();
        sender.Writer.Write((byte)state);
    }
    public override void ReceiveRPC(MessageReader reader) => state = (State)reader.ReadByte();

    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !Is(player)) return;
        if (!Player.IsAlive() || !GameStates.IsInTask || GameStates.IsMeeting || remaining < 0f) return;

        var newState = State.Normal;
        if (!Player.inVent)
        {
            int near = 0;
            Vector2 myPos = Player.transform.position;
            foreach (var pc in PlayerCatch.AllAlivePlayerControls)
            {
                if (pc.PlayerId == Player.PlayerId || pc.inVent) continue;
                if (Vector2.Distance(myPos, pc.transform.position) <= Radius && ++near >= 2) break;
            }
            newState = near == 0 ? State.Alone : near >= 2 ? State.Crowd : State.Normal;
        }
        if (newState != state)
        {
            state = newState;
            SendRPC();
            UtilsNotifyRoles.NotifyRoles(OnlyMeName: true, SpecifySeer: Player);
        }

        if (remaining <= 0f) return;
        var rate = state switch { State.Alone => AloneSpeed, State.Crowd => 0f, _ => 1f };
        remaining = Mathf.Max(0f, remaining - Time.fixedDeltaTime * rate);

        if (Time.time - lastSync < SyncInterval) return;
        // 速く進んだ分は早めに、止まった分はずれが大きくなってから合わせる
        if (Mathf.Abs(VanillaRemaining - remaining) < SyncThreshold && !(remaining <= 0f && VanillaRemaining > 0.5f)) return;

        var cd = Mathf.Max(0.1f, remaining);
        Player.SetKillCooldown(cd);
        vanillaStart = Time.time;
        vanillaLength = cd;
        lastSync = Time.time;
    }

    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        seen ??= seer;
        if (isForMeeting || !Is(seer) || !Is(seen) || !Player.IsAlive()) return "";
        return state switch
        {
            State.Alone => Utils.ColorString(Color.cyan, string.Format(GetString("LoneWolfAlone"), AloneSpeed)),
            State.Crowd => Utils.ColorString(Color.gray, GetString("LoneWolfCrowd")),
            _ => ""
        };
    }
}
