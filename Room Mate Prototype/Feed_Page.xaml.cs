using Room_Mate_Prototype.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Room_Mate_Prototype
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Feed_Page : Page
    {
        //list of feed items to manipulate
        private List<Feed_Items> feedData;
        //file name for local storage
        private const string fileName = "feedData";

        //These guys will help me with stuff.... I think....
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        //Default constructor
        public Feed_Page()
        {
            this.InitializeComponent();
            //Load saved state info
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //create a new list to use for the feed items
            feedData = new List<Feed_Items>();
            initPage();
        }


        /**This function will add new data to the stream. It will check to see if there is any saved data. If not it will
        create a new data object to be saved. If there is existing data it will pull it out, add the new item to the end and re-write*/
        private async Task AddNewFeedData()
        {

            string content = String.Empty;
            string feedItemToAdd = feedInput.Text;
            List<Feed_Items> readFeedData;
            var deserializer = new DataContractSerializer(typeof(List<Feed_Items>));

            //Try will run correctly if there is existing data
            try
            {
                var streamIn = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);
                readFeedData = (List<Feed_Items>)deserializer.ReadObject(streamIn);
            }

            //Exception will be thrown if there is no existing data, in which case we will create new data to save.
            catch (FileNotFoundException ex)
            {
                readFeedData = new List<Feed_Items>();
            }

            //Serialize and then write data to local storage
            readFeedData.Add(new Feed_Items(feedItemToAdd));
            var serializer = new DataContractSerializer(typeof(List<Feed_Items>));

            
            using (var streamOut = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
            {
                serializer.WriteObject(streamOut, readFeedData);
            }

            //Deseriazlie to refresh the displayed list
            await Deserialize();

            System.Diagnostics.Debug.WriteLine("Data Written");
            feedInput.Text = string.Empty;
        }


        //Deserialize is used to read the data from local storage and refresh the list on the page.
        private async Task Deserialize()
        {
            string content = String.Empty;
            List<Feed_Items> readFeedData;
            var deserializer = new DataContractSerializer(typeof(List<Feed_Items>));
            //Always clear all feed items before proceeding
            feedItems.Children.Clear();

            //This try block will attempt to locate and deserialize data in local folder
            try
            {
                var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);
                readFeedData = (List<Feed_Items>)deserializer.ReadObject(stream);


                foreach (var feed in readFeedData)
                {
                    TextBlock newTxt = new TextBlock();
                    newTxt.Text = feed.ItemName;
                    newTxt.FontSize = 20;
                    newTxt.Margin = new Thickness(10, 40, 10, 10);
                    feedItems.Children.Insert(0,newTxt);
                    content += String.Format("Item \n" + feed.ItemName);
                }

                System.Diagnostics.Debug.WriteLine(content);
            }

            //Exception will be thrown if there is no data.
            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine("No file to deserialize");
            }
        }

        //This method will get a list of urgent items to put at the top of the feed
        private async Task GetUrgentGroceries()
        {
            string content = String.Empty;
            List<Grocery_Items> readGroceryData;
            var deserializer = new DataContractSerializer(typeof(List<Grocery_Items>));

            //Most of this code is repeated from above
            try
            {
                var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("groceryData");
                readGroceryData = (List<Grocery_Items>)deserializer.ReadObject(stream);

                foreach (var food in readGroceryData)
                {
                    if (food.Urgent)
                    {
                        //Style the urgent items differently to indicate their urgency    
                        TextBlock newTxt = new TextBlock();
                        newTxt.Text = ("URGENT: Out Of "+food.ItemName);
                        newTxt.Foreground = new SolidColorBrush(Colors.Red);
                        newTxt.FontSize = 20;
                        newTxt.Margin = new Thickness(10, 40, 10, 10);
                        feedItems.Children.Insert(0,newTxt);
                    }
                }
            }

            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine("No file to deserialize");
            }
        }
        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private async void initPage()
        {
            // deserialize, populate list, then add urgent grocery items to top
            await Deserialize();
            await GetUrgentGroceries();
        }

        private void Groceries_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Grocery_Page));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings_Page));
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About_Page));
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            my_popup.IsOpen = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            my_popup.IsOpen = false;
        }

        private async void Add_Click_Confirm(object sender, RoutedEventArgs e)
        {
            //upon add click, add new data, close popup and then add urgent groceries
            await AddNewFeedData();
            my_popup.IsOpen = false;
            await GetUrgentGroceries();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            //When refresh is clicked, deserialize and then get urgent groceries
            await Deserialize();
            await GetUrgentGroceries();
        }
    }
}
