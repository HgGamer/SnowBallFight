# Unity New Input System Setup

This project has been updated to use Unity's new Input System instead of the legacy Input Manager. This README explains how to set up and use the new input system.

## Required Package Installation

1. Open the Unity Package Manager (Window > Package Manager)
2. Select "Unity Registry" from the dropdown
3. Search for "Input System"
4. Click "Install" to add the Input System package to your project
5. When prompted, select "Yes" to enable the new input system and restart the editor

## Project Configuration

The project uses the following input system components:

1. **PlayerInputActions.cs** - Auto-generated C# file that defines the input actions and bindings
2. **PlayerController.cs** - Updated to implement the InputSystem interfaces and handle input events

## Input Actions

The following input actions are configured:

- **Move** - WASD/Arrow keys on keyboard, Left Stick on gamepad
- **Look** - Mouse movement, Right Stick on gamepad
- **Fire** - Left mouse button, gamepad button (South/X/A)

## Implementation Notes

1. The PlayerController implements the `PlayerInputActions.IPlayerActions` interface to receive input callbacks
2. Input events are processed through callback methods:
   - `OnMove` - Handles movement input
   - `OnLook` - Handles rotation input
   - `OnFire` - Handles fire/action button input
3. Input is initialized in the `Awake` method and enabled/disabled in the `OnEnable`/`OnDisable` methods

## Configuring Input Actions in the Editor

While the input actions are defined in code for this project, you can also create and edit them visually:

1. In the Project window, right-click and select Create > Input Actions
2. Double-click the created asset to open the Input Action editor
3. Configure your actions, bindings, and control schemes
4. Click "Generate C# Class" to create the code file

## Testing Controller Support

1. Connect a gamepad controller to your PC
2. Launch the game
3. The game should now respond to:
   - Left stick for movement
   - Right stick for rotation
   - South button (A on Xbox, X on PlayStation) for firing/crafting snowballs

## Troubleshooting

If input isn't working correctly:

1. Check the Unity console for any input-related errors
2. Verify that the Input System package is installed and active
3. Ensure the PlayerInputActions class is generated correctly
4. Make sure the PlayerController properly implements the interface and enables the input actions 