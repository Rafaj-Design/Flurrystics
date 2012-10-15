﻿using System;
using System.Net;
using System.Windows;
using System.Net.NetworkInformation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Flurrystics
{
    public static class Util
    {

        public static long getCurrentTimestamp()
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            DateTime currentDate = DateTime.Now;
            long totalMiliSecond = (currentDate.Ticks - unixEpoch.Ticks) / 10000;
            return totalMiliSecond;
        }

        public static bool InternetIsAvailable()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Internet connection not available. Please try again later.");
                return false;
            }
            return true;
        }

    }
}