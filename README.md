# GameDev

Unity game development project using Unity 2023.1.22f1.

Repository:
https://github.com/am3068/GameDev

## Game Overview

This project is a 3D survival game developed in Unity. The player is placed in a world where they need to explore, collect resources and survive until they can escape. The environment consists of two different biomes, a forest and a desert, which the player can travel between. Each biome contains different objects and resources. The main aim of the game is to survive until Day 4, when the escape portal becomes available.

## How to Play

The player explores the environment, collects resources and tries to survive for several in-game days. Once Day 4 is reached, the escape portal unlocks and the player can use it to complete the game.

### Controls

- Movement: wasd to move around
- Camera: esc to release camera
- Inventory: 0-9 in order to select an item slot
- Combat: left click on the mouse to hit enemies

## Game Features

### Forest and Desert Biomes

Two biomes have been created to give the player different areas to explore. The forest contains grass, trees, bushes and healing plants. The desert contains sandy ground, rocks and crystal ores. Both biomes are part of the same environment, with a pathway connecting them. The environments were built using Unity's basic 3D objects, materials and prefabs.

### Day and Night Cycle

A day/night cycle has been added to make the environment change over time. The system rotates the directional light and adjusts the lighting to represent different times of day. It also keeps track of how many in-game days have passed.

### Escape Portal

An escape portal has been created as the main objective of the game. The portal is designed to remain inactive for the first three days and unlock on Day 4. The portal uses the day counter from the day/night cycle to determine when it should become available.

### Resources

Healing plants have been added to the forest and crystal ores have been added to the desert.
These resources are intended to be used as part of the game's collection and survival mechanics.

### Enemy

- The Enemies for the prototype use Capsule Colliders and Rigidbody components to interact with the environment easily
  and carry out implementations such as avoiding being stuck or hitting other enemies and environmental assets.
- They also contain RayCastHit for Player and Obstacle detection.
- They also have enemy health and can also respawn when they are attacked by Player.
- They fire projectiles towards the PLayer when they fall within their radius detection.
- They are levitating capsule models that hover around within the gameplay.
- They can appear anywhere within the game.
- Player can hide behind desert rocks or move farther away from them to avoid getting hit.




## Development
The game is being developed using Unity and C#. GitHub is used to manage the project and allow group members to work on different features. The environment is built using Unity GameObjects, prefabs, materials and colliders. C# scripts are used for features such as the day/night cycle and escape portal.




## Testing

The following features need to be checked as part of testing the complete game:

- Player movement between the two biomes
- Day/night cycle and day counter
- Escape portal activation on Day 4
- Resource collection
- Interaction between the different gameplay systems


## Running the Game

1. Download or clone the repository.
2. Open the project through Unity Hub.
3. Use the Unity version listed in `ProjectSettings/ProjectVersion.txt`.
4. Open the main gameplay scene.
5. Press Play to run the game.


## AI Assistance and External Resources

ChatGPT was used as a learning and development support tool to help explain Unity concepts, C# programming and gameplay implementation approaches.



## Version Control

- Git and GitHub used for version control.
- Git LFS used for large binary assets.
- `.gitignore` excludes Unity-generated files.
- `.gitattributes` configures Git LFS.
- Unity `.meta` files are tracked.
- Asset Serialization is set to Force Text.
- Version Control is set to Visible Meta Files.

## Unity Project

Main project folders:

    Assets/
    Packages/
    ProjectSettings/

Generated folders such as `Library/`, `Temp/`, `Obj/`, `Logs/` and `UserSettings/` are excluded from Git.

## Team Workflow

- Team members commit their own work.
- Changes are committed incrementally.
- The repository is regularly kept up to date.
- Scenes and prefabs are coordinated to minimise merge conflicts.
- Unity Smart Merge can be used for Unity YAML conflicts.

## Git LFS

Large assets such as models, textures, audio, fonts and other binary files are managed through Git LFS.

Check LFS files:

    git lfs ls-files

## Development

Before working:

    git pull
    git lfs pull

After making changes:

    git add .
    git commit -m "Describe changes"
    git push
