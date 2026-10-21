# Game Design Doc

## Game Title

Untitled Survival Game

## 1.1 Planned Features

### 1. Movement System — REQUIRED

- Move forward and backward.
- Rotate left and right.
- Strafe left and right.
- Attack.
- Defend.

  Additional : Player can jump, sprint.

### 2. Environment and Biomes — REQUIRED

- At least two parts of an environment with different aesthetics (two biomes), in the same or different levels.
- Day/night cycles.
- Biomes may be procedurally generated with different resources available and creatures spawning.

### 3. Inventory System — REQUIRED

- The player collects resources, which go into the player's inventory.
- The player can choose when to use collected resources.

Subfeatures:
- **3.1** Collecting items.
- **3.2** Storing items.
- **3.3** Accessing items via player controls.
- **3.4** Displaying items in the UI.
- **3.5** Using items.
- **3.6** Defining specific items to collect, use and combine.

### 4. Player Needs — REQUIRED

- The player has basic needs to satisfy, such as hunger and health.
- Some resources increase these values.
- Hunger decreases over time.
- Health decreases during combat with enemy creatures.
- If either hunger or health reaches zero, the player loses.

### 5. Survival Win Condition — REQUIRED

- The player wins after surviving the required amount of in-game days.

### 6. Crafting System — REQUIRED

- The player can craft items by combining specific resources.
- Crafted items satisfy needs more effectively or help with combat.

### 7. Creature AI — REQUIRED

- Creatures in the environment are controlled by AI.
- Some creatures are aggressive towards the player.

### 8. Combat System — REQUIRED

- The player can craft weapons and armour to fight or protect themselves against enemy creatures.
- Different types of weapons and armour are available, with varying effectiveness.
- Upgrades are available.
- The more a player uses a specific weapon or armour, the better they become at using it.

### 9. Increasing Survival Challenge — REQUIRED

- The survival challenge increases periodically.
- Player needs decay faster as the challenge increases.
- More threats emerge from the environment.

## 1.2 Additional Features

The following are potential additions beyond the required features. They will be prioritised according to available development time.

- **A1.** Main menu and pause menu.
- **A2.** Save and load functionality.
- **A3.** Additional creature types and behaviours.
- **A4.** Additional crafting recipes and equipment.
- **A5.** Additional visual effects, animations and audio.

## 1.3 Changes to Required Features

No changes to the required features are currently proposed.

Any proposed changes to required features will be recorded here and marked **UNAPPROVED** until approved by the allocated member of staff. Once approval is received, the status will be changed to **APPROVED**.

## 1.4 Core Features for the Prototype

The prototype will prioritise the smallest playable version of the required survival experience.

1. Player movement and basic attack/defend controls.
2. Health and hunger systems with loss conditions.
3. Resource collection, inventory storage, UI and item use.
4. Two visually distinct biomes and a basic day/night cycle.
5. One AI-controlled aggressive creature and basic combat.
6. Basic crafting using specific resources.
7. A survival-day counter and win condition.
8. A basic increase in difficulty over time.

Advanced equipment upgrades, weapon and armour proficiency, extensive crafting and procedural generation will be considered after the core gameplay is functional.

## 1.5 Feature Dependencies and Development Order

1. Implement player movement and controls.
2. Implement health, hunger and loss conditions.
3. Implement resource collection and inventory management.
4. Implement item use and basic crafting.
5. Create the two biomes and day/night cycle.
6. Implement creature AI and combat.
7. Implement survival-day tracking and the win condition.
8. Add increasing difficulty and expand equipment progression.

## 1.6 Team Responsibilities

Each feature will have one team member primarily responsible for completing and integrating it. Other members may collaborate.

| Feature | Responsible team member |
|---|---|
| Movement and controls | To be assigned |
| Environment and biomes | To be assigned |
| Inventory and resource collection | To be assigned |
| Player needs and survival progression | To be assigned |
| Crafting, weapons and armour | To be assigned |
| Creature AI and combat | To be assigned |
| Integration and testing | To be agreed by the group |

## 1.7 Design Review

The group will review this document regularly throughout development.

- Confirm that all group members agree with the planned design.
- Keep the prototype achievable within the available time.
- Prioritise high-risk and essential features.
- Record design changes and obtain staff approval for changes to required features where necessary.
- Update this document as the game develops.
