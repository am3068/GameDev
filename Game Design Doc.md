# Game Design Document

**Game Title:** Untitled Survival Game  
**Genre:** 3D Survival, Action and Crafting  
**Game Engine:** Unity  
**Programming Language:** C#  
**Course:** ECS657U/ECS7003P – Multi-Platform Game Development

---

## 1. Game Overview

Untitled Survival Game is a 3D survival game in which the player wakes up in the middle of nowhere, trapped in a hostile dimension and must survive long enough to escape.

The player must explore the environment, collect resources, manage health and hunger, craft useful items and defend themselves against hostile creatures.

The game contains two distinct biomes, a forest and a desert, connected within the same world. Each biome contains different resources that encourage exploration. Rocks block the pathway between the two biomes, the player must overcome all obstacles.

A day/night cycle tracks the passage of time, while the survival challenge increases as the game progresses.

**Main Objective:** Survive until Day 4, when the escape portal unlocks, and reach the portal to escape.

The game is designed to encourage exploration, resource management, utilise movement and have strategic survival decisions.

---

## 2. Planned Game Features

### 2.1 Movement System — Required

The player can move forward and backward, rotate left and right, and strafe in both directions.

The controls will also support attacking and defending, with jumping and sprinting as additional movement features.

### 2.2 Environment and Biomes — Required

The game contains two visually different environments:

- **Forest:** Green terrain, trees, bushes and healing plants.
- **Desert:** Sandy terrain, rocks and crystal ores.

Both biomes are connected by a pathway and a few rocks to jump over, allowing players to explore them without changing scenes.

A day/night cycle changes the environment's lighting and tracks the number of in-game days.

Procedural generation and biome-specific creature spawning may be considered as further improvements.

### 2.3 Inventory System — Required

The player can collect resources and store them in an inventory.

The inventory system will support collecting, storing, accessing, displaying and using items.

Certain resources can also be combined through the crafting system.

### 2.4 Player Needs — Required

The player must manage two main survival needs:

- **Health:** Decreases when the player takes damage from hostile creatures.
- **Hunger:** Gradually decreases over time.

Collected resources can restore these values.

If either health or hunger reaches zero, the player loses.

### 2.5 Survival Win Condition — Required

The player must survive for a specified number of in-game days.

The current design uses Day 4 as the escape portal unlock point.

Once the portal becomes available, the player must reach it to complete the game.

### 2.6 Crafting System — Required

The player can combine specific collected resources to craft useful items.

Crafted items may restore survival needs more effectively or provide weapons and armour that help during combat.

### 2.7 Creature AI - Required
The game includes basic Enemy AI features which are listed below:
- The enemies hover within the environment.
- They detect, attack and cause damage to Player.
- Their health status also reduces when they get attacked by the Player.
- They fire projectiles towards the Player during encounter.
- Enemies that are killed by the Player respawn the next day within the gameplay.

### 2.8 Combat System — Required

The player can attack and defend against hostile creatures using weapons and armour.

The broader design includes different equipment types, varying effectiveness, upgrades and proficiency improvements through repeated use.

Basic combat will be prioritised for the prototype.

### 2.9 Increasing Survival Challenge — Required

The game becomes more difficult as the player survives longer.

This may involve faster decreases in survival needs, more hostile creatures or additional environmental threats.

The aim is to increase pressure as the player approaches the escape objective.

---

## 3. Core Prototype Features

The prototype will prioritise the essential mechanics needed to create a playable survival experience.

1. Player movement and basic combat controls.
2. Health and hunger systems with loss conditions.
3. Resource collection, inventory storage and item use.
4. Two distinct biomes and a day/night cycle.
5. At least one aggressive AI-controlled creature.
6. Basic crafting and combat functionality.
7. A survival-day counter and escape portal win condition.
8. A basic increase in difficulty over time.

Advanced equipment upgrades, extensive crafting and procedural generation will be considered after the core mechanics are functional.

**Prototype Goal:** Create a playable game that demonstrates the main survival systems working together.

---

## 4. Environment and Technical Design

### 4.1 Forest and Desert

The forest and desert are created within the same Unity scene using GameObjects, materials, prefabs and colliders.

The forest contains trees, bushes and healing plants, while the desert contains rocks and crystal ores.

A pathway connects the biomes to encourage exploration. Obstacles to encourage players to learn the movement mechanics.

The different resources are intended to give each biome a distinct gameplay purpose and difficulty.

### 4.2 Day/Night Cycle

The `DayNightCycle` C# script manages the passage of time.

It rotates the directional light, adjusts environmental lighting and tracks the current in-game day.

The day counter is also used by the escape portal system.

### 4.3 Escape Portal

The escape portal provides the game's main objective.

The `EscapePortalController` script references the day/night system and checks whether the required day has been reached.

The portal is intended to remain unavailable until Day 4, after which the player can use it to escape.

### 4.4 Resource Collection

Healing plants and crystal ores are placed in their respective biomes.

The intended collection system allows the player to interact with resources, add them to the inventory and use them for survival or crafting.

These interactions will be integrated with the group's inventory system.

---


## 5. Testing and Evaluation

The prototype will be tested to ensure that the main gameplay systems work correctly and operate together.

Testing will focus on:

- Player movement and environmental interactions.
- Resource collection, inventory and crafting.
- Health, hunger and combat mechanics.
- Day/night cycle and survival-day tracking.
- Escape portal activation on Day 4.
- Win and loss conditions.
- Increasing survival difficulty.

Any issues discovered during testing will be recorded and addressed.

The group will evaluate whether the game is playable, the objective is clear and the main features provide the intended survival experience.

---

## 6. Design Review and Changes

The group will regularly review the game design to ensure the prototype remains achievable within the available development time.

Any proposed changes to required features will be recorded and marked **UNAPPROVED** until approval is received from the allocated member of staff, where required.

Once approved, the document will be updated to reflect the agreed changes.

The group will prioritise the core gameplay systems before implementing optional features.
