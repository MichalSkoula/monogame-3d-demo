# MonoGame Pseudo-3D Demo

A compact pseudo-3D renderer built with [MonoGame](https://monogame.net/) using classic per-column raycasting techniques (Wolfenstein-style rendering).  
This project is intended as an educational example of how to create a simple first-person effect from a 2D grid map.

https://github.com/user-attachments/assets/a9dfb9c1-e593-4b94-a5bb-b5e5e36ebbbf

## Features

- 2D tile map with wall collision
- WASD movement
- Mouse-look rotation
- Very simple fish-eye correction
- Distance-based wall brightness

## How it works (high level)

1. The world is represented by a 2D integer map (`0 = empty`, `1 = wall`).
2. For each screen column, a ray is cast from the player position at an angle inside the camera FOV.
3. The ray advances in small steps until it hits a wall or exits map bounds.
4. Hit distance is converted into wall slice height (closer wall = taller slice).
5. Vertical slices are drawn one by one to create the pseudo-3D scene.

## How to run it without Visual Studio:

dotnet run --project monogame-3d-demo

## Possible next steps

- texture-mapped walls
- minimap overlay
- adjustable FOV
- sprites/enemies
- level loading from external files
- frame-time/performance HUD

## Licence

MIT [skoula.cz](https://skoula.cz)
