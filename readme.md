# Embedded NET Interactive Revit Kernel
**Bringing 📜Literary Programming to Autodesk Revit**

Run Autodesk Revit code live with Visual Studio Code Polyglot Notebook extension and connect to other language kernels such as Python to explore new connections and new possibilites. The aim is to let user to develop in their own thought patterns, not subject to the order imposed by the computer. Collaborate with Github Copilot and ChatGPT to speed up development and have fun learning and exploring the Revit API.

[!NOTE]  
> This project is still in early development.



## Get started
The project is still in a early phase and the plan is to provide an installer in the future. But below is a guide for developers to setup the solution.

### What you need

- [x] Visual Studio Code
- [x] Polyglot Notebook to Visual Studio Code
- [x] Autodesk Revit 2024
- [x] Visual Studio 2022
### First step: Compile solution, start kernel
1. Clone this repo
2. Open and build solution

Building the solution will copy addin-manifest and assemblies. It will also publish a nuget package containing the Polyglot Notebook extension.

Open Autodesk Revit. Under Add-ins you'll see a button that lets you open the dockable pane window. Then "Start" the kernel.


### Second step: Connect to Revit Kernel
Open VS Code and explore [ready made samples](/samples/) or create a new notebook in Polyglot Notebooks extension in the sample directory.

Create a new cell and add the revit kernel using a #r magic command.

```
#r "Jowsy.DotNet.Interactive.Extensions.Revit"
```
If the extension did load successfully, use #!connect command to establish connection with the embedded kernel inside Autodesk Revit. Use whatever name you want.
```
#!connect revit --kernel-name revit24 --revit-version 2024
```
If this went sucessfully, we can start running code inside Revit.
### Global Variables
When a embedded revit kernel is started in Revit three variables are automatically created: uiapp (UIApplication), uidoc (UIApplication) and doc (Document). They are then available in cells when running on the revit kernel.

### Running code
If you want to send code to the embedded Revit kernel you provide the kernel name as a magic command. Everything below that command will be executed in the Revit process. Below code will display the name of the active view (hopefully).
```
#!revit24
display(uidoc.ActiveView.Name);
```
You may return a variable according to C# script:

```
#!revit24
var activeView = uidoc.ActiveView.Name;
activeView
```
This will return activeView as a variable and it will be added in the list of variables in Autodesk Revit.

⚠️Warning: you might display Autodesk element classes but beware because .NET interactive is configured to render all classes of type Autodesk.DB.Element to html. Large classes (such as Document) with deep structures can take a great deal of time to render (up to several minutes).

### What about IntelliSense?
For the time being the embedded Revit kernel does not support intelliSense. The plan is to support it in the future though. 

But a quick fix right now is to comment the magic command temporarily and install nuget revit packages. This will provide IntelliSense in the C#-cell.

```
#r "nuget:Revit.RevitApi.x64, 2023.0.0"
#r "nuget:Revit.RevitApiUi.x64, 2023.0.0"
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
```


### Version support : Revit 2024




