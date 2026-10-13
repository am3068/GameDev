# Game Design Doc

## Game Title
Untitled Survival Game

## 1.1 Planned Features

### 1. Player Movement and Controls — REQUIRED
- **1.1** Move forward and backward.
- **1.2** Rotate left and right.
- **1.3** Strafe left and right.
- **1.4** Attack enemies.
- **1.5** Defend against enemy attacks.

### 2. Environment and Biomes — REQUIRED
- **2.1** Create at least two visually distinct biomes.
- **2.2** Provide different aesthetics for each biome.
- **2.3** Implement a day/night cycle.
- **2.4** Place resources throughout the environment.
- **2.5** Support creatures spawning in the environment.
- **2.6** Allow biomes to be part of the same or separate levels.
- **2.7** Procedural biome generation is a potential implementation option.

### 3. Inventory and Resource Collection — REQUIRED
- **3.1** Allow the player to collect resources.
- **3.2** Store collected resources in the player's inventory.
- **3.3** Display collected items and quantities in the UI.
- **3.4** Allow the player to access and select inventory items using controls.
- **3.5** Allow the player to use items from the inventory.
- **3.6** Define resources that can be collected in each biome.
- **3.7** Consume resources when they are used or crafted into items.

### 4. Player Needs and Survival — REQUIRED
- **4.1** Track the player's hunger.
- **4.2** Decrease hunger over time.
- **4.3** Track the player's health.
- **4.4** Reduce health when the player takes damage in combat.
- **4.5** Allow specific resources to restore hunger or health.
- **4.6** Trigger a loss condition if hunger reaches zero.
- **4.7** Trigger a loss condition if health reaches zero.

### 5. Survival Progression and Win Condition — REQUIRED
- **5.1** Track the number of in-game days survived.
- **5.2** Advance days through the day/night cycle.
- **5.3** Define the number of days required to survive.
- **5.4** Trigger a win condition when the required number of days is reached.
- **5.5** Increase the survival challenge periodically.

### 6. Crafting System — REQUIRED
- **6.1** Define recipes using specific resources.
- **6.2** Allow the player to combine resources to craft items.
- **6.3** Consume the required resources when crafting.
- **6.4** Create items that restore hunger or health.
- **6.5** Create items that improve combat effectiveness.
- **6.6** Display available recipes and crafting results to the player.

### 7. Creature AI — REQUIRED
- **7.1** Implement AI-controlled creatures.
- **7.2** Allow some creatures to be aggressive towards the player.
- **7.3** Allow aggressive creatures to detect and approach the player.
- **7.4** Allow aggressive creatures to attack the player.
- **7.5** Allow creatures to spawn within the environment.
- **7.6** Increase environmental threats as survival progresses.

### 8. Combat, Weapons and Armour — REQUIRED
- **8.1** Allow the player to fight enemy creatures.
- **8.2** Allow the player to craft weapons.
- **8.3** Allow the player to craft armour.
- **8.4** Implement different weapon types with varying effectiveness.
- **8.5** Implement different armour types with varying protection.
- **8.6** Allow weapons and armour to be upgraded.
- **8.7** Improve the player's proficiency with weapons and armour through use.

### 9. Increasing Survival Difficulty — REQUIRED
- **9.1** Increase hunger decay periodically.
- **9.2** Introduce more environmental threats over time.
- **9.3** Increase the number or frequency of hostile creature encounters.
- **9.4** Balance difficulty progression against the required survival duration.

## 1.2 Additional Features

The following features are planned extensions and will be prioritised after the core survival gameplay is functional.

- **A1.** Main menu and pause menu.
- **A2.** Sound effects and background music.
- **A3.** Save and load functionality.
- **A4.** Additional biomes, resources and creature types.
- **A5.** More crafting recipes, weapon types and armour types.
- **A6.** Procedurally generated environments.
- **A7.** Improved UI, animations and visual effects.

## 1.3 Planned Changes to Required Features

No changes to the required features are currently proposed.

Any future changes to required features will be discussed within the group and submitted to the allocated member of staff for approval where necessary. Changes will be recorded here as **UNAPPROVED** until approval is received, then updated to **APPROVED**.

## 1.4 Core Features for the Prototype

The initial prototype will prioritise a small, playable survival loop.

1. Player movement and basic combat controls.
2. Health and hunger systems, including loss conditions.
3. Resource collection and a functional inventory.
4. Using food to restore hunger.
5. Two visually distinct biomes with a basic day/night cycle.
6. One aggressive enemy with basic AI and combat.
7. A survival-day counter and a win condition.
8. Basic crafting for at least one useful item.

These features will be developed first to test whether the core survival gameplay is enjoyable and achievable within the prototype schedule.

Advanced crafting, weapon proficiency, upgrades, procedural generation and extensive difficulty scaling will be developed later if time allows.

## 1.5 Dependencies and Development Order

1. Implement player movement and basic controls.
2. Implement health, hunger and win/loss conditions.
3. Implement resource collection and inventory management.
4. Implement food consumption and basic crafting.
5. Create two biomes and implement the day/night cycle.
6. Implement enemy AI and combat.
7. Connect survival progression to the win condition and increasing difficulty.
8. Expand crafting, equipment, upgrades and additional content.

## 1.6 Team Responsibilities

Each feature will have one team member responsible for completing and integrating it. Other team members may contribute where needed.

Team member names and responsibilities will be agreed upon and recorded here.

| Feature | Responsible team member |
|---|---|
| Player movement and controls | To be assigned |
| Inventory and resource collection | To be assigned |
| Environment and biomes | To be assigned |
| Survival needs and progression | To be assigned |
| Creature AI and combat | To be assigned |
| Crafting, weapons and armour | To be assigned |
| Integration and testing | Shared responsibility |

## 1.7 Design Review

The group will review this document regularly as development progresses.

- Confirm that all group members agree with the planned features.
- Prioritise features according to risk, dependencies and available development time.
- Keep the prototype achievable within the available schedule.
- Record design changes and obtain staff approval for changes to required features where necessary.
