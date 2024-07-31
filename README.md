# VR Attention

A virtual reality platform developed to record the effect of visual distractors on visuokinematic data. 
If I had more time, I would rewrite a codebase


## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Installation

Download and install Unity LTS 2022.3 (2022.3.39f1)
Download and install SteamVR (Requires Steam)
Open the "Main Scene" file 

## Usage

# For VIVE Focus 3:
Open VIVE Buisiness Streaming
Open SteamVR
Plug in headset using long cable to PC
Plug in Eye Tracking module to headset using short cable

## File Explination

GameManager.cs - Handles the entire "game" and globally just manages the world. Passes interactions towards trials
OrientationTrials.cs - A static class that handles all the logic and variables for the trials. It does not only handle with line orientation, but also grabbing input

## Directories

# ./Scenes/BasicScene
Unused. Unsure if required.

# ./Scenes/Config
Holds all Config .cs files
ConfigOptions.cs - Holds all OrientationBlockConfigs, and uses ProcedureConfig to generate an OrientationBlockConfig that has the correct data
ProcedureConfig.cs - Reads the procedure file, stores procedure file as string, and also saves which trial the user is on (This should be the only working track of current trial and current block that should be updated)
OrientationBlockConfig.cs - Object meant to store block data. Unfortunately, some variables are unused, or had to be ignored when adding procedure files.

# ./Scenes/InventoryAssets
Unused. Not required. Used to hold the inventory images, whenever the original system of selecting images was being made

# ./Scenes/Items
Unused. Not required. Used to be the objects inside Inventory, but that was removed. 

# ./Scenes/MainScene 
Item.cs - Unused. Used to be for inventory object.
ItemData.cs - Unused. Used to be for inventory object data.
ItemManager.cs - Unused. Used to handle the inventory objects being moved around
ItemSlots.cs - Unused. Used to be the slots the inventory would be placed in.
QuickColorMemory.cs - Unused. Used to be when I was going to recreate Dr. Adam's project
SliderTex


# ./Scenes/Materials
Shaders. 

# ./Scenes/Models
Meshes that aren't supported by unity's default meshes

# ./Scenes/Objects
Prefabs for any object that will be summoned. Incldues grab-able items, and the check-mark

## License

MIT License @ 2024