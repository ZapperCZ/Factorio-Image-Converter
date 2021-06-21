# Factorio Image Converter
This is a Windows desktop app created for conversion of pixel art into Factorio map art.
It was originally developed in Windows Forms, but has been ported over to WPF due to databinding and other features of WPF.
## Info
The optimal input for the app is pixel art in jpeg or png format. It is not specifically designed to handle anything else.
It might ocassionaly process a normal image correctly, but this is not guaranteed and will usually happen due to sheer coincidence.
The app might sometimes struggle even with normal pixel art, especially if it doesn't have clearly defined edges. 
## How to operate
To import an image, simply click on the "Import" button and select your desired image.
After that you need to specify the palette that should be used for color conversion (Normal, Map Editor, Full).
With a specified palette, you now need to convert the image colors to the one in the palette. This must be done manually by assigning a palette color to the image color.
Before exporting the image, you can specify how much it should be enlargened (4x, 16x, 64x), it is recommended to leave this setting at 4x unless you want the result to be really big
Finally you can click the export button which will bring up a dialog with the resulting blueprint string that you can copy and paste into Factorio
