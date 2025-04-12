# Spacewar! Commander – IAT267 University Project

## 1. The Project

The project is meant to simulate how it would feel like commanding a spaceship and fighting off adversaries. The project has a purposefully complex control scheme with a variety of control elements and indicators. The simulation requires the user to keep track of health, resource distribution, ammo, movement, and enemies. There are small in-simulation environment effects such as fog which also require the user to be located in the specific environment in the real world to keep the flashlight on. Movement is only possible in 3 degrees of freedom, meaning Pitch, Yaw (and technically Roll, as the user can rotate 360 degrees). Enemies are smart; they try to avoid, hide, and escape the user.

Aesthetically, the project was aiming to look like an old, rugged spaceship, as such there is a lot of rust and uncomfortable lights. While physical “real” externals for the simulation, the controller, have wires exposed and various circuit art drawn all over.

All visuals are done by our team, including modeling, painting, and texturing. Sounds were taken from freesound.org.

---

## 2. Code Explanation

### Arduino

Arduino board collects info from sensors and sends them using serial, and also receives serial input in the form of strings, which are then split up to determine what needs to be done for output. For example, it receives `a1abr`, that will mean that it will blink up the LED, activate the buzzer, and the servo motor. Lastly, Arduino is responsible for managing the servo with all delays, distances, etc.

### Python

A script that acts as a middleman between Arduino and Godot. Python receives/sends serial inputs from/to Arduino, and acts as a websocket server which Godot connects to and sends/receives info from serial that way. Python does not process anything, simply communicates data between two programs, acting as an interface.

### Godot (GDScript and C#)

Main part of the simulation, responsible for managing all logic, processing all inputs, determining enemy behavior, rendering, etc. There are a lot of separate scripts, classes, and methods involved. Below are some of the most prominent ones:

- `arduino_communication.gd` – receives info on websocket and calls required methods in the `Player.cs`
- `Player.cs` – has methods to receive various data from Arduino and use it to rotate, shoot, determine damage, healing, etc.
- `Enemy.gd` – has methods that call various action scripts to move the enemies and make them shoot, avoid the player, etc.

**File structure for it**
`spacewarcommander/
├── Spacewar-Commander/
│   └── Spacewar-Commander.ino
└── Scripts/
    ├── Player.cs
    └── GD/
        └── Enemy.gd`

---

## 3. Sensors and Actuators

### Sensors

- **Button 1** – reload trigger. When the reload box is pushed in, it touches the button, signaling the simulation to give the user full ammo.
- **Button 2** – activates shooting when held down, used to kill enemies.
- **Button 3** – activates selected acceleration to rotate the spaceship in the desired direction.
- **Slider 1** – selects acceleration in the X direction (Yaw).
- **Slider 2** – selects direction in the Y direction (Pitch).
- **Potentiometer** – allows the user to distribute their resources between damage to enemies and self-healing.
- **Light Sensor** – detects light level. If the luminosity goes below a certain level, it turns off the in-simulation flashlight.

### Actuators

- **LEDs (4)** – each LED indicates the corresponding direction of damage received by the user: left, right, up, down (back).
- **Buzzer** – makes a low buzzing noise when the user is at low health.
- **Servo Motor** – when the user runs out of ammo, the servo pushes out the reload box, which needs to be pushed back in to reload.

---


