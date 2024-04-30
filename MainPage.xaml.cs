using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using FuzzySharp;
using GIC.Utilities;
using GIC.Models;
using Android.App.AppSearch;


namespace GIC
{
    public partial class MainPage : ContentPage
    {
        private List<Product> _allProducts;
        private List<Description> _descriptions;
        private DatabaseService _databaseService;
        private TimeStampHelper _timeStampHelper;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _timeStampHelper = new TimeStampHelper();
            InitializeDataAsync();

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
        }


        private async Task LoadDataFromJsonAsync()
        {
            // Load products data
            string productsJsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "products.json");
            try
            {
                string productsJsonData = await File.ReadAllTextAsync(productsJsonFilePath);
                _allProducts = JsonConvert.DeserializeObject<List<Product>>(productsJsonData);
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
                _descriptions = JsonConvert.DeserializeObject<List<Description>>(descriptionsJsonData);
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
            if (_allProducts == null)
            {
                Console.WriteLine("Data not loaded, please check data initialization.");
                return;
            }

            string searchText = e.NewTextValue;
            List<string> searchResults = SearchProducts(searchText);

            // Update the ItemsSource for the CollectionView
            SuggestionResults.ItemsSource = searchResults;

            // Set the visibility based on whether there are items to display
            SuggestionResults.IsVisible = searchResults.Any(); // This will be true if there are items, false otherwise
        }





        private List<string> SearchProducts(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                SearchResults.Text = string.Empty;
                DescriptionHeaderLabel.IsVisible = false;
                // Return an empty list if the search text is empty or null
                return new List<string>();

            }

            // Continue with the filtering if search text is not empty
            return _allProducts.Where(p => p.Name.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                               .Select(p => p.Name)
                               .ToList();
        }






        //private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedProductName = e.CurrentSelection.FirstOrDefault() as string; // Assuming the items are strings

        //    if (selectedProductName != null)
        //    {
        //        // Find the selected product from the list of all products
        //        var selectedProduct = _allProducts.FirstOrDefault(p => p.Name == selectedProductName);

        //        if (selectedProduct != null)
        //        {
        //            // Display the description for the selected product
        //            DisplayDescriptionForSelectedProduct(selectedProduct);

        //        }
        //        else
        //        {
        //            // Handle case where selected product is not found
        //            SearchResults.Text = "Product not found";
        //        }
        //    }
        //    else
        //    {
        //        // Handle case where no product is selected
        //        SearchResults.Text = "No product selected";
        //    }

        //    ProductSearchBar.Unfocus();
        //}


        //private void DisplayDescriptionForSelectedProduct(Product selectedProduct)
        //{
        //    if (selectedProduct == null)
        //    {
        //        // Handle case where no product is selected
        //        SearchResults.Text = "No product selected";
        //        return;
        //    }

        //    // Find the description corresponding to the selected product's danger level
        //    var matchingDescription = _descriptions.FirstOrDefault(d => d.DangerLevel == selectedProduct.DangerLevel);

        //    if (matchingDescription != null)
        //    {
        //        // Display the description in the SearchResults label
        //        SearchResults.Text = matchingDescription.DescriptionText;
        //    }
        //    else
        //    {
        //        // Handle case where no description is found for the selected product's danger level
        //        SearchResults.Text = "No description found";
        //    }
        //}

        //private void OnSearchButtonClicked(object sender, EventArgs e)
        //{
        //    string searchText = ProductSearchBar.Text;  // Assuming you have a SearchBar named ProductSearchBar
        //    if (string.IsNullOrEmpty(searchText))
        //    {
        //        SearchResults.Text = "Please enter a product name to search.";
        //        return;
        //    }

        //    List<string> searchResults = SearchProducts(searchText);

        //    if (!searchResults.Any())
        //    {
        //        // No exact matches found, perform fuzzy search
        //        var bestMatch = _allProducts
        //            .Select(p => new { Product = p, Score = Fuzz.PartialRatio(p.Name, searchText) })
        //            .OrderByDescending(p => p.Score)
        //            .FirstOrDefault();

        //        if (bestMatch != null && bestMatch.Score > 60)  // Adjust the score threshold as needed
        //        {
        //            SearchResults.Text = $"Didn't find anything for your search '{searchText}'. Did you mean '{bestMatch.Product.Name}'?";
        //        }
        //        else
        //        {
        //            SearchResults.Text = $"No results found for '{searchText}'.";
        //        }
        //    }
        //    else
        //    {
        //        // Update the ItemsSource for the CollectionView to show the exact matches
        //        SuggestionResults.ItemsSource = searchResults;
        //        SuggestionResults.IsVisible = true;
        //    }
        //}


        private void DisplayDescription(Product selectedProduct)
        {
            if (selectedProduct == null)
            {
                // Handle case where no product is selected
                SearchResults.Text = "No product selected";
                return;
            }

            // Find the description corresponding to the selected product's danger level
            var matchingDescription = _descriptions.FirstOrDefault(d => d.DangerLevel == selectedProduct.DangerLevel);

            if (matchingDescription != null)
            {
                // Display the description in the SearchResults label
                SearchResults.Text = matchingDescription.DescriptionText;
            }
            else
            {
                // Handle case where no description is found for the selected product's danger level
                SearchResults.Text = "No description found";
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProductName = e.CurrentSelection.FirstOrDefault() as string; // Assuming the items are strings

            if (selectedProductName != null)
            {
                // Find the selected product from the list of all products
                var selectedProduct = _allProducts.FirstOrDefault(p => p.Name == selectedProductName);

                // Call the method to display the description
                DisplayDescription(selectedProduct);
            }
            else
            {
                // Handle case where no product is selected
                SearchResults.Text = "No product selected";
            }

            ProductSearchBar.Unfocus();
        }

        // Event handler for SearchButtonClicked
        private void OnSearchButtonClicked(object sender, EventArgs e)
        {
            string searchText = ProductSearchBar.Text?.Trim();  // Assuming you have a SearchBar named ProductSearchBar
            if (string.IsNullOrEmpty(searchText))
            {
                SearchResults.Text = "Please enter a product name to search.";
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
                    AmountOfHits.Text = $"Hittade 0 träffar på din sökning {searchText}.";
                    DidYouMeanLabel.IsVisible = true;
                    SuggestionLabel.Text = bestMatch.Value;
                    SuggestionLabel.IsVisible = true; // Show the suggestion label
                    return;
                }
                else
                {
                    // No close match found, display a message
                    SearchResults.Text = $"No results found for '{searchText}'.";
                    return;
                }
            }

            // Call the method to display the description
            DisplayDescription(selectedProduct);
        }

        // Event handler for tapping on the suggestion label
        private void OnSuggestionLabelTapped(object sender, EventArgs e)
        {
            string suggestionText = SuggestionLabel.Text?.Trim(); // Get the text from the suggestion label

            if (!string.IsNullOrEmpty(suggestionText))
            {
                // Find the product matching the suggestion text (case-insensitive match)
                var selectedProduct = _allProducts.FirstOrDefault(p => p.Name.Equals(suggestionText, StringComparison.OrdinalIgnoreCase));

                if (selectedProduct != null)
                {
                    // Call the method to display the description
                    DisplayDescription(selectedProduct);
                }
              
            }
        }


    }

}
