using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenMenu.Utillities
{
    public class ButtonModule
    {
        public string ModName;
        public string ToolTip;
        public bool Toggable;
        public bool Enabled;
        public Action EnableMethod;
        public Action DisableMethod;
        public Action Method;
        public string Catagory;
        public ButtonModule(string Modname,string Tooltip,bool Toggable_,bool Enabled_,Action EM = null,Action DM = null,Action Method_ = null,string Catagory_ = "")
        {
            ModName = Modname;
            ToolTip = Tooltip;
            Toggable = Toggable_;
            Enabled = Enabled_;
            EnableMethod = EM;
            DisableMethod = DM;
            Method = Method_;
            Catagory = Catagory_;
        }
    }
}
