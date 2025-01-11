﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace VsRemoteWinFsClient;

internal class ConfigSite
{
    public ConfigSite() { }
    [JsonIgnore]
    public Guid Id { get; } = Guid.NewGuid();
    public required string Label { get; set; }
    public required string Address { get; set; }
    public int Port { get; set; } = 9099;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public required string MountPoint { get; set; }
}

internal class ConfigFolder
{
    [JsonIgnore]
    public Guid Id { get; } = Guid.NewGuid();
    public string Label { get; set; } = string.Empty;
    public IEnumerable<ConfigFolder> Folders { get; set; } = Enumerable.Empty<ConfigFolder>();
    public IEnumerable<ConfigSite> Sites { get; set; } = Enumerable.Empty<ConfigSite>();

    [JsonIgnore]
    public bool Empty
        => (Folders == null || !Folders.Any()) && (Sites == null || !Sites.Any());

    public ConfigFolder? FindFolderFolder(Guid id)
    {
        ConfigFolder? tmp = null;
        foreach (var subf in Folders)
        {
            if (subf.Id == id)
                return this;
            tmp = subf.FindFolderFolder(id);
            if (tmp != null)
                return tmp;
        }
        return tmp;
    }

    public ConfigFolder? FindSiteFolder(Guid id)
    {
        ConfigFolder? tmp = null;
        foreach (var site in Sites)
        {
            if (site.Id == id)
                return this;
        }
        foreach (var subf in Folders)
        {
            tmp = subf.FindSiteFolder(id);
            if (tmp != null)
                return tmp;
        }
        return tmp;
    }

}

internal static class TreeFactory
{
    private const string SitesConfigFile = "vsremote.json";
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true
    };

    public static ConfigFolder ReadSitesConfiguration()
    {
        if (File.Exists(SitesConfigFile))
        {
            return JsonSerializer.Deserialize<ConfigFolder>(File.ReadAllText(SitesConfigFile))
                ?? throw new Exception("Invalid Sites Configuration File!");
        }
        else
        {
            return new ConfigFolder()
            {
                Label = "<empty>"
            };
        }
    }

    internal static void SaveSitesConfiguration(ConfigFolder sitesConfig)
    {
        var jsontxt = JsonSerializer.Serialize(sitesConfig, serializerOptions);
        File.WriteAllText(SitesConfigFile, jsontxt);
    }
}
