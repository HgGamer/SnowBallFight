# Controller Support Setup Instructions

This document explains how to configure Unity's Input Manager to work with the controller support added to the PlayerController script.

## Input Manager Configuration

1. In the Unity Editor, go to **Edit > Project Settings > Input Manager**
2. Add the following input axes to the Input Manager:

### Left Stick Horizontal
- Name: "LeftStickHorizontal"
- Descriptive Name: "Left Stick Horizontal Axis"
- Negative Button: (leave empty)
- Positive Button: (leave empty)
- Gravity: 0
- Dead: 0.19
- Sensitivity: 1
- Type: Joystick Axis
- Joy Num: Get Motion from all Joysticks
- Axis: X axis

### Left Stick Vertical
- Name: "LeftStickVertical"
- Descriptive Name: "Left Stick Vertical Axis"
- Negative Button: (leave empty)
- Positive Button: (leave empty)
- Gravity: 0
- Dead: 0.19
- Sensitivity: 1
- Type: Joystick Axis
- Joy Num: Get Motion from all Joysticks
- Axis: Y axis
- Invert: Yes

### Right Stick Horizontal
- Name: "RightStickHorizontal"
- Descriptive Name: "Right Stick Horizontal Axis" 
- Negative Button: (leave empty)
- Positive Button: (leave empty)
- Gravity: 0
- Dead: 0.19
- Sensitivity: 1
- Type: Joystick Axis
- Joy Num: Get Motion from all Joysticks
- Axis: 3rd axis (Joysticks and Scrollwheel)

### JoystickFire1
- Name: "JoystickFire1"
- Descriptive Name: "Joystick Button 0"
- Negative Button: (leave empty)
- Positive Button: joystick button 0
- Gravity: 1000
- Dead: 0.001
- Sensitivity: 1000
- Type: Key or Mouse Button

## Controller Parameter Tuning

You can adjust the following parameters in the PlayerController component in the Inspector:
- **Rotation Speed**: Controls how fast the player rotates when using the right stick (default: 120)
- **Right Stick Deadzone**: Minimum right stick movement needed before rotation is applied (default: 0.2)

## Testing Controller Support

1. Connect a gamepad controller to your PC
2. Launch the game
3. The game should now respond to:
   - Left stick for movement
   - Right stick for rotation
   - A button (Xbox) or X button (PlayStation) for firing/crafting snowballs

## Troubleshooting

If your controller isn't working properly:

1. Check that your controller is recognized by Windows
2. Try remapping buttons in Unity's Input Manager if your controller uses different button mappings
3. Adjust the deadzone values if you experience drift or oversensitivity
4. For Xbox controllers, ensure you have the appropriate drivers installed 