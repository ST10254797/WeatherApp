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
            UnitToggleButton.IsChecked = true;  // Set to Celsius by default
        }

        private async void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text.Trim();

            if (string.IsNullOrEmpty(city))
            {
                MessageBox.Show("Please enter a city name.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Determine whether to use Celsius or Fahrenheit based on the toggle button
            string unit = UnitToggleButton.IsChecked == true ? "metric" : "imperial";  // "metric" for Celsius, "imperial" for Fahrenheit

            var weather = await GetWeatherAsync(city, unit);

            if (weather != null)
            {
                CityNameText.Text = $"City: {weather.Name}";
                TemperatureText.Text = $"Temperature: {weather.Main.Temp}°{(unit == "metric" ? "C" : "F")}";
                HumidityText.Text = $"Humidity: {weather.Main.Humidity}%";
                DescriptionText.Text = $"Description: {weather.Weather[0].Description}";
                WindSpeedText.Text = $"Wind Speed: {weather.Wind.Speed} {(unit == "metric" ? "m/s" : "mph")}";
                WindDirectionText.Text = $"Wind Direction: {weather.Wind.Deg}°";
                VisibilityText.Text = $"Visibility: {weather.Visibility / 1000.0} km";
                PressureText.Text = $"Pressure: {weather.Main.Pressure} hPa";


                // Update the weather icon based on the weather description
                UpdateWeatherIcon(weather.Weather[0].Description);
            }
            else
            {
                MessageBox.Show("Unable to fetch weather data. Please check the city name or your internet connection.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static async Task<WeatherResponse> GetWeatherAsync(string city, string unit)
        {
            string apiKey = "c2cfb03bac68bbf380f03cdbc32a83e4"; // Replace with your API key
            string cityUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units={unit}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Get city weather data
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
        "snow" => "Assets/icons/snow.png",
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
            public string Name { get; set; }
            public Main Main { get; set; }
            public Weather[] Weather { get; set; }
            public Coord Coord { get; set; }
            public Wind Wind { get; set; }
            public int Visibility { get; set; }
        }

        public class Main
        {
            public double Temp { get; set; } // Temperature
            public int Humidity { get; set; } // Humidity
            public int Pressure { get; set; } // Pressure
        }

        public class Weather
        {
            public string Description { get; set; } // Weather description
        }
        public class Coord
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        public class Wind
        {
            public double Speed { get; set; }
            public int Deg { get; set; }
        }

    }
}
