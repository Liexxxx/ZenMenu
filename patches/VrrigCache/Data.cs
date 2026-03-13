using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZenMenu.patches.VrrigCache
{
    internal class Data
    {
        public static List<VRRig> vrrigs = new List<VRRig>();
        public static List<VRRig> Modertors =
        vrrigs.Where(x => x != null).Where(x => x.cosmeticSet != null && x.cosmeticSet.items != null)
          .Where(x => !x.cosmeticSet.items.Any(i =>
                 !i.isNullItem &&
                 (i.itemName.Contains("LBAAK") || i.itemName.Contains("LBADE"))))
          .ToList();
    }
}
