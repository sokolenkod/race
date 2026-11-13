# Third-party assets

All downloaded files are under `Assets/ThirdParty/`. Models the game spawns at runtime live in a `Resources` folder so Unity can load them with `Resources.Load`. If a model fails to import, the race falls back to procedurally generated cars, pines and rocks.

## Kenney Car Kit 3.1 — CC0

- Author: Kenney (www.kenney.nl)
- License: Creative Commons Zero 1.0 (public domain)
- https://creativecommons.org/publicdomain/zero/1.0/
- Page: https://kenney.nl/assets/car-kit
- Mirror used for the download: https://opengameart.org/content/car-kit
- Local license: `Assets/ThirdParty/Kenney/CarKit/License.txt`
- Files: `Assets/ThirdParty/Kenney/CarKit/Resources/Vehicles/`
  - `race.fbx` — player car
  - `sedan-sports.fbx` — AI Novak
  - `hatchback-sports.fbx` — AI Horak
  - `suv.fbx` — AI Berg
  - `cone.fbx` — checkpoint cones
  - `Textures/colormap.png` — palette referenced by the FBX materials

## Kenney Nature Kit 2.1 — CC0

- Author: Kenney (www.kenney.nl)
- License: Creative Commons Zero 1.0 (public domain)
- Page: https://kenney.nl/assets/nature-kit
- Mirror used for the download: https://opengameart.org/content/nature-kit
- Local license: `Assets/ThirdParty/Kenney/NatureKit/License.txt`
- Files: `Assets/ThirdParty/Kenney/NatureKit/Resources/Props/`
  - Pines and broadleaf trees: `tree_pineDefaultA`, `tree_pineDefaultB`, `tree_pineTallA`, `tree_pineSmallA`, `tree_default`
  - Rocks: `rock_largeA`, `rock_largeB`, `rock_tallA`, `rock_smallA`, `rock_smallB`, `stone_largeA`
  - Ground cover: `plant_bush`, `plant_bushLarge`, `grass_large`, `flower_redA`, `mushroom_red`

Only these FBX files were kept from the pack so the repository stays small. Materials are vertex/material colors embedded in the FBX (no separate texture).

## Kenney Starter Kit Racing — audio, CC0

- Source: https://github.com/KenneyNL/Starter-Kit-Racing
- The repository code is MIT. The README states that the included 2D, 3D and sound assets are CC0. The skid effect is by Landeplage and is also CC0.
- Files: `Assets/ThirdParty/Kenney/RacingAudio/Resources/Audio/`
  - `engine.ogg` — looping engine
  - `skid.ogg` — drift / off-road skid (Landeplage, CC0)
  - `impact.ogg` — wall and rock impact

## Original content (not third-party)

The mountain road, terrain, guardrails, gantry, HUD, music bed and fallback meshes are generated in code (`Assets/Scripts/AlpineRush/`). The music is a short original synth loop (Dm–Bb–F–C), not a downloaded track.
