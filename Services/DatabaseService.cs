using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
using GIC.Models;

namespace GIC
{
    internal class DatabaseService
    {
        public string ConnectionString { get; set; }
        public DatabaseService()
        {
            // Default connection string for Windows and iOS
            ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=Edgar20230414!;Database=giftinformationscentralendb";

            // Check if running on an Android emulator or device
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Debug output to check the device model
                System.Diagnostics.Debug.WriteLine($"Device model: {DeviceInfo.Model}");

                if (DeviceInfo.Model.Contains("Emulator"))
                {
                    // Connection string for Android emulator
                    ConnectionString = "Host=10.0.2.2;Port=5432;Username=postgres;Password=Edgar20230414!;Database=giftinformationscentralendb";
                    System.Diagnostics.Debug.WriteLine("Detected Android Emulator.");
                }
                else
                {
                    // Connection string for Android physical device
                    ConnectionString = "Host=192.168.0.125;Port=5432;Username=postgres;Password=Edgar20230414!;Database=giftinformationscentralendb";
                    System.Diagnostics.Debug.WriteLine("Detected Android Physical Device.");
                }
            }
        }


        // Method to fetch all data from database and cache it in a JSON file
        public async Task CacheDataAsync()
        {
            var allProducts = new List<Product>();
            var allDescriptions = new List<Description>(); // List to store descriptions

            await using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                // Fetch product data
                await using (var command = new NpgsqlCommand("SELECT product_name, danger_level FROM products", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            allProducts.Add(new Product
                            {
                                Name = reader.GetString(0),
                                DangerLevel = reader.GetInt32(1)
                            });
                        }
                    }
                }

                // Fetch danger level descriptions
                await using (var command = new NpgsqlCommand("SELECT level, description FROM danger_levels", connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            allDescriptions.Add(new Description
                            {
                                DangerLevel = reader.GetInt32(0),
                                DescriptionText = reader.GetString(1).Replace("\\n", "\n")
                            });
                        }
                    }
                }
            }

            // Serialize product data to JSON
            string productsJson = JsonConvert.SerializeObject(allProducts, Newtonsoft.Json.Formatting.Indented);

            // Serialize descriptions data to JSON
            string descriptionsJson = JsonConvert.SerializeObject(allDescriptions, Newtonsoft.Json.Formatting.Indented);

            // Define file paths
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string productsJsonFileName = "products.json";
            string descriptionsJsonFileName = "descriptions.json";
            string productsJsonFilePath = Path.Combine(localPath, productsJsonFileName);
            string descriptionsJsonFilePath = Path.Combine(localPath, descriptionsJsonFileName);

            // Write JSON to files
            await File.WriteAllTextAsync(productsJsonFilePath, productsJson, Encoding.UTF8);
            await File.WriteAllTextAsync(descriptionsJsonFilePath, descriptionsJson, Encoding.UTF8);

            // For debugging purposes, you can write the paths to the console or a debug output
            System.Diagnostics.Debug.WriteLine($"Product JSON file saved at: {productsJsonFilePath}");
            System.Diagnostics.Debug.WriteLine($"Descriptions JSON file saved at: {descriptionsJsonFilePath}");
        }

    }
}
