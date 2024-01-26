# Embedded NET Interactive Revit Kernel
**Bringing 📜Literary Programming to Autodesk Revit**

Run Autodesk Revit code live with Visual Studio Code Polyglot Notebook extension and connect to other language kernels such as Python to explore new possibilites. The aim is to let user to develop in their own thought patterns, not subject to the order imposed by the computer. Collaborate with Github Copilot and ChatGPT to speed up development and have fun learning and exploring the Revit API.

![](./samples/screenshot.png)

> ❗  This project is still in early development.

> 💥 There is a third-party dll conflict between NET interactive and Autodesk BIM 360 Issues addin. In order to get the full experience you have to temporarily disable the BIM 360 Issues addin. It is still possible to run code but formatting won't work.

Supported languages:
- [x] C#
- [ ] F#
- [ ] Python

### Prequisities
- [x] Git
- [x] VSCode
- [x] VSCode Polyglot Notebook Extension
- [x] Autodesk Revit 2024

## How to get started
1. Clone this repo
2. Open Visual Studio Code and open folder /src as working directory. Make sure that Polyglot Notebook extension is installed!
3. Open /src/GetStarted.ipynb and follow instructions. In the notebook you will run code to build necessary assemblies, packages and installing addins.

## License

MIT
