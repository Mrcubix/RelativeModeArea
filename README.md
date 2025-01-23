# Relative Mode Area

This plugin will allow you to define area within which you need need to place your pen, Absolute mouse or finger to move the cursor.

## Dependencies

- [OpenTabletDriver 0.6.4.0](https://github.com/OpenTabletDriver/OpenTabletDriver)

### For Touch only

- [OTD.EnhancedOutputMode](https://github.com/Mrcubix/OTD.EnhancedOutputMode)

## Installation

### Pen & Mouse only

Drag the zip file inside hte Plugin Manager or `File` > `Install Plugin...`

### Pen, Mouse & Touch

Place the dll for this plugin in the same folder as OTD.EnhancedOutputMode.Lib.dll
(in the same folder where OpenTabletDriver Installed OTD.EnhancedOutputMode).

## How to use

1. Make your area using the Absolute Mode's Area Editor,
2. Input the values at the bottom (except rotation as it is not supported yet) into the plugin,
3. Switch back to Relative Mode,
4. Save or Apply and it should work!

The X & Y values represent the center of the area, like in OpenTabletDriver.