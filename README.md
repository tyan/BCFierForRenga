## Intro

BCFierForRenga is an extendible, opensource [BCF](https://github.com/BuildingSMART/BCF-XML) client. This project is a fork of [BCFier](https://github.com/teocomi/BCFier) project.

Currently BCFierForRenga is composed of the:
- Renga plugin
- Standalone Windows Viewer

**[Here](./USERGUIDE.html) you can find a Guide on how to use the installed version of BCFierForRenga**

## Getting Started

To get started fork the repo and download the latest version of the [Renga SDK](https://rengabim.com/sdk/).

### Structure

The core of BCFier is under `Bcfier`, it contains all the logic and UI that is used by all the different integrations (modules). All modules will reference that project and extend it adding specific commands for the software they are integrating with.

The control `Bcfier.UserControls.BcfierPanel` contains the logic and UI for the main panel, while `Bcfier.UserControls.BcfReportPanel` for each BCF opened inside the TabControl.

### Creating a new Module

Revit and NavisWorks plugins were removed from the repository, because I can not support them and develop BCFierForRenga without SDK. Renga SDK is open, thats why this project contains only Renga plugin for now.

But if you want to create a new plugin, for instance, an Achicad plugin, follow these steps:
- create a new project with the namespace `Bcfier.Archicad`
- reference the `Bcfier` project
- add the specific Archicad methods and structure to fire the plugin (like the Entry folder in the Renga plugin)
- create a main WPF window that contains the `Bcfier.UserControls.BcfierPanel`
- create a command for adding a new view (`data:Commands.AddView`), this will have to generate a BCF ViewPoint (see Renga plugin for reference)
- create a command for and opening a view (`data:Commands.OpenView`)
- extend the installer to copy these new dlls where needed

### Settings
The settings file is stored in `%localappdata%\BCFier\settings.config` so that it can be accessible by all modules.

The class that handles the settings file is under `Bcfier.Data.Utils.UserSettings`, and stores the file as a `ExeConfigurationFileMap` for easy management. The same class provides methods to automatically save/retrieve settings based on the UserControl name.

### Contact
Feel free to add your [suggestions and report issues](https://github.com/tyan/BCFierForRenga/issues), I'll try to answer all of them.

### License
GNU General Public License v3 Extended
This program uses the GNU General Public License v3, extended to support the use of BCFierForRenga as a Plugin of the non-free software RengaStandard  or RengaProfessional.
See <http://www.gnu.org/licenses/gpl-faq.en.html#GPLPluginsInNF>.

Copyright (c) 2013-2016 Matteo Cominetti, 2024 Evgenii Tyan

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/gpl.txt>.
