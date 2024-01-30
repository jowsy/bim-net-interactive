# Embedded NET Interactive Revit Kernel


This project aims to create documents or notebooks containing live Revit C#-scripts, visualizations, and narrative text. 
A simple way to document and demonstrate automations or simply experiment with code. This allows for cell-to-cell execution of revit API code in any order where results can be shared by reference between "code cells" in VS Code Polyglot Notebooks.

The solution constists of an extension to VS Code Polyglot Notebooks and an addin to Revit that acts as a data environment where variables are stored in memory during a session.

![](./samples/screenshot.png)

> 💥 There is a third-party dll conflict between NET interactive and Autodesk BIM 360 Issues addin. In order to get the full experience you have to temporarily disable the BIM 360 Issues addin. Variable sharing will not work when the addin is enabled.

Supported languages:
- [x] C# (the scripting dialect, see [C# Scripting](https://learn.microsoft.com/en-us/archive/msdn-magazine/2016/january/essential-net-csharp-scripting))
- [ ] F#
- [ ] Python

### Prequisities
- [x] Git
- [x] VS Code
- [x] VS Code Polyglot Notebook Extension
- [x] Autodesk Revit 2024

## How to get started
1. Clone this repo
2. Install Polyglot Notebook extension to VS Code
2. Open VS Code and open folder /src as working directory.
3. Open /src/GetStarted.ipynb and take it from there.

## License

MIT
