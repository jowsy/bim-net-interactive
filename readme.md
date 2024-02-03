# Revit Polyglot Notebook

This project aims to create documents or notebooks containing live Revit C#-scripts, visualizations, and narrative text. 
A simple way to document and demonstrate automations or simply experiment with code. This allows for cell-to-cell execution of revit API code in any order where results can be shared by reference between "code cells" in a notebook.

The solution constists of an extension to VS Code Polyglot Notebooks and an addin to Revit that acts as a data environment where variables are stored in memory during a session.

**Supported languages:**
- [x] C# (the scripting dialect, see [C# Scripting](https://learn.microsoft.com/en-us/archive/msdn-magazine/2016/january/essential-net-csharp-scripting))
- [ ] F#
- [ ] Python

### Prequisities
- [x] SDKs for .NET framework 4.8 and NET 8
- [x] Git
- [x] VS Code
- [x] VS Code Polyglot Notebook Extension
- [x] Autodesk Revit 2024

## How to get started
1. Clone this repo
2. Install Polyglot Notebook extension to VS Code
2. Open VS Code and open folder /src as working directory.
3. Open /src/GetStarted.ipynb and take it from there.

> 💥 **There is a third-party dll conflict between NET interactive and Autodesk BIM 360 Issues addin. In order to get the full experience you have to temporarily disable the BIM 360 Issues addin. Variable sharing will not work when the addin is enabled.**

## Examples

![](./samples/example2.gif)

## Aknowledgements and third-party dependencies

* This solution stands steadily on [NET Interactive](https://github.com/dotnet/interactive) for handling inter-process communication.
* Got a lot inspiration from [RevitAddinManager](https://github.com/chuongmep/RevitAddInManager) for how to configure csproj.
* Credit to Alexander Sharykin for his [https://github.com/jowsy/RetroUI](RetroUI) WPF theme (forked by me and adapted for use in a Revit addin).

## License

MIT
