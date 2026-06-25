# PLINTERSECT

Create the intersection area between two closed polylines in AutoCAD.

## Features

* Select two closed polylines.
* Automatically converts both polylines to temporary regions.
* Performs a Boolean **Intersection** operation.
* Creates a new region representing the overlapping area.
* Highlights the resulting intersection region for easy identification.
* Preserves the original polylines.

## Command

```text
PLINTERSECT
```

## How It Works

1. Run `PLINTERSECT`.
2. Select the first closed polyline.
3. Select the second closed polyline.
4. The plugin calculates the overlapping area between the two polylines.
5. A new region is created representing the intersection.

## Requirements

* AutoCAD 2025 or later
* .NET 8.0
* x64 platform

## Installation

1. Build the project.
2. Load the generated DLL using:

```text
NETLOAD
```

3. Select the compiled `PLINTERSECT.dll`.

## Example Use Cases

* Finding overlapping property boundaries.
* Utility and corridor clash analysis.
* Area comparison between design alternatives.
* GIS and mapping workflows.
* Civil engineering layout checks.

## Author

Suman Kumar
