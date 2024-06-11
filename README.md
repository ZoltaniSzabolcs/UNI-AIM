# AK47 Aim Simulator

## Overview

AK47 Aim Simulator is an immersive first-person aim training simulator that allows users to experience semi-automatic firing with an AK47 assault rifle. The program simulates weapon recoil, target shooting, and various target behaviors in a predefined or randomized environment. It provides a realistic training environment for improving shooting accuracy and reflexes.

## Features

- **Weapon Simulation**: 
  - Semi-automatic firing with AK47.
  - Recoil simulation upon firing.
  - Ability to holster the weapon to fire without recoil.

- **Target Mechanics**: 
  - Circles or other objects (e.g., ducks) move in predefined or random patterns.
  - Targets disappear when hit, or another random object appears to indicate a hit.
  - Targets can change color upon being hit (e.g., white to red) and stay for 2 seconds.
  - Pressing the 'P' key changes all targets to objects.

- **Controls**: 
  - `WASD` keys for movement within the designated area.
  - `O` key to toggle drawing only white circles that turn red when hit.
  - `P` key to change all targets to objects.
  - `Q` key to holster the weapon.
  - `T` key for a third-person view showing the AK47 and possibly the character.

- **Customization**: 
  - Adjust target distances using specific keys or via ImGui interface.
  - The background is a skybox for clear contrast with targets and ducks.

## Controls

- **Movement**: 
  - `W` - Move forward
  - `A` - Move left
  - `S` - Move backward
  - `D` - Move right

- **Weapon Handling**: 
  - `Q` - Holster the weapon
  - `T` - Toggle third-person view

- **Target Interaction**: 
  - `O` - Toggle target drawing mode (white to red upon hit)
  - `P` - Change all targets to objects

## Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/ZoltaniSzabolcs/UNI-AIM.git
