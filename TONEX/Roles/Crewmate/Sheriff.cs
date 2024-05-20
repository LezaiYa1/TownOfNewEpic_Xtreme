using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Roles.Neutral;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX.Roles.Crewmate;
public sealed class Sheriff : RoleBase, IKiller, ISchrodingerCatOwner
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Sheriff),
            player => new Sheriff(player),
            CustomRoles.Sheriff,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Crewmate,
            20700,
            SetupOptionItem,
            "sh|警長",
            "#f8cd46",
            true
        );
    public Sheriff(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        ShotLimit = ShotLimitOpt.GetInt();
        CurrentKillCooldown = KillCooldown.GetFloat();
    }

    private static OptionItem KillCooldown;
    private static OptionItem MisfireKillsTarget;
    private static OptionItem ShotLimitOpt;
    private static OptionItem CanKillAllAlive;
    private static OptionItem CanKillNeutrals;
    public static OptionItem CanKillNeutralsMode;
    private static OptionItem CanKillMadmate;
    private static OptionItem CanKillCharmed;
    private static OptionItem CanKillWolfMate;
    private static OptionItem SetMadCanKill;
    private static OptionItem MadCanKillCrew;
    private static OptionItem MadCanKillImp;
    private static OptionItem MadCanKillNeutral;
    public static OptionItem HasDeputy;
    public static OptionItem DeputySkillLimit;
    public static OptionItem DeputySkillCooldown;
    public static OptionItem DeputyCanBecomeSheriff;
    public static OptionItem SheriffKnowDeputy;
    public static OptionItem DeputyKnowSheriff;
    enum OptionName
    {
        SheriffMisfireKillsTarget,
        SheriffShotLimit,
        SheriffCanKillAllAlive,
        SheriffCanKillNeutrals,
        SheriffCanKillNeutralsMode,
        SheriffCanKill,
        SheriffCanKillMadmate,
        SheriffCanKillCharmed,
        SheriffCanKillWolfMate,
        SheriffSetMadCanKill,
        SheriffMadCanKillImp,
        SheriffMadCanKillNeutral,
        SheriffMadCanKillCrew,
        HasDeputy,
        DeputySkillLimit,
        DeputySkillCooldown,
        DeputyCanBecomeSheriff,
        SheriffKnowDeputy,
        DeputyKnowSheriff
    }
    public static Dictionary<CustomRoles, OptionItem> KillTargetOptions = new();
    public static Dictionary<SchrodingerCat.TeamType, OptionItem> SchrodingerCatKillTargetOptions = new();
    public int ShotLimit = 0;
    public float CurrentKillCooldown = 30;
    public static readonly string[] KillOption =
    {
            "SheriffCanKillAll", "SheriffCanKillSeparately"
    };
    public SchrodingerCat.TeamType SchrodingerCatChangeTo => SchrodingerCat.TeamType.Crew;

    private static void SetupOptionItem()
    {
        KillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        MisfireKillsTarget = BooleanOptionItem.Create(RoleInfo, 11, OptionName.SheriffMisfireKillsTarget, false, false);
        ShotLimitOpt = IntegerOptionItem.Create(RoleInfo, 12, OptionName.SheriffShotLimit, new(1, 15, 1), 5, false)
            .SetValueFormat(OptionFormat.Times);
        CanKillAllAlive = BooleanOptionItem.Create(RoleInfo, 15, OptionName.SheriffCanKillAllAlive, true, false);
        CanKillMadmate = BooleanOptionItem.Create(RoleInfo, 16, OptionName.SheriffCanKillMadmate, true, false);
        CanKillCharmed = BooleanOptionItem.Create(RoleInfo, 17, OptionName.SheriffCanKillCharmed, true, false);
        CanKillWolfMate = BooleanOptionItem.Create(RoleInfo, 18, OptionName.SheriffCanKillWolfMate, true, false);
        CanKillNeutrals = BooleanOptionItem.Create(RoleInfo, 19, OptionName.SheriffCanKillNeutrals, true, false);
        CanKillNeutralsMode = BooleanOptionItem.Create(RoleInfo, 20, OptionName.SheriffCanKillNeutralsMode,false, false, CanKillNeutrals);
        
        SetUpNeutralOptions(40);
        SetMadCanKill = BooleanOptionItem.Create(RoleInfo, 21, OptionName.SheriffSetMadCanKill, false, false);
        MadCanKillImp = BooleanOptionItem.Create(RoleInfo, 22, OptionName.SheriffMadCanKillImp, true, false).SetParent(SetMadCanKill);
        MadCanKillNeutral = BooleanOptionItem.Create(RoleInfo, 23, OptionName.SheriffMadCanKillNeutral, true, false).SetParent(SetMadCanKill);
        MadCanKillCrew = BooleanOptionItem.Create(RoleInfo, 24, OptionName.SheriffMadCanKillCrew, true, false).SetParent(SetMadCanKill);
        HasDeputy = BooleanOptionItem.Create(RoleInfo, 25, OptionName.HasDeputy, true, false);
        DeputySkillLimit = IntegerOptionItem.Create(RoleInfo, 26, OptionName.DeputySkillLimit, new(1, 999, 1), 5, false,HasDeputy).SetValueFormat(OptionFormat.Times);
        DeputySkillCooldown = FloatOptionItem.Create(RoleInfo, 27, OptionName.DeputySkillCooldown, new(2.5f, 180f, 2.5f), 15f, false, HasDeputy).SetValueFormat(OptionFormat.Seconds);
        DeputyCanBecomeSheriff = BooleanOptionItem.Create(RoleInfo, 28, OptionName.DeputyCanBecomeSheriff, true, false, HasDeputy);
        SheriffKnowDeputy = BooleanOptionItem.Create(RoleInfo, 29, OptionName.SheriffKnowDeputy, true, false, HasDeputy);
        DeputyKnowSheriff = BooleanOptionItem.Create(RoleInfo, 30, OptionName.DeputyKnowSheriff, true, false, HasDeputy);
    }
    public static void SetUpNeutralOptions(int idOffset)
    {

        foreach (var neutral in CustomRolesHelper.AllStandardRoles.Where(x => x.IsNeutral() && !x.IsTODO()).ToArray())
        {
            if (neutral is CustomRoles.SchrodingerCat or CustomRoles.HotPotato or CustomRoles.ColdPotato or CustomRoles.Non_Villain or CustomRoles.GodOfPlagues or CustomRoles.Phantom) continue;
            if (neutral.IsTODO()) continue;
            SetUpKillTargetOption(neutral, idOffset, true, CanKillNeutralsMode);
            idOffset++;
        }
        foreach (var catType in EnumHelper.GetAllValues<SchrodingerCat.TeamType>())
        {
            if ((byte)catType < 50 || catType == SchrodingerCat.TeamType.NightWolf)
            {
                continue;
            }
            SetUpSchrodingerCatKillTargetOption(catType, idOffset, true, CanKillNeutralsMode);
            idOffset++;
        }
    }
    public static void SetUpKillTargetOption(CustomRoles role, int idOffset, bool defaultValue = true, OptionItem parent = null)
    {
        var id = RoleInfo.ConfigId + idOffset;
        parent ??= RoleInfo.RoleOption;
        var roleName = Utils.GetRoleName(role);
        Dictionary<string, string> replacementDic = new() { { "%role%", Utils.ColorString(Utils.GetRoleColor(role), roleName) } };
        KillTargetOptions[role] = BooleanOptionItem.Create(id, OptionName.SheriffCanKill + "%role%", defaultValue, RoleInfo.Tab, false).SetParent(parent);
        KillTargetOptions[role].ReplacementDictionary = replacementDic;
    }
    public static void SetUpSchrodingerCatKillTargetOption(SchrodingerCat.TeamType catType, int idOffset, bool defaultValue = true, OptionItem parent = null)
    {
        var id = RoleInfo.ConfigId + idOffset;
        parent ??= RoleInfo.RoleOption;
        // (%team%陣営)
        var inTeam = GetString("In%team%", new Dictionary<string, string>() { ["%team%"] = GetRoleString(catType.ToString()) });
        // シュレディンガーの猫(%team%陣営)
        var catInTeam = Utils.ColorString(SchrodingerCat.GetCatColor(catType), Utils.GetRoleName(CustomRoles.SchrodingerCat) + inTeam);
        Dictionary<string, string> replacementDic = new() { ["%role%"] = catInTeam };
        SchrodingerCatKillTargetOptions[catType] = BooleanOptionItem.Create(id, OptionName.SheriffCanKill + "%role%", defaultValue, RoleInfo.Tab, false).SetParent(parent);
        SchrodingerCatKillTargetOptions[catType].ReplacementDictionary = replacementDic;
    }
    public override void Add()
    {
        var playerId = Player.PlayerId;
        CurrentKillCooldown = KillCooldown.GetFloat();

        ShotLimit = ShotLimitOpt.GetInt();
        Logger.Info($"{Utils.GetPlayerById(playerId)?.GetNameWithRole()} : 残り{ShotLimit}発", "Sheriff");
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(ShotLimit);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        

        ShotLimit = reader.ReadInt32();
    }
    public float CalculateKillCooldown() => CanUseKillButton() ? CurrentKillCooldown : 255f;
    public bool CanUseKillButton()
        => Player.IsAlive()
        && (CanKillAllAlive.GetBool() || GameStates.AlreadyDied)
        && ShotLimit > 0;
    public bool CanUseImpostorVentButton() => false;
    public bool CanUseSabotageButton() => false;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        if (Is(info.AttemptKiller) && !info.IsSuicide)
        {
            (var killer, var target) = info.AttemptTuple;

            Logger.Info($"{killer.GetNameWithRole()} : 残り{ShotLimit}発", "Sheriff");
            if (ShotLimit <= 0) return false;
            ShotLimit--;
            SendRPC();
            if (!killer.Is(CustomRoles.Madmate) ?
                CanBeKilledBy(target) :
                (!SetMadCanKill.GetBool() ||
                (target.GetCustomRole().IsCrewmate() && MadCanKillCrew.GetBool()) ||
                (target.GetCustomRole().IsNeutral() && MadCanKillNeutral.GetBool()) ||
                (target.GetCustomRole().IsImpostor() && MadCanKillImp.GetBool())
                ))
            {
                killer.ResetKillCooldown();
                return true;
            }
            killer.RpcMurderPlayer(killer);
            PlayerState.GetByPlayerId(killer.PlayerId).DeathReason = CustomDeathReason.Misfire;
            if (!MisfireKillsTarget.GetBool()) return false;
        }
        return true;
    }
    public override string GetProgressText(bool comms = false) => Utils.ColorString(CanUseKillButton() ? Color.yellow : Color.gray, $"({ShotLimit})");
    public static bool CanBeKilledBy(PlayerControl player)
    {
        var cRole = player.GetCustomRole();

        if (player.GetRoleClass() is SchrodingerCat schrodingerCat)
        {
            if (schrodingerCat.Team == SchrodingerCat.TeamType.None)
            {
                Logger.Warn($"シェリフ({player.GetRealName()})にキルされたシュレディンガーの猫のロールが変化していません", nameof(Sheriff));
                return false;
            }
            return schrodingerCat.Team switch
            {
                SchrodingerCat.TeamType.Mad => KillTargetOptions.TryGetValue(CustomRoles.Madmate, out var option) && option.GetBool(),
                SchrodingerCat.TeamType.Crew => false,
                _ => CanKillNeutrals.GetValue() == 0 || (SchrodingerCatKillTargetOptions.TryGetValue(schrodingerCat.Team, out var option) && option.GetBool()),
            };
        }

        var subRole = player.GetCustomSubRoles();
        bool CanKill = false;
        foreach (var role in subRole)
        {
            if (role == CustomRoles.Madmate)
                CanKill = CanKillMadmate.GetBool();
            if (role == CustomRoles.Charmed)
                CanKill = CanKillCharmed.GetBool();
        }

        return cRole.GetCustomRoleTypes() switch
        {
            CustomRoleTypes.Impostor => true,
            CustomRoleTypes.Neutral => CanKillNeutrals.GetBool() && (CanKillNeutralsMode.GetValue() == 0 || (!KillTargetOptions.TryGetValue(cRole, out var option) || option.GetBool())),
            _ => CanKill,
        };
    }
    public override string GetMark(PlayerControl seer, PlayerControl seen, bool _ = false)
    {
        //seenが省略の場合seer
        seen ??= seer;
        if (seen.Is(CustomRoles.Deputy) && SheriffKnowDeputy.GetBool()) return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Sheriff), "△");
        else
            return "";
    }
}