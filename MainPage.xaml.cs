using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using FuzzySharp;
using GIC.Utilities;
using GIC.Models;
using System.ComponentModel;
#if ANDROID
using Android.Content;
using Android.App.AppSearch;
#endif


namespace GIC
{
    public partial class MainPage : ContentPage
    {

        private List<Product> _allProducts;
        private List<Description> _descriptions;
        private DatabaseService _databaseService;
        private TimeStampHelper _timeStampHelper;
        private ColorStyling _colorStyling;

        private PhoneCall phoneCallHelper;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _timeStampHelper = new TimeStampHelper();
            _colorStyling = new ColorStyling();
            phoneCallHelper = new PhoneCall();
            _allProducts = new List<Product>();
            _descriptions = new List<Description>();
            InitializeAsync();
            

        }
        private async void InitializeAsync()
        {
            await InitializeDataAsync();
            // Other initialization code
        }
        private async Task InitializeDataAsync()
        {
            
            string jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "products.json");

            // Load the last known update timestamp
            var lastKnownTimestamp = _timeStampHelper.LoadLastKnownUpdateTimestamp();
            var currentTimestamp = await _timeStampHelper.GetLastUpdateTimestampAsync();

            bool shouldCacheData = !File.Exists(jsonFilePath);

            if (currentTimestamp.HasValue)
            {
                // If there's no last known timestamp, we should cache data
                // OR if the current timestamp is more recent than the last known, we should cache data
                shouldCacheData |= lastKnownTimestamp == null || currentTimestamp.Value > lastKnownTimestamp;
            }

            if (shouldCacheData)
            {
                await _databaseService.CacheDataAsync();
                // Update the last known timestamp after successful caching
                if (currentTimestamp.HasValue)
                {
                    _timeStampHelper.SaveLastKnownUpdateTimestamp(currentTimestamp.Value);
                }
            }

            // Always load the latest data from the JSON file
            await LoadDataFromJsonAsync();
            ProductSearchBar.Focus();
        }

        private async Task LoadDataFromJsonAsync()
        {
            // Initialize _allProducts and _descriptions with empty lists
            _allProducts = new List<Product>();
            _descriptions = new List<Description>();

            // Load products data
            string productsJsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "products.json");
            try
            {
                string productsJsonData = await File.ReadAllTextAsync(productsJsonFilePath);
                _allProducts = JsonConvert.DeserializeObject<List<Product>>(productsJsonData) ?? new List<Product>();
            }
            catch (IOException ex)
            {
                // Handle errors such as file not found or lack of permissions
                Console.WriteLine($"An error occurred while reading the products JSON file: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle errors in JSON formatting
                Console.WriteLine($"An error occurred while deserializing the products JSON data: {ex.Message}");
            }

            // Load descriptions data
            string descriptionsJsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "descriptions.json");
            try
            {
                string descriptionsJsonData = await File.ReadAllTextAsync(descriptionsJsonFilePath);
                _descriptions = JsonConvert.DeserializeObject<List<Description>>(descriptionsJsonData) ?? new List<Description>();
                // You can process the loaded descriptions as needed
            }
            catch (IOException ex)
            {
                // Handle errors such as file not found or lack of permissions
                Console.WriteLine($"An error occurred while reading the descriptions JSON file: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Handle errors in JSON formatting
                Console.WriteLine($"An error occurred while deserializing the descriptions JSON data: {ex.Message}");
            }
        }


        public void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DescriptionHeaderLabel.IsVisible = false;
            DescriptionText.IsVisible = false;
            if (_allProducts == null)
            {
                Console.WriteLine("Data not loaded, please check data initialization.");
                return;
            }

            string searchText = e.NewTextValue;
            List<string> searchResults = SearchProducts(searchText);

            // Update the ItemsSource for the CollectionView
            SuggestionResults.ItemsSource = searchResults;
            

            // Clear the selection if the search text is empty
            if (string.IsNullOrEmpty(searchText))
            {
                // Construct HTML with dynamic background color
                string htmlContent = $"<div style='background-color: {000}>";
          
               

                // Set the HTML content of the WebView
                DescriptionText.Source = new HtmlWebViewSource
                {
                    Html = htmlContent
                };

                SuggestionResults.SelectedItem = null;
                DescriptionHeaderLabel.IsVisible = false;
                DescriptionText.IsVisible = false;
                SuggestionLabel.IsVisible = false;
                DidYouMeanLabel.IsVisible = false;
                AmountOfHits.IsVisible = false;
            }

            // Set the visibility based on whether there are items to display
            SuggestionResults.IsVisible = searchResults.Any(); // This will be true if there are items, false otherwise
        }

        private List<string> SearchProducts(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                DescriptionText.Source = string.Empty;
                DescriptionHeaderLabel.IsVisible = false;
                // Return an empty list if the search text is empty or null
                return new List<string>();

            }

            // Continue with the filtering if search text is not empty
            return _allProducts.Where(p => p.Name.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                               .Select(p => p.Name)
                               .ToList();
        }


        private void DisplayDescription(Product selectedProduct)
        {
            if (selectedProduct == null)
            {
                // Handle case where no product is selected
                DescriptionText.Source = "No product selected";
                return;
            }

            // Retrieve the corresponding description
            var matchingDescription = _descriptions.FirstOrDefault(d => d.DangerLevel == selectedProduct.DangerLevel);
            string colorHex = _colorStyling.GetColorByDangerLevel(selectedProduct.DangerLevel);

            if (matchingDescription != null)
            {
                // Construct HTML with dynamic background color
                string htmlContent = $"<div style='background-color: {colorHex}; padding: 20px; Height: 250px; '>{matchingDescription.DescriptionText}</div>";

                // Show the WebView
                DescriptionText.IsVisible = true;
                SuggestionResults.IsVisible = false;
                DescriptionHeaderLabel.IsVisible = true;
                AmountOfHits.IsVisible = true;
                AmountOfHits.Text = $"Hittade 1 träff på din sökning {selectedProduct.Name}.";

                // Set the HTML content of the WebView
                DescriptionText.Source = new HtmlWebViewSource
                {
                    Html = htmlContent
                };
            }
            else
            {
                // Handle case where no matching description is found
                DescriptionText.Source = "No description available";
                SuggestionResults.IsVisible = false;
            }
        }



        private void OnSelectedProductInList(object sender, SelectionChangedEventArgs e)
        {
            var selectedProductName = e.CurrentSelection.FirstOrDefault() as string; // Assuming the items are strings

            if (selectedProductName != null)
            {
                ProductSearchBar.Text = selectedProductName;

                // Find the selected product from the list of all products
                var selectedProduct = _allProducts.FirstOrDefault(p => p.Name == selectedProductName);

                // Check if selectedProduct is not null before calling DisplayDescription
                if (selectedProduct != null)
                {
                    // Call the method to display the description
                    DisplayDescription(selectedProduct);
                }
                else
                {
                    // Handle the case where selectedProduct is null (optional)
                    // For example: show an error message or perform alternative action
                }
            }

            ProductSearchBar.Unfocus();
        }



        // Event handler for SearchButtonClicked
        private void OnSearchButtonClicked(object sender, EventArgs e)
        {
            string? searchText = ProductSearchBar.Text?.Trim();  // Assuming you have a SearchBar named ProductSearchBar
            if (string.IsNullOrEmpty(searchText))
            {
                DescriptionText.Source = "Please enter a product name to search.";
                return;
            }

            // Find the product matching the search text (case-insensitive match)
            var selectedProduct = _allProducts.FirstOrDefault(p => p.Name.Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (selectedProduct == null)
            {
                // No exact match found, perform fuzzy search
                var bestMatch = Process.ExtractOne(searchText, _allProducts.Select(p => p.Name).ToList());

                if (bestMatch != null && bestMatch.Score >= 80) // Adjust the score threshold as needed
                {
                    // Display a message suggesting the best match
                    AmountOfHits.IsVisible = true;
                    AmountOfHits.Text = $"Hittade 0 träffar på din sökning {searchText}.";
                    DidYouMeanLabel.IsVisible = true;
                    SuggestionLabel.Text = bestMatch.Value;
                    SuggestionLabel.IsVisible = true; // Show the suggestion label
                    SuggestionResults.IsVisible = false;
                    return;
                }
                else
                {
                    // No close match found, display a message
                    AmountOfHits.IsVisible = true;
                    AmountOfHits.Text = $"Hittade 0 träffar på din sökning {searchText}.";
                    return;
                }
            }

            // Call the method to display the description
            DisplayDescription(selectedProduct);
            ProductSearchBar.Unfocus();
        }

        // Event handler for tapping on the suggestion label
        private void OnSuggestionLabelTapped(object sender, EventArgs e)
        {
            string? suggestionText = SuggestionLabel.Text?.Trim(); // Get the text from the suggestion label

            if (!string.IsNullOrEmpty(suggestionText))
            {
                // Find the product matching the suggestion text (case-insensitive match)
                var selectedProduct = _allProducts.FirstOrDefault(p => p.Name.Equals(suggestionText, StringComparison.OrdinalIgnoreCase));

                if (selectedProduct != null)
                {
                    // Call the method to display the description
                    ProductSearchBar.Text = selectedProduct.Name;
                    DisplayDescription(selectedProduct);
                    SuggestionLabel.IsVisible = false;
                    DidYouMeanLabel.IsVisible = false;
                }
              
            }
        }




        private async void EmergencyCallButton_Clicked(object sender, EventArgs e)
        {
            await phoneCallHelper.CallEmergencyNumber(this);
        }


        private async void NonEmergencyCallButton_Clicked(object sender, EventArgs e)
        {
            await phoneCallHelper.CallNonEmergencyNumber(this);
        }



    }

}
