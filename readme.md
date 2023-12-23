# Don Defender Source Repo

Welcome to don defender. This is a project based on the tutorial from [Devslopes Unity 3D](https://www.udemy.com/course/devslopes-unity3d/) course.
I've added a few more system on top of the tutorial. It was a great learning opportunity. To play it, visit [here](https://donis3.github.io/unity-dondefender-build/)

![Gameplay of Don Defender](https://github.com/donis3/unity-dondefender-build/blob/main/img/dondefender_gameplay.gif)

## Game Features

-   3 Levels with varying wave counts.
-   3 Tower types
-   Tower upgrades
-   Auto next wave, 2x speed and pause menu
-   Sounds not implemented

## Technical Details

-   A wave generator system implemented. Generates a wave with various enemy types depending on the current wave number. No need for statically typing each wave enemy list. Just define how many waves you want and set the difficulty level. Enemy lists will be auto generated at runtime.

-   A feedback system implemented to show the player messages at the bottom left corner.
