﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;

namespace Flurrystics
{
    public partial class MainPage : PhoneApplicationPage
    {

        string apiKey;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

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

            // Set the data context of the listbox control to the sample data
            //DataContext = App.ViewModel;
            //this.Loaded += new RoutedEventHandler(MainPage_Loaded)
            var w = new WebClient();
            Observable
            .FromEvent<DownloadStringCompletedEventArgs>(w, "DownloadStringCompleted")
            .Subscribe(r =>
            {
                XDocument loadedData = null;
                try
                {
                    try
                    {
                        loadedData = XDocument.Parse(r.EventArgs.Result);
                    }
                    catch (WebException) // load failed, probably wrong apiKey - goto settings
                    {
                        NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
                    }

                    if (loadedData != null)
                    {

                        //XDocument loadedData = XDocument.Load("getAllApplications.xml");
                        PageTitle.Text = (string)loadedData.Root.Attribute("companyName");
                        var data = from query in loadedData.Descendants("application")
                                   select new AppViewModel
                                   {
                                       LineOne = (string)query.Attribute("name"),
                                       LineTwo = (string)query.Attribute("platform") + ", created: " + (string)query.Attribute("createdDate"),
                                       LineFour = (string)query.Attribute("apiKey")
                                   };
                        progressBar1.Visibility = System.Windows.Visibility.Collapsed;
                        progressBar1.IsIndeterminate = false; // switch off so it doesn't hit performance when not visible (!)
                        MainListBox.ItemsSource = data;
                    }
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("Flurry API overload, please try again later."); // should not happen - EVER
                }

            });

            if (Util.InternetIsAvailable()) // if Internet is available - go download and process our feed
            {
                w.Headers[HttpRequestHeader.Accept] = "application/xml"; // get us XMLs version!
                w.DownloadStringAsync(
                    new Uri("http://api.flurry.com/appInfo/getAllApplications?apiAccessCode=" + apiKey)
                    );

            }
        }

        // Handle selection changed on ListBox
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (MainListBox.SelectedIndex == -1)
                return;

            // Navigate to the new page
            AppViewModel selected = (AppViewModel)MainListBox.Items[MainListBox.SelectedIndex];
            NavigationService.Navigate(new Uri("/AppMetrics.xaml?apikey=" + selected.LineFour + " &appName=" + selected.LineOne, UriKind.Relative));
                
                // .SelectedIndex, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            MainListBox.SelectedIndex = -1;
        }

        private void SettingsOption_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            //Do work for your application here.
        }
        

        // Load data for the ViewModel Items
        /*
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }*/

    }
}