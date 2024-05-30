# 3DLayers: Bringing Layer-Based Color Editing to VR Painting

In this repository, we share the source code for the prototype implementation of the research paper:

"3D-Layers: Bringing Layer-Based Color Editing to VR Painting", Emilie Yu, Fanny Chevalier, Karan Singh and Adrien Bousseau, ACM Transactions on Graphics (SIGGRAPH) - 2024

This is a Unity project that implements a simple VR application compatible with Quest 2/3/Pro headsets. The project features:

- A VR app with basic 3D painting features (painting tube strokes, stroke deletion and transformation, color palette, undo/redo).
- A UI in the VR app to create, paint in, and edit shape and appearance layers, as described in the 3DLayers paper. We have a basic menu UI for users to visualize and navigate in the layer hierarchy.
- A basic in-Unity visualizer for paintings created with our system. It enables users to view and render still frames or simple camera path animations. We used it to create all results in the paper/video.

If building on this work, please consider citing our paper:

```
@Article{YCSB24,
  author       = "Yu, Emilie and Chevalier, Fanny and Singh, Karan and Bousseau, Adrien",
  title        = "3D-Layers: Bringing Layer-Based Color Editing to VR Painting",
  journal      = "ACM Transactions on Graphics (SIGGRAPH Conference Proceedings)",
  number       = "4",
  volume       = "43",
  month        = "July",
  year         = "2024",
  url          = "http://www-sop.inria.fr/reves/Basilic/2024/YCSB24"
}
```

## Requirements

- Quest 2/3/Pro headset with AirLink or Link cable (a good USB-C to USB-A cable in a USB 3.0 port should work). This app is well suited to use sitting down, so a short cable is usually enough.
- Compatible Windows PC: https://www.meta.com/help/quest/articles/headsets-and-accessories/oculus-link/requirements-quest-link/
- Meta Quest Link software (also called "Oculus PC app")
- Unity Hub

## Setup

- Clone the project and pull the submodule (color picker)

  ```bash
  git clone git@github.com:graphdeco-inria/3dlayers.git
  cd 3dlayers
  git submodule update --init
  ```

- Projects -> Add project from disk -> Choose the location where you cloned the repo

- Unity will prompt for installation of the correct version (2020.3.44f1). It is also useful to install Visual Studio at this point if you do not have it. VS will help with developing and debugging C# scripts for Unity.

- Load TextMeshPro Essentials (otherwise the text will show as pink squares): `Window -> TextMeshPro -> Import TMP Essential Resources`

## Loading Quill paintings

### Working with our example scenes

- Download the example scenes from [TODO upload scenes on some server]
- Unzip the example scenes in `Assets/Resources/Preload` (there should be for each scene a `fbx` and a `json` file, as well as corresponding Unity meta files that define how the files are interpreted by the Unity editor => the meta file for the `fbx` file defines Import Settings and is quite important)
- Open `MainScene` (in `Project` panel: `Assets -> Scenes -> Double click MainScene`)

### Importing your own Quill scenes (basic)

Our app is limited in painting features, so for more complex scenes it can be easier to create most of the geometry in Quill. A Quill painting can be imported with the following process:

- Organize the scene in Quill layers, where each layer corresponds to roughly a single object or kind of object in the scene. Each Quill layer will be made into one of our "shape" layer.
- Export Quill scene
  - FBX format
  - Linear Color Space
  - Export Meshes (ticked)
  - Bake Transforms (ticked)
- Drop the fbx file in `Assets/Resources/Preload`
- `Import Settings` need to be changed, this can be done by selecting the fbx file asset in Unity and looking at the Inspector panel. We provide a preset: click the preset icon to the top right of the Inspector panel, and choose `QuillFBXImporterPreset`. Alternatively you can try to reproduce settings from the example scenes we provide. The `Scale Factor` might need to be adjusted depending on the scale you worked in, in Quill
- Follow instructions to [select that scene to be loaded in the app](#choosing-the-scene-to-load)

### Importing your own Quill scenes with distinct brush stroke entities

The issue with the above workflow is that Quill scenes imported that way will have all brush strokes from a Quill layer "baked" as one entity. This means that if you have in Quill a layer `Trees` with multiple tree trunk strokes, once imported into our app you will not be able to individually select, transform, copy or delete individual tree trunk strokes. They will all be "fused" into one entity, in our app.

TODO: add Blender script

We provide a simple Blender script that solves that problem by editing the fbx file to separate each disconnected component into its own object, and re-export the fbx. The script also creates a json file that keeps track of which layer contains each object (so that layers are re-imported correctly in our app). To run it:

```
blender -b -P preprocess_quill_fbx.py -- -i <path to the FBX file> [-o <output folder path>]

# We are running Blender in background mode (without the UI), see the official Blender doc: https://docs.blender.org/api/current/info_tips_and_tricks.html#use-blender-without-it-s-user-interface
# Note: Depending on your setup you might have to enter the full path to the Blender executable instead of "blender"
```

Next drag both the new fbx file and json file into the `Assets/Resources/Preload` folder and proceed as in the previous scenario.

## Changing handedness
Set your preferred dominant hand in [TODO]. The dominant hand chosen here will be the one of any executables built, we did not create a UI to change handedness in the executable.

## Choosing the scene to load
The scene that will be loaded on `Play` and in a built executable can be switched in the Unity Editor.
In `MainScene`, open chilren to the `Canvas` gameobject, and select `Layers` gameobject. In `LayerManager` component, you can switch the `File Type` and specify the `Input File Name`.

These parameters affect loading in the following way:
- `File Type`:
    - `FBX`: loads the fbx/json files pair with a matching name based on the value of `Input File Name`
    - `Session`: loads a session history file with a matching path based on the value of `Input File Name`
    - `Latest`: if any previous session for this scene was saved, this will load the most recent one, otherwise it will load the matching fbx/json files. It relies on the fact that logs for a specific scene get saved in a folder matching the scene name, when clicking the `Save` button in the VR app.
- `Input File Name`: the scene name (eg `Desk-diorama`), or the session log file name. Note that if you're specifying a session log, you can also set this as being a relative path to `Assets/SessionData~`, eg `Desk-diorama/2024-05-29-17-31-07_session` would be a valid value for this parameter, given that that session log exists.

## Using the VR painting app

Please refer to the video tutorial: https://youtu.be/-NJEbUf1-vA

You can also refer to a cheatsheet for controllers button mappings in the VR app, above the non-dominant hand.

Clicking `Save` in the VR painting app creates log files in `Assets/SessionData~/[Scene Name]`. There is both a json and binary log file, the json log files are purely for practical data analysis, Unity will always read the binary `.dat` log files to reload sessions.

## Visualizing 3D paintings and past painting sessions

- Open the scene `VisScene`
- Select the session log to visualize in `Canvas > Layers > Layer Manager`, by specifying `Input File Name`. The `File type` should be set in `Latest` or `Session` here
- Click `Play`
- This should load the session in the state it was at save time
- Available visualization possibilities:
  - Move camera around the scene (left click to orbit, right click to pan, wheel to zoom)
  - Visualize the progression of actions in the session by using the `Session Visualizer` component. When the game panel is in focus, you can use the left and right arrow to go to the previous/next logged action.
  - Render the current frame with a transparent background and full resolution: `Layer Renderer` component, click `Render current frame`
  - Render a *super basic* camera path animation. Go to the `Main Camera` gameobject and find the `Camera Path Animator` component. By moving the camera to the desired position, and hitting `Set Start` or `Set End`, you can set the beginning and end keyframes for the path. Clicking `Animate` will render all frames by interpolating those keyframes. Make sure `Render frames` is toggled on once you are ready to actually render the frames.
  - While in `Play` mode, you can inspect the `Layers` gameobject in the Scene Hierarchy. Each child of the `Layers` gameobject is a Layer. Any gameobject can be toggled on/off by opening the Inspector for that gameobject and clicking the toggle next to the gameobject name at the top. For a `ClippedLayer` we also provide some basic visualization options through boolean properties in the inspector: `Debug Highlight` renders the full 3D strokes and `Debug Focus` renders only the color contribution from that layer. These two modes work best when only one `ClippedLayer` gameobject is active.

## Where to find code described in the paper

TODO

## Credits

- The color picker was adapted from judah4's Unity color picker: https://github.com/judah4/HSV-Color-Picker-Unity
- The tubular strokes mesh generation was adapted from mattatz's tubular mesh generator code: https://github.com/mattatz/unity-tubular

## Contact

email: emiliextyu@gmail.com