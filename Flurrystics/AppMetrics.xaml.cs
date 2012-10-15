﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using System.Xml.Linq;
using System.ComponentModel;
using System.Threading;
using System.IO.IsolatedStorage;

namespace Flurrystics
{
    public partial class PivotPage1 : PhoneApplicationPage
    {
        string apiKey;
        string appapikey = ""; // initial apikey of the app
        string appName = ""; // appName
        long lastRequest = 0; // timestamp of lastrequest

        public PivotPage1()
        {
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                apiKey = (string)IsolatedStorageSettings.ApplicationSettings["apikey"];
            }
            catch (KeyNotFoundException)
            {
                NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            }
            NavigationContext.QueryString.TryGetValue("apikey", out appapikey);
            NavigationContext.QueryString.TryGetValue("appName", out appName);
            MainPivot.Title = "FLURRYSTICS - " + appName; 
        }

        private void Perform(Action myMethod, int delayInMilliseconds)
        {

            long diff = Util.getCurrentTimestamp() - lastRequest;
            int throttledDelay = 0;

            if (diff < delayInMilliseconds) // if delay between requests is less then second then count time we need to wait before firing up next request
            {
                throttledDelay = (int)diff;
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) => Thread.Sleep(throttledDelay);
            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();
            worker.RunWorkerAsync();        
        
        }

        private void LoadUpXML(string metrics, AmCharts.Windows.QuickCharts.SerialChart targetChart, Microsoft.Phone.Controls.PerformanceProgressBar progressBar)
        {
            lastRequest = Util.getCurrentTimestamp();
            string EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            string StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-31));
            String queryURL = StartDate + " - " + EndDate;

            WebClient w = new WebClient();

                Observable
                .FromEvent<DownloadStringCompletedEventArgs>(w, "DownloadStringCompleted")
                .Subscribe(r =>
                {
                    try
                    {
                        XDocument loadedData = XDocument.Parse(r.EventArgs.Result);
                        //XDocument loadedData = XDocument.Load("getAllApplications.xml");

                    // ListTitle.Text = (string)loadedData.Root.Attribute("metric");
                    var data = from query in loadedData.Descendants("day")
                               select new ChartDataPoint
                               {
                                   Value = (double)query.Attribute("value"),
                                   Label = (string)query.Attribute("date")
                               };

                    progressBar.Visibility = System.Windows.Visibility.Collapsed;
                    progressBar.IsIndeterminate = false;

                    targetChart.DataSource = data;
                    // MainListBox.ItemsSource = data;

                    }
                        catch (NotSupportedException e) // it's not XML - probably API overload
                    {
                        //MessageBox.Show("Flurry API overload, please try again later.");
                    }

                });



            w.Headers[HttpRequestHeader.Accept] = "application/xml"; // get us XMLs version!
            w.DownloadStringAsync(
                new Uri("http://api.flurry.com/appMetrics/"+metrics+"?apiAccessCode="+apiKey+"&apiKey=" + appapikey + "&startDate=" + StartDate + "&endDate=" + EndDate)
                );
        }


        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MainPivot.SelectedIndex)
            {
                case 0:     //ActiveUsers
                    this.Perform(() => LoadUpXML("ActiveUsers", chart1, progressBar1), 1000);
                    break;
                case 1:     //ActiveUsersByWeek
                    this.Perform(() => LoadUpXML("ActiveUsersByWeek", chart2, progressBar2), 1000);
                    break;
                case 2:     //ActiveUsers
                    this.Perform(() => LoadUpXML("ActiveUsersByMonth", chart3, progressBar3), 1000);
                    break;
                case 3:     //ActiveUsersByWeek
                    this.Perform(() => LoadUpXML("NewUsers", chart4, progressBar4), 1000);
                    break;
                case 4:     //ActiveUsers
                    this.Perform(() => LoadUpXML("MedianSessionLength", chart5, progressBar5), 1000);
                    break;
                case 5:     //ActiveUsersByWeek
                    this.Perform(() => LoadUpXML("AvgSessionLength", chart6, progressBar6), 1000);
                    break;
                case 6:     //ActiveUsers
                    this.Perform(() => LoadUpXML("Sessions", chart7, progressBar7), 1000);
                    break;
                case 7:     //ActiveUsersByWeek
                    this.Perform(() => LoadUpXML("RetainedUsers", chart8, progressBar8), 1000);
                    break;

            } // switch
        }

    } // class

   public class ChartDataPoint
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }

}