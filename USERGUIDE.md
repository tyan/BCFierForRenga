# User Guide #


## BCF Report ##

A “BCF report” or “BCF file” is a file containing one or more issues of a project. It is store on disk with the extension .bcf.

To create a new empty report just fire up BCFierForRenga and click on “New”, then you can start adding issues.

BCFier allows you to have more than one BCF report open at the same time, and you can switch by clicking on the tabs. To open one or more BCF files click Open.

BCFier supports BCF files version 2.0.

### Issues ###

To add a new Issue to a report, just click the “Add Issue” button, a new empty Issue will be generated. You can now set a title and description and start adding Views and Comments.

### Views ###

A View is the combination of an image and a viewpoint (the 3D information of the current view as camera position and elements visibility/selection status). You can add multiple views in one issue.

When adding a new View from BCFier Standalone Viewer no viewpoint will be added in the view therefore it will not contain 3D information.

### Comments ###

Comments can either be general issue comments or be attached to a specific view.

You can add your user name and the available statuses from the BCFier Settings.

Web urls will automatically render as clickable, while if you want to make a local or network absolute path clickable, just wrap it in square brackets [].

Examples:

[C:\Projects\Collaboration\MyProject.rnt]

## Renga

BCFier for Renga is accessible via the Primary Panel.

These will let the user create BCF Issues for the current view in the model.

### Components ###

As already mentioned views contain information on the current scene and therefore elements visibility and selection status (components).

If most elements in the scene are hidden, BCFier will only store a list of the visible elements, otherwise a list of the hidden ones, this to improve performance.

All selected items will always be stored in the component list as well.

The full list of components contained in a view can be see by clicking the components icon after the view has been created.

Please note that the visibility and selection of the components relies on their GUID (Global Unique ID), and will not be possible if this changes (or is lost) passing the model from a tool to another.

## Installation ##

Installation of the plugin is done by copying the folder with the plugin to the %RengaInstallationFolder%/Plugins. Detailed instructions are [here](https://help.rengabim.com/en/index.htm#plugins.htm). BCFier for Windows does not need any installation.

### Requirements ###

.NET Framework 4.8, RengaProfessional/RengaStandard 7.4 or higher.

### Uninstallation ###

To uninstall the plugin simply remove it from Plugins folder. To manually remove a user settings file delete %localappdata%\BCFier\settings.config.
