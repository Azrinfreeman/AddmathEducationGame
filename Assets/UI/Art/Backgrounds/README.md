# Playground artwork

The editable source is `generate_art.py`. It draws the scenery and navigation pictures with Pillow primitives. These assets were authored for this project; no external artwork or generative image service was used.

Run `python Assets/UI/Art/Backgrounds/generate_art.py` from the project root to export the eight PNG sprites. Pillow is required and was already available in the development environment. Change the named colors or drawing coordinates in the source, export, then let Unity reimport the PNGs. Existing `.meta` files preserve sprite references.

Scene composition, cropping and placement are edited in `Assets/UI/Prefabs/Backgrounds/MathPlayground.prefab`. Color tints and motion periods belong to the profiles under `Assets/UI/Resources/Backgrounds/`. Editing the prefab does not require regenerating these PNGs.
