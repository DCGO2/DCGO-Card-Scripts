# DCGO-Card-Scripts

DCGO-Card-Scripts is a collection of scripts and card effect definitions for the DCGO card game. This repository organizes card effects, game logic, and supporting scripts to facilitate the development and management of card sets and gameplay mechanics.

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
   git clone <repository-url>
   ```
2. Open the project in your preferred C# IDE.
3. Ensure you have the required dependencies and game engine Unity.
4. Build and run the project to test card scripts and effects.

## Contributing

Contributions are welcome! Please submit issues or pull requests for bug fixes, new features, or card effect scripts.

## License

Specify your license here (e.g., MIT, GPL, proprietary).

## Glossary

- **Card Effect**: A script or function that defines the behavior or special abilities of a card during gameplay.
- **Set**: A collection of cards released together, organized in folders such as BT1, EX1, ST1, etc.
- **Script**: A C# file that implements game logic, card interactions, or utility functions.
- **Meta File**: Unity-specific file (.meta) that stores metadata about assets and scripts.
- **Controller**: A script that manages the state, behavior, or interactions of cards and game objects.
- **Factory**: A design pattern used to create card effects or other objects dynamically based on type or configuration.
- **Commons**: Shared logic or utilities used across multiple scripts or card effects.

## Additional Notes

- Card effect scripts are organized by set for easy management and expansion.
- The project is designed to be modular, allowing new card sets and effects to be added with minimal changes to core logic.
- For more information, refer to documentation within the Scripts/ folder or contact the project maintainers.
