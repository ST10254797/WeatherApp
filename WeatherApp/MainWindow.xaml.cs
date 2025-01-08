using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private List<string> searchHistory = new List<string>(); // Store searched cities

        public MainWindow()
        {
            InitializeComponent();
            UnitToggleButton.IsChecked = true;  // Set to Celsius by default
            LoadSearchHistory(); // Load previous search history when the app starts
        }

        private async void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            await GetWeatherForCity();
        }

        private async Task GetWeatherForCity()
        {
            string city = CityTextBox.Text.Trim();

            if (string.IsNullOrEmpty(city))
            {
                MessageBox.Show("Please enter a city name.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string unit = UnitToggleButton.IsChecked == true ? "metric" : "imperial";
            var weather = await GetWeatherAsync(city, unit);

            if (weather != null)
            {
                DisplayWeather(weather, unit);
                AddCityToHistory(city); // Add the city to search history
            }
            else
            {
                MessageBox.Show("Unable to fetch weather data. Please check the city name or your internet connection.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Display the weather details on the UI
        private void DisplayWeather(WeatherResponse weather, string unit)
        {
            CityNameText.Text = $"City: {weather.Name}";
            TemperatureText.Text = $"Temperature: {weather.Main.Temp}°{(unit == "metric" ? "C" : "F")}";
            HumidityText.Text = $"Humidity: {weather.Main.Humidity}%";
            DescriptionText.Text = $"Description: {weather.Weather[0].Description}";
            WindSpeedText.Text = $"Wind Speed: {weather.Wind.Speed} {(unit == "metric" ? "m/s" : "mph")}";
            WindDirectionText.Text = $"Wind Direction: {weather.Wind.Deg}°";
            VisibilityText.Text = $"Visibility: {weather.Visibility / 1000.0} km";
            PressureText.Text = $"Pressure: {weather.Main.Pressure} hPa";

            // Check if there are any weather alerts
            if (weather.Alerts != null && weather.Alerts.Any())
            {
                AlertsText.Text = $"Weather Alert: {weather.Alerts[0].Event} - {weather.Alerts[0].Description}";
            }
            else
            {
                AlertsText.Text = "No weather alerts at this time.";
            }

            // Update the weather icon based on the weather description
            UpdateWeatherIcon(weather.Weather[0].Description);
        }

        private async Task<WeatherResponse> GetWeatherAsync(string city, string unit)
        {
            string apiKey = "c2cfb03bac68bbf380f03cdbc32a83e4"; // Replace with your OpenWeather API key
            string cityUrl = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units={unit}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage cityResponse = await client.GetAsync(cityUrl);

                    if (cityResponse.IsSuccessStatusCode)
                    {
                        string cityResponseBody = await cityResponse.Content.ReadAsStringAsync();
                        var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(cityResponseBody);

                        return weatherResponse;
                    }
                    else
                    {
                        string errorResponse = await cityResponse.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {cityResponse.StatusCode} - {errorResponse}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    MessageBox.Show($"HTTP Error: {httpEx.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }

        // Update the weather icon based on the weather description
        private void UpdateWeatherIcon(string weatherCondition)
        {
            string iconPath = weatherCondition.ToLower() switch
            {
                "clear sky" => "Assets/icons/sunny.png",
                "few clouds" => "Assets/icons/cloudy.png",
                "scattered clouds" => "Assets/icons/cloudy.png",
                "broken clouds" => "Assets/icons/cloudy.png",
                "shower rain" => "Assets/icons/rainy.png",
                "rain" => "Assets/icons/rainy.png",
                "thunderstorm" => "Assets/icons/rainy.png",
                "snow" => "Assets/icons/snow.png",
                _ => "Assets/icons/default.png"
            };

            try
            {
                WeatherIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void UnitToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Determine the unit based on the toggle button
            string unit = UnitToggleButton.IsChecked == true ? "metric" : "imperial";  // "metric" for Celsius, "imperial" for Fahrenheit

            // Update the button content based on the selected unit
            if (UnitToggleButton.IsChecked == true)
            {
                UnitToggleButton.Content = "°C";  // Celsius
            }
            else
            {
                UnitToggleButton.Content = "°F";  // Fahrenheit
            }

            // Get the current city from the text box
            string city = CityTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(city))
            {
                // Fetch weather data with the new unit
                var weather = await GetWeatherAsync(city, unit);

                if (weather != null)
                {
                    // Update the weather information based on the unit change
                    CityNameText.Text = $"City: {weather.Name}";
                    TemperatureText.Text = $"Temperature: {weather.Main.Temp}°{(unit == "metric" ? "C" : "F")}";
                    HumidityText.Text = $"Humidity: {weather.Main.Humidity}%";
                    DescriptionText.Text = $"Description: {weather.Weather[0].Description}";

                    // Update the weather icon based on the weather description
                    UpdateWeatherIcon(weather.Weather[0].Description);
                }
            }
        }


        // Handle theme toggle click
        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var currentTheme = this.Resources.MergedDictionaries.FirstOrDefault()?.Source?.OriginalString;

            if (currentTheme != null && currentTheme.Contains("DarkTheme"))
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/WeatherApp;component/Themes/LightTheme.xaml")
                });
            }
            else
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/WeatherApp;component/Themes/DarkTheme.xaml")
                });
            }
        }

        // Update the WrapPanel that shows search history
        private void UpdateSearchHistoryList()
        {
            SearchHistoryPanel.Children.Clear();  // Clear existing buttons

            foreach (var city in searchHistory)
            {
                Button historyButton = new Button
                {
                    Content = city,
                    Width = 100,
                    Height = 30,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                historyButton.Click += (sender, e) =>
                {
                    CityTextBox.Text = city;
                    GetWeatherButton_Click(sender, e);  // Trigger search for the selected city
                };

                SearchHistoryPanel.Children.Add(historyButton);  // Add button to WrapPanel
            }
        }

        // Add city to search history and update the UI
        private void AddCityToHistory(string city)
        {
            if (!searchHistory.Contains(city))
            {
                searchHistory.Add(city);
                UpdateSearchHistoryList();
                SaveSearchHistory(); // Save to a file or local storage
            }
        }

        // Load search history from file or local storage
        private void LoadSearchHistory()
        {
            // For simplicity, using in-memory history only in this example.
            searchHistory = new List<string>(); // Replace with actual loading logic if needed
            UpdateSearchHistoryList();
        }

        // Save search history to file or local storage
        private void SaveSearchHistory()
        {
            // For simplicity, we're not implementing a file save here.
        }

        // Root JSON response class
        public class WeatherResponse
        {
            public string Name { get; set; }
            public Main Main { get; set; }
            public List<Weather> Weather { get; set; }
            public Wind Wind { get; set; }
            public Coord Coord { get; set; }
            public int Visibility { get; set; }
            public List<Alert> Alerts { get; set; }
        }

        public class Main
        {
            public double Temp { get; set; }
            public int Humidity { get; set; }
            public double Pressure { get; set; }
        }

        public class Weather
        {
            public string Description { get; set; }
        }

        public class Wind
        {
            public double Speed { get; set; }
            public int Deg { get; set; }
        }

        public class Coord
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        public class Alert
        {
            public string Event { get; set; }
            public string Description { get; set; }
        }
    }
}
