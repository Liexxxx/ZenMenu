using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZenMenu.mods.Safety;
using ZenMenu.Utillities;

namespace ZenMenu.Menu
{
    internal class ButtonSets
    {
        public static Dictionary<string, Dictionary<string, ButtonModule>> buttonSets_ = new Dictionary<string, Dictionary<string, ButtonModule>>()
        {
        {
            "Main",
            new Dictionary<string, ButtonModule>()
            {
                { "Safety", new ButtonModule("Safety","Place Holder",false,false,Method_:()=>Main.ChangeCataory("Safety"),Catagory_:"Main") },
                { "Room", new ButtonModule("Room","Place Holder",false,false,Method_:()=>Main.ChangeCataory("Room"),Catagory_:"Main") },
                { "Vrrig", new ButtonModule("Vrrig","Place Holder",false,false,Method_ :()=>Main.ChangeCataory("Vrrig"), Catagory_:"Main") },
                { "Movement", new ButtonModule("Movement","Place Holder",false,false,Method_ :()=>Main.ChangeCataory("Movement"), Catagory_:"Main") },
                { "Advantage", new ButtonModule("Advantage","Place Holder",false,false,Method_ : ()=>Main.ChangeCataory("Advantage"), Catagory_:"Main") },
                { "Visuals", new ButtonModule("Visuals","Place Holder",false,false,Method_ : ()=>Main.ChangeCataory("Visuals"), Catagory_:"Main") }
            }
        },
        {
            "Safety",
            new Dictionary<string, ButtonModule>()
            {
                { "AntiBan(UserC)", new ButtonModule("AntiBan(UserC)","Prevents You From Getting Banned",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.AnitBan_Mock), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.AnitBan_Mock),Catagory_:"Safety") },
                { "AntiBan(AA)", new ButtonModule("AntiBan(UserAA)","Prevents You From Getting Banned",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.AntiBan_MotherShip), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.AntiBan_MotherShip),Catagory_:"Safety") },
                { "AntiReport", new ButtonModule("AntiReport","Prevents You From Getting Reported",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.AntiReport), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.AntiReport),Catagory_:"Safety") },
                { "AntiModerator", new ButtonModule("AntiModerator","Prevents You From Being In a Lobby with an moderator",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.AntiModerator), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.AntiModerator),Catagory_:"Safety") },
                { "PlayerSpoof", new ButtonModule("PlayerSpoof","Spoofs The Player Credentials",false,false,Method_:()=> SafetyManager.EnableMod(SafetyManager.Mods.PlayerSpoof),Catagory_:"Safety") },
                { "PhotonSpoof", new ButtonModule("PhotonSpoof","Spoofs The Player Credentials (NETWORKED | CS)",false,false,Method_:()=> SafetyManager.EnableMod(SafetyManager.Mods.PhotonSpoof),Catagory_:"Safety") },
                { "AntiCrash", new ButtonModule("AnitCrash","Prevents You From Being Crashed",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.AntiCrash), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.AntiCrash),Catagory_:"Safety") },
                { "FlushPlayerCredentials", new ButtonModule("FlushInfo","Flushes Player Credentials",false,false,Method_:()=> SafetyManager.EnableMod(SafetyManager.Mods.FlushPlayerCredentials),Catagory_:"Safety") },
                { "FlushCache", new ButtonModule("FlushCache","Flushes The Local Cache",false,false,Method_:()=> SafetyManager.EnableMod(SafetyManager.Mods.FlushPlayerCredentials),Catagory_:"Safety") },
                { "FlushRPCS", new ButtonModule("FlushRPCS","Flushes The Local RPC Traces",false,false,Method_:()=> SafetyManager.EnableMod(SafetyManager.Mods.FlushRPCS),Catagory_:"Safety") },
                { "ClearCacheOnGameQuit", new ButtonModule("RemoveQuitTraces","Clears Cache | Traces (RPC) on game quitting",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.AntiCrash), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.AntiCrash),Catagory_:"Safety") },
            }
        },
        {
            "Room",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder1", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Room") },
                { "PlaceHolder2", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Room") },
            }
        },
        {
            "Vrrig",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder1", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Vrrig") },
                { "PlaceHolder2", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Vrrig") },
            }
        },
        {
            "Movement",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder1", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Movement") },
                { "PlaceHolder2", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Movement") },
            }
        },
        {
            "Advantage",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder1", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Advantage") },
                { "PlaceHolder2", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Advantage") },
            }
        },
        {
            "Visuals",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder1", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Visuals") },
                { "PlaceHolder2", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Visuals") },
            }
        },
        };
    }
    public class ButtonCollider : MonoBehaviour
    {
        public ButtonModule Button;

        public bool IsPageButton;
        public bool IsPrevPage;
        float lastPress;
        public void OnTriggerEnter(Collider collider)
        {
            if (collider != Main.buttonCollider) return;
            if (IsPageButton)
            {
                Main.ChangePage(IsPrevPage);
                return;
            }
            if (Button != null)
                Main.ToggleMod(Button);
        }
    }
}
