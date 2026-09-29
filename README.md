# GameDev

Unity game development project using Unity 2023.1.22f31.

Repository:
https://github.com/am3068/GameDev

## Version Control

- Git and GitHub used for version control.
- Git LFS used for large binary assets.
- `.gitignore` excludes Unity-generated files.
- `.gitattributes` configures Git LFS.
- Unity `.meta` files are tracked.
- Asset Serialization is set to Force Text.
- Version Control is set to Visible Meta Files.

## Unity Project

Main project folders:

    Assets/
    Packages/
    ProjectSettings/

Generated folders such as `Library/`, `Temp/`, `Obj/`, `Logs/` and `UserSettings/` are excluded from Git.

## Team Workflow

- Team members commit their own work.
- Changes are committed incrementally.
- The repository is regularly kept up to date.
- Scenes and prefabs are coordinated to minimise merge conflicts.
- Unity Smart Merge can be used for Unity YAML conflicts.

## Git LFS

Large assets such as models, textures, audio, fonts and other binary files are managed through Git LFS.

Check LFS files:

    git lfs ls-files

## Development

Before working:

    git pull
    git lfs pull

After making changes:

    git add .
    git commit -m "Describe changes"
    git push
