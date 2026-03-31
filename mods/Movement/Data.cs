using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZenMenu_.mods.Movement
{
    internal class Data
    {
        public static Vector3 LongArmsAddition = new Vector3(0.5f, 0.5f, 0.5f);
        public static Vector3 OriginalArmsScale;
        public static float SpeedAddition = 1.0f;
        public static float orignalControll;

        public static bool LongArms;
        public static bool Speed;
        public static bool Slide;

        public static GameObject RightPlat;
        public static GameObject LeftPlat;

        public static bool RightPlaced;
        public static bool LeftPlaced;

        public static bool Dashed;
        public static bool Casted;
        public static bool TouchLeft = false;
        public static bool TouchRight = false;
    }
}
