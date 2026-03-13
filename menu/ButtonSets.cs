using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
                { "Safety", new ButtonModule("Place Holder","Place Holder",false,false,Method_:null,Catagory_:"Safety") },
                { "Room", new ButtonModule("Place Holder","Place Holder",false,false,Method_:null,Catagory_:"Safety") },
                { "Vrrig", new ButtonModule("Place Holder","Place Holder",false,false,Method_ : null, Catagory_:"Safety") },
                { "Movement", new ButtonModule("Place Holder","Place Holder",false,false,Method_ : null, Catagory_:"Safety") },
                { "Advantage", new ButtonModule("Place Holder","Place Holder",false,false,Method_ : null, Catagory_:"Safety") },
                { "Visuals", new ButtonModule("Place Holder","Place Holder",false,false,Method_ : null, Catagory_:"Safety") }
            }
        },
        {
            "Safety",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Safety") },
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Safety") },
            }
        },
        {
            "Room",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Room") },
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Room") },
            }
        },
        {
            "Vrrig",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Vrrig") },
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Vrrig") },
            }
        },
        {
            "Movement",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Movement") },
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Movement") },
            }
        },
        {
            "Advantage",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Advantage") },
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Advantage") },
            }
        },
        {
            "Visuals",
            new Dictionary<string, ButtonModule>()
            {
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Visuals") },
                { "PlaceHolder", new ButtonModule("Place Holder","Place Holder",false,false,Catagory_:"Visuals") },
            }
        },
            };
        }
    public class ButtonCollider : MonoBehaviour
    {
        public ButtonModule Button;
        public void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.name.Contains("ModButton"))
            {
                Main.ToggleMod(Button);
            }
            else if (collider.gameObject.name.Contains("Page"))
            {
                Main.ChangePage(collider.gameObject.name.Contains("Prev") ? true : false);
            }
        }
    }
}
