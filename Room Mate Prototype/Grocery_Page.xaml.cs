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
    public sealed partial class Grocery_Page : Page
    {
        private List<Grocery_Items> groceryData;
        private const string fileName = "groceryData";

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        //Default constructor
        public Grocery_Page()
        {
            //load saved state info
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            groceryData = new List<Grocery_Items>();

            initPage();
        }

        //This method will check if local storage exists, if so it will add new data and re-save
        //if not, it will create new data and save
        private async Task AddNewGroceryData()
        {
            
            string content = String.Empty;
            string groceryItemToAdd = groceryInput.Text;
            List<Grocery_Items> readGroceryData;
            var deserializer = new DataContractSerializer(typeof(List<Grocery_Items>));
           
            //try to find data to deserialize
            try
            {
                var streamIn = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);
                readGroceryData = (List<Grocery_Items>)deserializer.ReadObject(streamIn);
            }

            //exception is thrown if no data, then create new data
            catch (FileNotFoundException ex)
            {
                readGroceryData = new List<Grocery_Items>();
            }

            //If item has been marked as urgent, add item with urgent marked as true
            if (UrgentToggle.Content.ToString() == "URGENT")
            {
                readGroceryData.Add(new Grocery_Items(groceryItemToAdd, false, true));
            }

            //otherwise add gorcery data with urgent marked as false
            else
            {
                readGroceryData.Add(new Grocery_Items(groceryItemToAdd, false, false));
            }
            
            //Deserialize, refresh page
            var serializer = new DataContractSerializer(typeof(List<Grocery_Items>));

            using (var streamOut = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
            {
                serializer.WriteObject(streamOut, readGroceryData);
            }

            await Deserialize();

            System.Diagnostics.Debug.WriteLine("Data Written");
            groceryInput.Text = string.Empty;
        }

        //This function runs to delete all grocery list items and clear the list
        private async Task ClearGroceryList()
        {

            try
            {
                var stream = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                await stream.DeleteAsync();
                await Deserialize();
                //no exception means file exists
            }
            catch (FileNotFoundException ex)
            {
                //find out through exception 
                System.Diagnostics.Debug.WriteLine("No File Exists");
            }
            
        }

        //This will check off a selected grocery item and move it to the checked off list. It will not delete the item.
        private async Task DeleteGroceryItem()
        {
            RadioButton radioToDelete = UncheckedGroceryItems.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);
           
            //If the user has not made a selection
            if (radioToDelete == null)
            {
                System.Diagnostics.Debug.WriteLine("Nothing to check off");
            }

            else 
            {
                //Find info about the selected item to check off
                string itemToDelete = radioToDelete.Content.ToString();
                System.Diagnostics.Debug.WriteLine(itemToDelete);

                //prepare to deserialize and pull all data from localstorage
                string content = String.Empty;
                string groceryItemToAdd = groceryInput.Text;
                List<Grocery_Items> readGroceryData;
                var deserializer = new DataContractSerializer(typeof(List<Grocery_Items>));

                //Try to deserialzie
                try
                {
                    var streamIn = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);
                    readGroceryData = (List<Grocery_Items>)deserializer.ReadObject(streamIn);
                    var i = 0;
                    var indexToDelete = 0;

                    //Loop through to find the index value of the item that needs checking off
                    foreach (var food in readGroceryData)
                    {

                        if (food.ItemName.Equals(itemToDelete))
                        {
                            indexToDelete = i;
                        }
                        i++;
                    }

                    //remove the item, add it back at the same index with true for checked off
                    readGroceryData.RemoveAt(indexToDelete);
                    //Urgency is automatically set to false when an item is checked off
                    readGroceryData.Add(new Grocery_Items(itemToDelete, true, false));
                    var serializer = new DataContractSerializer(typeof(List<Grocery_Items>));

                    //Save the data back to local storage
                    using (var streamOut = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
                    {
                        serializer.WriteObject(streamOut, readGroceryData);
                    }

                    await Deserialize();
                }

                catch (FileNotFoundException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error: " + ex);
                }
            }

        }

        //This function deserializes and updates the entire page
        //Functionality is nearly identical to Feed_Page.xaml.cs Deserialize task, refer there for more verbose comments
        private async Task Deserialize()
        {
            string content = String.Empty;
            List<Grocery_Items> readGroceryData;
            List<RadioButton> urgentItemQueue = new List<RadioButton>();
            var deserializer = new DataContractSerializer(typeof(List<Grocery_Items>));
            UncheckedGroceryItems.Children.Clear();
            CheckedGroceryItems.Children.Clear();

            try
            {
                var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);
                readGroceryData = (List<Grocery_Items>)deserializer.ReadObject(stream);
                
                foreach (var food in readGroceryData)
                {
                    RadioButton newBtn = new RadioButton();
                    newBtn.Content = food.ItemName;

                    if (food.CheckedOff)
                    {
                        newBtn.IsEnabled = false;
                        CheckedGroceryItems.Children.Add(newBtn);
                    }

                    else
                    {
                        if (food.Urgent)
                        {
                            newBtn.Foreground = new SolidColorBrush(Colors.Red);
                            urgentItemQueue.Add(newBtn);
                        }
                        else
                        {
                            UncheckedGroceryItems.Children.Add(newBtn);
                        }
                    }
                    content += String.Format("Item \n" + food.ItemName);
                }

                System.Diagnostics.Debug.WriteLine(content);
            }

            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine("No file to deserialize");
            }

            if (urgentItemQueue.Count() > 0)
            {
                foreach(RadioButton food in urgentItemQueue)
                {
                    UncheckedGroceryItems.Children.Insert(0, food);
                }
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

        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Feed_Page));
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

        //This method toggles the urgency of the new item
        private void Toggle_Urgent(object sender, RoutedEventArgs e)
        {
            if ((Convert.ToString(UrgentToggle.Content)) == "URGENT")
            {
                UrgentToggle.Content = "Not Urgent";
            }

            else
            {
                UrgentToggle.Content = "URGENT";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            my_popup.IsOpen = false;
        }

        private async void Add_Click_Confirm(object sender, RoutedEventArgs e)
        {
            await AddNewGroceryData();
            my_popup.IsOpen = false;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await Deserialize();
        }

        private async void initPage()
        {
            await Deserialize();
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            await ClearGroceryList();
        }

        private async void Check_Click(object sender, RoutedEventArgs e)
        {
            await DeleteGroceryItem();
        }
    }
}
