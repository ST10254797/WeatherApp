using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text.Trim();

            if (string.IsNullOrEmpty(city))
            {
                MessageBox.Show("Please enter a city name.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var weather = await GetWeatherAsync(city);

            if (weather != null)
            {
                CityNameText.Text = $"City: {weather.Name}";
                TemperatureText.Text = $"Temperature: {weather.Main.Temp}°C";
                HumidityText.Text = $"Humidity: {weather.Main.Humidity}%";
                DescriptionText.Text = $"Description: {weather.Weather[0].Description}";

                // Update the weather icon based on the weather description
                UpdateWeatherIcon(weather.Weather[0].Description);
            }
            else
            {
                MessageBox.Show("Unable to fetch weather data. Please check the city name or your internet connection.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static async Task<WeatherResponse> GetWeatherAsync(string city)
        {
            string apiKey = "Enter your own API Key"; // Replace with your API key
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<WeatherResponse>(responseBody);
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {response.StatusCode} - {errorResponse}",
                                        "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void UpdateWeatherIcon(string weatherCondition)
        {
            // Map weather conditions to specific icons
            string iconPath = weatherCondition.ToLower() switch
            {
                "clear sky" => "Assets/icons/sunny.png",
                "few clouds" => "Assets/icons/cloudy.png",
                "scattered clouds" => "Assets/icons/cloudy.png",
                "broken clouds" => "Assets/icons/cloudy.png",
                "shower rain" => "Assets/icons/rainy.png",
                "rain" => "Assets/icons/rainy.png",
                "thunderstorm" => "Assets/icons/rainy.png",
                "snow" => "Assets/icons/snowy.png",
                _ => "Assets/icons/default.png" // Default icon for unknown conditions
            };

            // Update the Image source for the weather icon
            try
            {
                WeatherIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Check current theme by checking if DarkTheme is applied
            var currentTheme = this.Resources.MergedDictionaries.FirstOrDefault()?.Source?.OriginalString;

            // If the current theme is dark, switch to light
            if (currentTheme != null && currentTheme.Contains("DarkTheme"))
            {
                // Apply light theme
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/WeatherApp;component/Themes/LightTheme.xaml")
                });
            }
            else
            {
                // Apply dark theme
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/WeatherApp;component/Themes/DarkTheme.xaml")
                });
            }
        }

        // Root JSON response class
        public class WeatherResponse
        {
            public string Name { get; set; } // City name
            public Main Main { get; set; }
            public Weather[] Weather { get; set; }
        }

        public class Main
        {
            public double Temp { get; set; } // Temperature
            public int Humidity { get; set; } // Humidity
        }

        public class Weather
        {
            public string Description { get; set; } // Weather description
        }
    }
}
