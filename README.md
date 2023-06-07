# minit
A collection of my personal init scripts for certain libraries and workflows written in C#. Everything is implemented using [System.CommandLine](https://github.com/dotnet/command-line-api).
## Installation
Extract the most recent release zip anywhere and add it `PATH` if you wish

# Supported commands
## Manim
init script for [ManimCE](https://github.com/manimCommunity/manim) since I found the bundled one lacking. 
### Features over the original
* Leaves out the default template clutter in the main.py file (No template feature)
* Adds a pyproject.toml which sets linters to ignore wildcard imports
### Usage
Run the command `minit manim <ProjectName>` and a project folder will be created in the current folder with the settings you specified.

## LaTeX
initialize a simple latex project
### Usage
call `minit latex <ProjectName> [<options>]` and a project folder with `ProjectName` will be created
#### Options
* **-b** or **--bib**: Wether to add a `main.bib` file, default `false`
* **-l** or **--language**: specify language for `cleveref`, `varioref` and `babel`, default `english`
* **-p** or **--packages**: specify a package to be added, can be used multiple times
* **-c** or **--class**: specify `documentclass`, default `article`
