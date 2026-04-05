using System;
using System.IO;
using System.Windows;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bookmarkaa.Helpers;
using Bookmarkaa.Models;

namespace Bookmarkaa.Managers;

public static class SettingsManager
{
    public static AppSettings Settings { get; set; } = new AppSettings();
    public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Constants.CompanyName,
        Constants.AppName,
        Constants.SettingsFileName);

    static SettingsManager()
    {
        ReadSettings();
    }

    private static void NotifyStaticPropertyChanged([CallerMemberName] string propertyName = "")
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }

    public static void ReadSettings()
    {
        try
        {
            string? directory = Path.GetDirectoryName(SettingsFilePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            if (!File.Exists(SettingsFilePath))
            {
                Settings = new AppSettings();
                return;
            }

            var json = File.ReadAllText(SettingsFilePath);
            Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to read settings: {ex.Message}");
            Settings = new AppSettings();
        }

    }

    public static void SaveSettings()
    {
        string? directory = Path.GetDirectoryName(SettingsFilePath);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        try
        {
            var json = JsonSerializer.Serialize(Settings, Options);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}

