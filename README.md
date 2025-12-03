# DCGO-Card-Scripts

DCGO-Card-Scripts is a collection of scripts and card effect definitions for the Digimon Trading Card Game. This repository organizes card effects, game logic, and supporting scripts to facilitate the development and management of card sets and gameplay mechanics.

## Project Structure

- **CardEffect/**: Contains folders and .meta files for each card set (e.g., BT1, EX1, ST1, etc.). Each set folder includes scripts and resources specific to that set's cards.
- **Scripts/**: Core C# scripts that implement game logic, card effects, controllers, and utilities. Key files include:
    - `AttackProcess.cs`: Handles attack logic and resolution.
    - `AutoProcessing.cs`: Automates certain game processes.
    - `BattleMode.cs`: Manages different battle modes.
    - `CardController.cs`, `CardObjectController.cs`: Control card behaviors and interactions.
    - `CardEffectFactory.cs`, `CardEffectCommons.cs`, `CardEffectInterfaces.cs`: Define and manage card effects and their interfaces.
    - Additional scripts for UI, sound, and other game features.

## Getting Started

1. Clone the repository:
   ```powershell
   git clone https://github.com/DCGO2/DCGO-Card-Scripts.git
   ```
2. Open the project in your preferred C# IDE along with Unity Editor. *Requires Unity and .NET SDK installed*


## Contributing

Contributions are welcome! Please submit issues or pull requests for bug fixes, new features, or card effect scripts.

## Discord Link

Join our community on [Discord](https://discord.gg/EcjZaD4jhs) for discussions, support, and updates

## Glossary

- **Card Effect**: A script or function that defines the behavior or special abilities of a card during gameplay.
- **Set**: A collection of cards released together, organized in folders such as BT1, EX1, ST1, etc.

## Additional Notes

- Card effect scripts are organized by set for easy management and expansion.
- The project is designed to be modular, allowing new card sets and effects to be added with minimal changes to core logic.
- For more information, refer to documentation within the Scripts/ folder or contact the project maintainers.
