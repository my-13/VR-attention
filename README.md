# VR Attention

A virtual reality platform developed to record the effect of visual distractors on visuokinematic data. 

"If I had more time, I would have programmed a shorter code base" - Tony

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Installation
Download and install Unity LTS 2022.3 (2022.3.56f1)
Download and install SteamVR (Free) This requires Steam to download and a Steam account
Download and install VIVE Business Streaming [https://business.vive.com/eu/solutions/streaming/]
Requires a USB-C 3.1 short cable, and ideally at least USB-C 3.0 long cable. 
Unity Plugins require for this project: 
VIVE OpenXR Plugin 2.5.1
OpenXR Plugin 1.14.3
Input System 1.11.2

## Usage

# For VIVE Focus 3:
* Open VIVE Buisiness Streaming on PC
* Open SteamVR on PC
* Plug in headset using long cable to PC
* Open VIVE Business Streaming on VR. You should see yourself in a room with screens on the walls. 
* Plug in Eye Tracking module to headset using short cable. This should show a new setting called Eye tracker.
* Go to Settings -> Input -> Calibrate Eye Tracker, and go through the process of calibrating the eye trackers 
* Open Unity Project, and open the MainScene scene.
* Make sure that you are in SteamVR mode on the headset. This can be confirmed by clicking the menu button on the left hand controller, opening a Steam interface 
* Ensure the participant has the headset in the correct position, that they will be tested in. 
* If in SteamVR mode, press play in the Unity Editor. 
* If not in SteamVR mode, make sure that SteamVR is running, go to settings and ensure all modules are enabled.


# General
Once your VR headset is set up, to run a trial, open the MainScene scene. There will be a node called "GameManager"
There will be a variable called "Participant ID" will be a unique identification code defined.
If the participant ID is "0000", then the next highest available participant ID will be selected. If not, then the participant ID will be overwritten.
Ensure the participant is wearing the VR headset, then you can hit the play button in the Unity Editor. and then they can read instructions. Researchers will be able to follow along by looking at the Game tab in the Unity Editor. 

# Notes:
In between having the headset being setup and the researcher pressing play, the participants will be in a SteamVR environment. Use a dedicated lab steam account.

procedure.txt is how the order of the procedures are defined. There are 4 seperate categories that can currently be modified, 
such as inputText (how the participants select their input), distractor (whether there exists a distractor), mainColor (whether the target color is primary or secondary), and horizontal (whether the target has a horizontal line or a vertical line). These are created to ensure that there is an equal distribution of distractors, so they are currently symetrical, but can be modified to test for different procedures.  

Despite documentation specifying USB 3.0, the eye tracking hardware on the VIVE Focus 3 seems to require at least a USB 3.1 Gen 2 cable.
Without the proper cable, the eye tracking hardware will randomly connect and disconnect, and will not be able to complete the calibration.

The participants will likely need to be taught a basic concept of how VR controllers are setup, as some terms are controller specific.
* Grabbing: Squeezing on controller, pressing the side with ring or pinky finger.
* Trigger: The trigger at the top of the controller, pressed by the index finger. 
* Buttons: The two A/X/Y/B buttons on each controller, pressed by the thumb.
* Joystick: The one stick next to the buttons, controlled using the thumb. 

## File Explanation
GameManager.cs - Handles the entire "game" and globally just manages the world. Passes interactions towards trials
OrientationTrials.cs - A static class that handles all the logic and variables for the trials. It does not only handle with line orientation, but also grabbing input

## Directories

### ./Scenes/Config
Holds all Config .cs files
ConfigOptions.cs - Holds all OrientationBlockConfigs, and uses ProcedureConfig to generate an OrientationBlockConfig that has the correct data
ProcedureConfig.cs - Reads the procedure file, stores procedure file as string, and also saves which trial the user is on (This should be the only working track of current trial and current block that should be updated)
OrientationBlockConfig.cs - Object meant to store block data. Unfortunately, some variables are unused, or had to be ignored when adding procedure files.

### ./Scenes/Materials
Shaders. 

### ./Scenes/Models
Game Meshes that aren't supported by unity's default meshes

###  ./Scenes/Objects
Prefabs for any object that will be summoned. Incldues grab-able items, and the check-mark

## Builds
I don't know how Unity handles file reading, so I do not hae instructions with setting up builds

## Running
To run the study, have the participant calibrate the eye tracking hardware. From the Lobby, select Settings. Select Inputs > Eye tracker. Make sure Eye tracker is turned on, and has a consistent connection. Select **Calibrate** and follow the onscreen instructions to complete the calibration process. 
Once the participant is calibrated, then the researcher can select a unique particiapnt ID, press the Play button on the MainScene scene, and then ensure the researcher is in the scene. Once in the scene, the participant will have a set of instructions that they can read, which will explain the project. 

## License
MIT License @ 2024
