using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ZenMenu.Utillities.MotherShip.V2
{
    internal class ReportDispatching
    {
        [DllImport("MothershipApi", EntryPoint = "CSharp_CreateReportRequest_ToHttpRequest")]
        private static extern IntPtr CreateReportRequest_ToHttpRequest(HandleRef jarg1);
        public static void DispatchReportRequest(string userID)
        {
            string platform = "Steam";
            try
            {
                var rig = ZenMenu.patches.VrrigCache.Data.vrrigs.FirstOrDefault(x => x.OwningNetPlayer.UserId == userID);
                if (rig != null) platform = rig.IsItemAllowed("FIRST LOGIN") ? "Steam" : "Oculus";
            }
            catch { }

            using (CreateReportRequest reportRequest = new CreateReportRequest { reported_user_id = userID, category = 0, platform = platform, modded_client = true, metadata = "{}" })
            {
                SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t sharedPtr = reportRequest.ToHttpRequest();
                HandleRef sharedPtrHandle = (HandleRef)typeof(SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t).GetField("swigCPtr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sharedPtr);
                MothershipHTTPRequest httpRequest = (MothershipHTTPRequest)Activator.CreateInstance(typeof(MothershipHTTPRequest), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { sharedPtrHandle.Handle, true }, null);
                MothershipHttpRunner.instance.SendRequest(null, httpRequest, response => {  });
            }
        }
    }
}