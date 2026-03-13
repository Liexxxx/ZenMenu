using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZenMenu.Utillities.MotherShip.V1
{
    internal class Data_
    {
        public static MothershipAuthenticator Auth = GameObject.Find("Networking Scripts/PlayFabAuthenticator").GetComponent<MothershipAuthenticator>();
    }
}
