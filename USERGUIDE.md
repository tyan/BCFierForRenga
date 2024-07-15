## Intro ##

BCFierForRenga is an extendible and Open Source BCF client. Like IFC is the open standard for Building Information models, BCF (also developed by buildingSMART) is the open standard for Building Issues. BCFier is a set of plugins and standalone apps (modules) that handle BCF and integrate directly with BIM tools.

Currently "BCFierForRenga" is composed of:

- Renga plugin
- Standalone Windows Viewer

## General Usage ##


### BCF Report ###

A “BCF report” or “BCF file” is a file containing one or more issues of a project. It is store on disk with the extension .bcf.

To create a new empty report just fire up BCFierForRenga and click on “New”, then you can start adding issues.

BCFier allows you to have more than one BCF report open at the same time, and you can switch by clicking on the blue tabs. To open one or more BCF files just use the main menu button or Drag&Drop them on the main interface.

BCFier supports BCF files version 1.0 and 2.0. Saved files will always use the latest version of BCF.

###Issues To add a new Issue to a report, just click the “Add Issue” button, a new empty Issue will be generated. You can now set a title and description and start adding Views and Comments.

### Views ###

A View is the combination of a snapshot (just an image) and a viewpoint (the 3D information of the current view as camera position and elements visibility/selection status), BCF 2.0 introduced support for multiple views per issue and so does BCFier 2.

When adding a new View from BCFier Standalone Viewer no viewpoint will be added in the view therefore it will not contain 3D information.

2D Views are not a feature part of BCF. Although the BCFier addins for Revit will support creation of 2D Views (because of the numerous requests) by storing the ID of the view. Therefore it will not work with other tools that support BCF (many tools as Solibri or Navisworks don’t even have 2D Views).

### Comments ###

Comments can either be general issue comments or be attached to a specific view.

You can add your user name and the available statuses from the BCFier Settings.

Web urls will automatically render as clickable, while if you want to make a local or network absolute path clickable, just wrap it in square brackets [].

Examples:

[C:\Projects\Collaboration\MyProject.rnt]

## Renga

BCFier for Renga are accessible via the Primary Panel.

These will let the user create BCF Issues for the current view in the model.

### Components ###

As already mentioned views contain information on the current scene and therefore elements visibility and selection status (components).

If most elements in the scene are hidden, BCFier will only store a list of the visible elements, otherwise a list of the hidden ones, this to improve performance.

All selected items will always be stored in the component list as well.

The full list of components contained in a view can be see by clicking the components icon after the view has been created.

Please note that the visibility and selection of the components relies on their GUID (Global Unique ID), and will not be possible if this changes (or is lost) passing the model from a tool to another.

## Installation ##

Installation is done by running BCFier.exe that can be downloaded from bcfier.com. Follow the installation wizard and select the modules you want to install.

### Requirements ###

.NET Framework 4.8

### Uninstallation ###

Uninstall using Programs and Features from Windows or the link in Start Menu > All Programs > BCFier > Uninstall. To manually remove a user settings file delete %localappdata%\BCFier\settings.config.
