using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZenMenu.mods.Safety;
using ZenMenu.Utillities;
using ZenMenu_.mods.Advantage;
using ZenMenu_.mods.Movement;
using ZenMenu_.mods.Room;
using ZenMenu_.mods.Visual;
using ZenMenu_.mods.VRRig_;

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
                { "RPCProtection", new ButtonModule("RPCProtection","Protection from any rpc lethal to an modder",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.RpcProtection), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.RpcProtection),Catagory_:"Safety") },
                { "Proxy", new ButtonModule("NetProxy","Protection from any tracers that have to do with ip",true,false,EM:()=> SafetyManager.EnableMod(SafetyManager.Mods.Proxy), DM:()=>SafetyManager.EnableMod(SafetyManager.Mods.Proxy),Catagory_:"Safety") },
            }
        },
        {
            "Room",
            new Dictionary<string, ButtonModule>()
            {
                { "Disconnect", new ButtonModule("Disconnect","Leaves the current room",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.Disconnect),Catagory_:"Room") },
                { "Reconnect", new ButtonModule("Reconnect","Rejoins the current room",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.Recconect),Catagory_:"Room") },
                { "JoinRandom", new ButtonModule("JoinRandom","Rejoins the current room",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.JoinRandom),Catagory_:"Room") },
                { "JoinLastLobby", new ButtonModule("JoinLast","Rejoins the last room you were in",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.JoinLastRoom),Catagory_:"Room") },
                { "CreatePublicRoom", new ButtonModule("CreatePublic","Creates a public lobby",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.CreatePublic),Catagory_:"Room") },
                { "CreateLockedRoom", new ButtonModule("CreateLocked","Creates a locked lobby",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.CreateLocked),Catagory_:"Room") },
                { "CreatemoddedRoom", new ButtonModule("CreateModded","Creates a Modded lobby",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.CreateModded),Catagory_:"Room") },
                { "ClearRoomCache", new ButtonModule("ClearRoomCache","Clears all event 200 cache",false,false,Method_:()=> RoomManager.EnableMod(RoomManager.Mods.ClearRoomCache),Catagory_:"Room") },
            }
        },
        {
            "Vrrig",
            new Dictionary<string, ButtonModule>()
            {
                { "TPose", new ButtonModule("Tpose","Makes The local rig go into a TPose",true,false,EM:()=> RigManager.EnableMod(RigManager.Mods.Tpose),DM: ()=>RigManager.EnableMod(RigManager.Mods.Tpose),Catagory_:"Vrrig") },
                { "GhostMonkey", new ButtonModule("GhostMonkey","Makes The local rig detatch form reality",true,false,EM:()=> RigManager.EnableMod(RigManager.Mods.GhostMonkey),DM:()=>RigManager.EnableMod(RigManager.Mods.GhostMonkey),Catagory_:"Vrrig") },
                { "InvisMoneky", new ButtonModule("InvisMonkey","Makes The local rig invisible",true,false,EM:()=> RigManager.EnableMod(RigManager.Mods.InvisMonkey),DM:()=>RigManager.EnableMod(RigManager.Mods.InvisMonkey),Catagory_:"Vrrig") },
                { "GrabRig", new ButtonModule("GrabRig","Makes The local rig go into your hands",true,false,EM:()=> RigManager.EnableMod(RigManager.Mods.GrabRig),DM:()=>RigManager.EnableMod(RigManager.Mods.GrabRig),Catagory_:"Vrrig") },
                { "Helecopter", new ButtonModule("Helecopter","Makes The local rig fly into the sky",true,false,EM:()=> RigManager.EnableMod(RigManager.Mods.Helecopter),DM:()=>RigManager.EnableMod(RigManager.Mods.Helecopter),Catagory_:"Vrrig") },
                { "Bees", new ButtonModule("Bees","Makes The local rig annoy people",true,false,EM:()=> RigManager.EnableMod(RigManager.Mods.Bees),DM:()=>RigManager.EnableMod(RigManager.Mods.Bees),Catagory_:"Vrrig") },
                { "Dih", new ButtonModule("Dih","Makes it so you have a dih",true,false,EM:()=> RigManager.EnableMod(RigManager.Mods.Dih),DM:()=>RigManager.EnableMod(RigManager.Mods.Dih),Catagory_:"Vrrig") },
            }
        },
        {
            "Movement",
            new Dictionary<string, ButtonModule>()
            {
                { "Longarms", new ButtonModule("Longarms","Gives the player extra reach",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Longarms),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Longarms),Catagory_:"Movement") },
                { "Speedboost", new ButtonModule("Speedboost","Gives the player extra speed",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Speedboost),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Speedboost),Catagory_:"Movement") },
                { "Noclip", new ButtonModule("Noclip","Lets the player go thorugh objects",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Noclip),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Noclip),Catagory_:"Movement") },
                { "Platforms", new ButtonModule("Platforms","Lets you jump in the air",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Platforms),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Platforms),Catagory_:"Movement") },
                { "Dash", new ButtonModule("Dash","Lets you get a little boost",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Dash),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Dash),Catagory_:"Movement") },
                { "Pull", new ButtonModule("Pull","Lets you get a little boost",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Pull),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Pull),Catagory_:"Movement") },
                { "Casting", new ButtonModule("Casting","Lets you get a little boost",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Casting),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Casting),Catagory_:"Movement") },
                { "Slidecontrol", new ButtonModule("Slidecontrol","Lets you controll where you go on ice better",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Slidecontrol),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Slidecontrol),Catagory_:"Movement") },
                { "SlipperySurfaces", new ButtonModule("SlipperySurfaces","Makes everything slippery",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.SlipperySurfaces),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.SlipperySurfaces),Catagory_:"Movement") },
                { "Fly", new ButtonModule("Fly","Gives the abillity to fly",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Fly),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Fly),Catagory_:"Movement") },
                { "Slingshotfly", new ButtonModule("Slingshotfly","Gives the abillity to fly",true,false,EM:()=> MovementManager.EnableMod(MovementManager.Mods.Slingshotfly),DM: ()=>MovementManager.EnableMod(MovementManager.Mods.Slingshotfly),Catagory_:"Movement") },
            }
        },
        {
            "Advantage",
            new Dictionary<string, ButtonModule>()
            {
                { "Tagall", new ButtonModule("Tagall","Tags everyone",true,false,EM:()=> AdvantageManager.EnableMod(AdvantageManager.Mods.TagAll),DM: ()=>AdvantageManager.EnableMod(AdvantageManager.Mods.TagAll),Catagory_:"Advantage") },
                { "Taggun", new ButtonModule("Taggun","Tags person of your choice",true,false,EM:()=> AdvantageManager.EnableMod(AdvantageManager.Mods.TagGun),DM: ()=>AdvantageManager.EnableMod(AdvantageManager.Mods.TagGun),Catagory_:"Advantage") },
                { "Tagarua", new ButtonModule("Tagarua","Tags peeople close to you",true,false,EM:()=> AdvantageManager.EnableMod(AdvantageManager.Mods.TagAura),DM: ()=>AdvantageManager.EnableMod(AdvantageManager.Mods.TagAura),Catagory_:"Advantage") },
                { "Autojuke", new ButtonModule("Autojuke","Moves out of dangers way",true,false,EM:()=> AdvantageManager.EnableMod(AdvantageManager.Mods.AutoJuke),DM: ()=>AdvantageManager.EnableMod(AdvantageManager.Mods.AutoJuke),Catagory_:"Advantage") },
                { "Switchprop", new ButtonModule("Switchprop","Changes the porp you currently hidden as",false,false,Method_:()=> AdvantageManager.EnableMod(AdvantageManager.Mods.SwitchProp),Catagory_:"Advantage") },
                { "Flicktag", new ButtonModule("Flicktag","Tags people from afar",true,false,EM:()=> AdvantageManager.EnableMod(AdvantageManager.Mods.FlickTag),DM: ()=>AdvantageManager.EnableMod(AdvantageManager.Mods.FlickTag),Catagory_:"Advantage") },
                { "Antitagfreeze", new ButtonModule("Antifreeze","Ensures Tag freeze does not effect gameplay",true,false,EM:()=> AdvantageManager.EnableMod(AdvantageManager.Mods.Antitagfreeze),DM: ()=>AdvantageManager.EnableMod(AdvantageManager.Mods.Antitagfreeze),Catagory_:"Advantage") },
            }
        },
        {
            "Visuals",
            new Dictionary<string, ButtonModule>()
            {
                { "Chams", new ButtonModule("Chams", "See players through walls", true, false, EM: () => VisualManager.ENableMod(VisualManager.Mods.Chams), DM: () => VisualManager.ENableMod(VisualManager.Mods.Chams), Catagory_: "Visuals") },
                { "Tracers", new ButtonModule("Tracers", "Draw lines to all players", true, false, EM: () => VisualManager.ENableMod(VisualManager.Mods.Tracers), DM: () => VisualManager.ENableMod(VisualManager.Mods.Tracers), Catagory_: "Visuals") },
                { "NameEsp", new ButtonModule("NameEsp", "Show player names", true, false, EM: () => VisualManager.ENableMod(VisualManager.Mods.NameEsp), DM: () => VisualManager.ENableMod(VisualManager.Mods.NameEsp), Catagory_: "Visuals") },
                { "DistanceEsp", new ButtonModule("DistanceEsp", "Show distance to players", true, false, EM: () => VisualManager.ENableMod(VisualManager.Mods.DistanceEsp), DM: () => VisualManager.ENableMod(VisualManager.Mods.DistanceEsp), Catagory_: "Visuals") },
                { "BoxEsp", new ButtonModule("BoxEsp", "Draw boxes around players", true, false, EM: () => VisualManager.ENableMod(VisualManager.Mods.BoxEsp), DM: () => VisualManager.ENableMod(VisualManager.Mods.BoxEsp), Catagory_: "Visuals") },
                { "TrailEsp", new ButtonModule("TrailEsp", "Show player movement trails", true, false, EM: () => VisualManager.ENableMod(VisualManager.Mods.TrailEsp), DM: () => VisualManager.ENableMod(VisualManager.Mods.TrailEsp), Catagory_: "Visuals") },
            }
        },
        };
    }
    public class ButtonCollider : MonoBehaviour
    {
        public ButtonModule Button;

        public bool IsPageButton;
        public bool IsPrevPage;
        public bool IsHome;
        public float Cooldown = 0.15f;
        float lastPress;
        public void OnTriggerEnter(Collider collider)
        {
            if (collider != Main.buttonCollider && collider != Main.buttonCollider2) return;
            if (Time.time - lastPress < Cooldown) return;
            lastPress = Time.time;
            if (IsPageButton)
            {
                Main.ChangePage(IsPrevPage);
                GorillaTagger.Instance.StartVibration(Main.OpenedWithLeft, 5f, 1f);
                return;
            }
            if (IsHome)
            {
                Main.ReturnHome();
                GorillaTagger.Instance.StartVibration(Main.OpenedWithLeft, 5f, 1f);
                return;
            }
            if (Button != null)
                Main.ToggleMod(Button);
            GorillaTagger.Instance.StartVibration(Main.OpenedWithLeft, 5f, 1f);
        }
    }
}
