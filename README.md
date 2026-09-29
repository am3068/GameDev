# GameDev

Unity game development project built with **Unity 2023.1.22f31**.

**Repository:** [github.com/am3068/GameDev](https://github.com/am3068/GameDev?utm_source=chatgpt.com)

## Requirements

* [Unity 2023.1.22f31](https://unity.com/releases/editor/archive)
* Git
* Git LFS

## Getting Started

### Clone the repository

```bash
git clone https://github.com/am3068/GameDev.git
cd GameDev
```

### Set up Git LFS

```bash
git lfs install
git lfs pull
```

Then open the project in **Unity Hub** using:

```text
Unity 2023.1.22f31
```

Unity will automatically regenerate local project files and caches such as `Library/`.

## Project Structure

```text
GameDev/
├── Assets/
├── Packages/
├── ProjectSettings/
├── .gitignore
├── .gitattributes
└── README.md
```

### Assets

The `Assets/` folder contains the project's Unity assets, scripts, scenes, prefabs, materials, textures, models, audio and other game content.

### Packages

The `Packages/` folder contains the Unity Package Manager configuration for the project.

### ProjectSettings

The `ProjectSettings/` folder contains the Unity project configuration.

## Git & Git LFS

This project uses **GitHub** for source control and **Git LFS** for large binary assets.

Git LFS is used for appropriate large assets such as:

* 3D models
* Textures
* Photoshop files
* Audio
* HDR/EXR files
* Fonts

To see which files are currently managed by LFS:

```bash
git lfs ls-files
```

To download LFS files after cloning:

```bash
git lfs pull
```

## Unity Version Control Settings

The project should use:

**Edit → Project Settings → Editor**

* **Version Control:** Visible Meta Files
* **Asset Serialization:** Force Text

Unity `.meta` files are tracked by Git and should not be deleted or ignored.

## Files Not Tracked

Unity-generated files and local editor caches are excluded through `.gitignore`.

Examples include:

```text
Library/
Temp/
Obj/
Logs/
UserSettings/
```

These folders are generated locally by Unity and should not be committed to the repository.

## Development Workflow

Before starting work:

```bash
git pull
git lfs pull
```

After making changes:

```bash
git status
git add .
git commit -m "Describe your changes"
git push
```

Keep commit messages short and descriptive.

Example:

```bash
git commit -m "Add player movement"
```

## Important Unity Notes

### Meta files

Do not manually delete `.meta` files for assets that are already part of the project. Unity uses the GUIDs stored in `.meta` files to maintain references between assets.

### Generated folders

Do not manually commit Unity's generated folders such as `Library/`, `Temp/`, or `Obj/`.

### Large files

Large binary assets should follow the project's `.gitattributes` configuration and be stored using Git LFS where appropriate.

## Build

Build the project using Unity's build settings:

**File → Build Settings**

Local build output should not be committed unless specifically required by the project.

## License

Add the project's license information here if/when a license is chosen.
