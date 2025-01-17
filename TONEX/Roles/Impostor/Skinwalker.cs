﻿using AmongUs.GameOptions;
using Hazel;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Impostor;
public sealed class Skinwalker : RoleBase, IImpostor
{

    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Skinwalker),
            player => new Skinwalker(player),
            CustomRoles.Skinwalker,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            199874,
            SetupOptionItem,
            "sh|化形",
           experimental: true
        );
    public Skinwalker(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        TargetSkins = new();
        KillerSkins = new();
        KillerSpeed = new();
        KillerName = "";
        TargetSpeed = new();
        TargetName = "";
    }
    public NetworkedPlayerInfo.PlayerOutfit TargetSkins = new();
    public NetworkedPlayerInfo.PlayerOutfit KillerSkins = new();
    public float KillerSpeed = new();
    public string KillerName = "";
    public float TargetSpeed = new();
    public string TargetName = "";
    static OptionItem KillCooldown;
    private static void SetupOptionItem()
    {
        KillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 35f, false)
             .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        TargetSkins = new();
        KillerSkins = new();
        KillerSpeed = new();
        KillerName = "";
        TargetSpeed = new();
        TargetName = "";
    }
    public float CalculateKillCooldown() => KillCooldown.GetFloat();
    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        KillerSkins = new NetworkedPlayerInfo.PlayerOutfit().Set(killer.GetRealName(), killer.Data.DefaultOutfit.ColorId, killer.Data.DefaultOutfit.HatId, killer.Data.DefaultOutfit.SkinId, killer.Data.DefaultOutfit.VisorId, killer.Data.DefaultOutfit.PetId, killer.Data.DefaultOutfit.NamePlateId);
        TargetSkins = new NetworkedPlayerInfo.PlayerOutfit().Set(target.GetRealName(), target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.PetId, target.Data.DefaultOutfit.NamePlateId);
        
        TargetSpeed = Main.AllPlayerSpeed[target.PlayerId];
        TargetName = Main.AllPlayerNames[target.PlayerId];
        KillerSpeed = Main.AllPlayerSpeed[killer.PlayerId];
        KillerName = Main.AllPlayerNames[killer.PlayerId];

        target.SetOutFit(killer.Data.DefaultOutfit.ColorId);
        var sender = CustomRpcSender.Create(name: $"RpcSetSkin({target.Data.PlayerName})");


        var outfit = TargetSkins;
        var outfit2 = KillerSkins;

        //凶手变样子
        Main.AllPlayerSpeed[killer.PlayerId] = TargetSpeed;
        killer.SetOutFit(outfit.ColorId, outfit.HatId, outfit.SkinId, outfit.VisorId, outfit.PetId);
        Main.AllPlayerNames[killer.PlayerId] = TargetName;
        killer.RpcSetName(TargetName);

        Main.AllPlayerSpeed[target.PlayerId] = KillerSpeed;
        target.SetOutFit(outfit2.ColorId, outfit2.HatId, outfit2.SkinId, outfit2.VisorId, outfit2.PetId);
        Main.AllPlayerNames[target.PlayerId] = KillerName;
        target.RpcSetName(KillerName);

        Utils.NotifyRoles(target);
        Utils.NotifyRoles(killer);
        Utils.NotifyRoles();
    }
}
