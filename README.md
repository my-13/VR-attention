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
Download and install SteamVR (Requires Steam and Steam account)
Download and install VIVE Business Streaming

## Usage

# For VIVE Focus 3:
Open VIVE Buisiness Streaming on PC
Open SteamVR on PC
Open VIVE Business Streaming on VR
Plug in headset using long cable to PC
Plug in Eye Tracking module to headset using short cable. 
Open Unity Project, and open the MainScene scene.
Make sure that you are in SteamVR mode on the headset. This can be confirmed by clicking the menu button on the left hand controller. 
If in SteamVR mode, press play in the Unity Editor. 
If not in SteamVR mode, make sure that SteamVR is running and has all modules enabled.


## File Explination

GameManager.cs - Handles the entire "game" and globally just manages the world. Passes interactions towards trials
OrientationTrials.cs - A static class that handles all the logic and variables for the trials. It does not only handle with line orientation, but also grabbing input

## Directories

# ./Scenes/Config
Holds all Config .cs files
ConfigOptions.cs - Holds all OrientationBlockConfigs, and uses ProcedureConfig to generate an OrientationBlockConfig that has the correct data
ProcedureConfig.cs - Reads the procedure file, stores procedure file as string, and also saves which trial the user is on (This should be the only working track of current trial and current block that should be updated)
OrientationBlockConfig.cs - Object meant to store block data. Unfortunately, some variables are unused, or had to be ignored when adding procedure files.

# ./Scenes/Materials
Shaders. 

# ./Scenes/Models
Meshes that aren't supported by unity's default meshes

# ./Scenes/Objects
Prefabs for any object that will be summoned. Incldues grab-able items, and the check-mark

## Builds
Currently, builds do not work. They are built, but potentially due to reading files, there is a lack of a procedure file for the build. 
This can potentially be fixed using Unity's IO file reader instead of C#'s system IO

## License

MIT License @ 2024