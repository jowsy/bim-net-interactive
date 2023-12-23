using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WixSharp;
using WixSharp.CommonTasks;
using Assembly = System.Reflection.Assembly;

var appName = "CQBimRevitConnector";
var appId = new Guid("641F5BA4-4AED-4AA3-8689-C5315BF469DF");
var appVersion = Assembly.GetExecutingAssembly().GetName().Version.ClearRevision();
var installLocation = @"%AppDataFolder%\Autodesk\Revit\Addins";
var publishFolderName = "Publish";
var installTargets = args.Length > 1 ? args : GetDefaultInstallTargets();
var vixEntities = GenerateWixEntities(installTargets);

new Project
{
    OutDir = "output",
    Name = appName,
    Version = appVersion,
    Platform = Platform.x64,
    UI = WUI.WixUI_ProgressOnly,
    GUID = appId,
    ProductId = Guid.NewGuid(),
    InstallScope = InstallScope.perUser,
    OutFileName = $"{appId}-{appVersion}",
    Dirs = new[] { new InstallDir(installLocation, vixEntities) },
    MajorUpgrade = new MajorUpgrade
    {
        AllowSameVersionUpgrades = true,
        DowngradeErrorMessage = $"A newer version of {appName} is already installed."
    },
    ControlPanelInfo =
    {
        Manufacturer = "Sweco AB",
    }
}.BuildMsi();

static WixEntity[] GenerateWixEntities(IEnumerable<string> args)
{
    var versionRegex = new Regex(@"\d+");
    var versionStorages = new Dictionary<string, List<WixEntity>>();

    foreach (var directory in args)
    {
        var directoryInfo = new DirectoryInfo(directory);
        var fileVersion = versionRegex.Match(directoryInfo.Name).Value;
        var feature = new Feature
        {
            Name = $"Revit {fileVersion}",
            Description = $"Install add-in for Revit {fileVersion}",
            ConfigurableDir = $"INSTALL{fileVersion}"
        };

        var files = new Files(feature, $@"{directory}\*.*");
        if (versionStorages.TryGetValue(fileVersion, out var storage))
            storage.Add(files);
        else
            versionStorages.Add(fileVersion, new List<WixEntity> { files });

        var assemblies = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        Console.WriteLine($"Installer files for version '{fileVersion}':");
        foreach (var assembly in assemblies) Console.WriteLine($"'{assembly}'");
    }

    return versionStorages
        .Select(storage => new Dir(new Id($"INSTALL{storage.Key}"), storage.Key, storage.Value.ToArray()))
        .Cast<WixEntity>()
        .ToArray();
}

string[] GetDefaultInstallTargets()
{
    var basePath = Assembly.GetExecutingAssembly().Location;
    string? publishPath = null;
    while (IsNotPublishFolder(publishPath))
    {
        try
        {
            basePath = Path.GetDirectoryName(basePath);
            publishPath = Directory.GetDirectories(basePath, publishFolderName).FirstOrDefault();
        }
        catch
        {
            throw new Exception("Unable to determine location of add-in contents to bundle");
        }
    }

    return Directory.GetDirectories(publishPath);
}

bool IsNotPublishFolder(string? publishPath)
{
    return publishPath is null
        || !Path.GetFileName(publishPath).Equals(publishFolderName, StringComparison.InvariantCultureIgnoreCase);
}