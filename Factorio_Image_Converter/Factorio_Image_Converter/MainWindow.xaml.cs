using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using zlib;
using System.Windows.Media.Imaging;
using System.Linq;

namespace Factorio_Image_Converter
{
    //Big thanks to Gachl (https://github.com/Gachl) for creating the monocolor image converter and to Factorio Prints (https://factorioprints.com/) for creating and maintaining a great online tool
    //TODO: Implement transparency
    //TODO: Default non-converted colors to being transarent by default
    //TODO: Maybe add some sort of progression bar for when the image is exporting
    //TODO: Improve the design of the interface, design it closer to how the Factorio interface looks
    public partial class MainWindow : INotifyPropertyChanged
    {
        string imagePath;                       //Path of the user loaded image
        int colorRange = 10;                    //Intensity of antialiasing removal (maybe give user access to this?)
        public string BlueprintString;          //The result Factorio blueprint string
        BitmapImage _currentImage;              //Current shown image
        BitmapImage OriginalImage;              //The image user selected for conversion
        BitmapImage ResultImage;                //The converted image
        List<UBlock> AvailableBlocks;           //Currently loaded blocks from palette
        List<UTile> AvailableTiles;             //Currently loaded tiles from palette
        List<Color> ImageColors;                //All colors in the image
        Root FactorioBlueprint;                 //Root for the result json
        private Dictionary<string, int> D_RequiredBlocks;       //Dictionary that contains how many blocks are present in the picture <block name, amount>
        public Dictionary<string, string> D_colorConversion;    //Dictionary that ties image colors with the Factorio colors <original color hex, result block name>

        public BitmapImage CurrentImage
        {
            get { return _currentImage; }
            set
            {
                if (_currentImage != value)
                {
                    Debug.WriteLine("set curr img");
                    _currentImage = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Loaded += OnLoad;
        }
        private void OnLoad(object sender,EventArgs e)
        {
            FactorioBlueprint = new Root();
            D_colorConversion = new Dictionary<string, string>();
            LoadAvailableBlocks();          //Loads the palette
        }
        private void InstantiateRoot()
        {
            //Create an instance of JSON Root and initialize it
            FactorioBlueprint = new Root();
            FactorioBlueprint.blueprint = new Blueprint();
            FactorioBlueprint.blueprint.description = "The result image made by Zapper's Factorio Image Converter";
            FactorioBlueprint.blueprint.icons = new List<Icon>();
            FactorioBlueprint.blueprint.entities = new List<Entity>();
            FactorioBlueprint.blueprint.tiles = new List<Tile>();
            FactorioBlueprint.blueprint.item = "blueprint";
            FactorioBlueprint.blueprint.label = "Image";
            FactorioBlueprint.blueprint.version = 281479273906176;      //Factorio map version number, not sure how to get it so I harcoded the current version
                                                                        //TODO: Maybe find out how this actually wokrs?
            Icon defaultIcon = new Icon();
            defaultIcon.signal = new Signal();
            defaultIcon.signal.type = "item";
            defaultIcon.signal.name = "wooden-chest";
            defaultIcon.index = 1;
            FactorioBlueprint.blueprint.icons.Add(defaultIcon);
        }
        private void ConvertImageToBlocks(BitmapImage inputImage) //1 image px = 4 factorio blocks
        {
            InstantiateRoot();
            int index = 1;
            UBlock resultBlock = new UBlock();
            UTile resultTile = new UTile();
            Color resultColor = new Color();
            Bitmap bitmap = BitmapImage2Bitmap(inputImage);
            D_RequiredBlocks = new Dictionary<string, int>();

            //List<string> debugList = new List<string>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    /*
                     * TODO: This is very unoptimized as there are a lot of actions happening for every pixel, optimize this
                     * Optimize this maybe by detecting if the pixel color has already been used in the past and using that instead of checking it again
                     * Or assign a range for every block (2 colors creating a line in 3D space if you consider the colors to be vectors) and check if the current color is in the range
                    */

                    Color pixelColor = bitmap.GetPixel(x, y);
                    string pixelColorHex = ColorTranslator.ToHtml(pixelColor).ToLower();
                    //TODO: Implement NiX3r's pixel compression code

                    //Find the block selected for this color
                    foreach (KeyValuePair<string, string> pair in D_colorConversion)    //Iterate through the dictionary
                    {
                        Color dictColor = ColorTranslator.FromHtml(pair.Key);   //Color from the Dictionary

                        if (pixelColor.R < (dictColor.R + colorRange) && pixelColor.R > (dictColor.R - colorRange) &&
                            pixelColor.G < (dictColor.G + colorRange) && pixelColor.G > (dictColor.G - colorRange) &&
                            pixelColor.B < (dictColor.B + colorRange) && pixelColor.B > (dictColor.B - colorRange))
                        {
                            //If the current color is in the color range of the current pixel
                            pixelColorHex = pair.Key;   //assign the current color as the color of the pixel

                            //TODO: Check that this actually works using debug
                            //Count required blocks
                            if (!D_RequiredBlocks.ContainsKey(pair.Value))
                            {
                                D_RequiredBlocks.Add(pair.Value, 1);    //First time block has been found in the image, add it and initialize its amount as 1
                            }
                            else
                                D_RequiredBlocks[pair.Value]++;         //Block already exists in the image, increase its count

                            //Check if the color corresponds to a block or a tile
                            if (AvailableBlocks.Count(block => block.name == pair.Value) > 0)
                            {
                                resultBlock = AvailableBlocks.Find(block => block.name == pair.Value);
                                resultColor = ColorTranslator.FromHtml(resultBlock.color);
                                //Debug.WriteLine(resultBlock.color + "\t- " + resultBlock.name);
                                break;
                            }
                            else
                            {
                                resultTile = AvailableTiles.Find(tile => tile.name == pair.Value);
                                resultColor = ColorTranslator.FromHtml(resultTile.color);
                                //Debug.WriteLine(resultTile.color + "\t- " +  resultTile.name);
                                break;
                            }
                        }
                        else
                        {
                            resultColor = ColorTranslator.FromHtml(pixelColorHex);
                            //Debug.WriteLine(pixelColorHex + "\t- outside of range");
                        }
                        //Debug.WriteLine("");
                    }

                    //I'm not sure what's exactly happening below
                    bool foundBlock = false;
                    foreach (UBlock block in AvailableBlocks)
                    {
                        Color blockColor = ColorTranslator.FromHtml(block.color);
                        //Checking if the current pixel corresponds to the block by comparing their colors with range accounted for
                        if (resultColor.R < (blockColor.R + colorRange * 1) && resultColor.R > (blockColor.R - colorRange * 1) &&
                            resultColor.G < (blockColor.G + colorRange * 1) && resultColor.G > (blockColor.G - colorRange * 1) &&
                            resultColor.B < (blockColor.B + colorRange * 1) && resultColor.B > (blockColor.B - colorRange * 1))
                        {
                            //Entities are listed through in pairs of 4, so top left, top right, bottom left, bottom right
                            int sizeX = Convert.ToInt32(block.occupied_space[0].ToString());
                            int sizeY = Convert.ToInt32(block.occupied_space[2].ToString());
                            bool tempX = Convert.ToBoolean(sizeX - 1);
                            bool tempY = Convert.ToBoolean(sizeY - 1);
                            tempX = !tempX;
                            tempY = !tempY;
                            sizeX = Convert.ToInt32(tempX) + 1;
                            sizeY = Convert.ToInt32(tempY) + 1;

                            List<Entity> entityList = new List<Entity>();

                            //Filling up all the 4 spacess
                            for (int i = sizeY; i > 0; i--)
                            {
                                for (int j = sizeX; j > 0; j--)
                                {
                                    Entity newEntity = new Entity();
                                    Position pos = new Position();
                                    pos.x = x + x - j;
                                    pos.y = y + y - i;
                                    newEntity.position = pos;
                                    newEntity.entity_number = index++;
                                    newEntity.name = block.name;
                                    if (block.name.Contains("underground"))
                                    {
                                        newEntity.type = "input";
                                        newEntity.SType = true;
                                    }
                                    if (block.has_direction)
                                    {
                                        newEntity.direction = 4;
                                        newEntity.SDirection = true;
                                    }
                                    FactorioBlueprint.blueprint.entities.Add(newEntity);
                                }
                            }
                            foundBlock = true;
                            break;
                        }
                    }

                    if (!foundBlock)
                    {
                        foreach (UTile tile in AvailableTiles)
                        {
                            Color tileColor = ColorTranslator.FromHtml(tile.color);

                            if (resultColor.R < (tileColor.R + colorRange * 1) && resultColor.R > (tileColor.R - colorRange * 1) &&
                                resultColor.G < (tileColor.G + colorRange * 1) && resultColor.G > (tileColor.G - colorRange * 1) &&
                                resultColor.B < (tileColor.B + colorRange * 1) && resultColor.B > (tileColor.B - colorRange * 1))
                            {
                                for (int i = 2; i > 0; i--)
                                {
                                    for (int j = 2; j > 0; j--)
                                    {
                                        Tile newTile = new Tile();
                                        Position pos = new Position();
                                        newTile.name = tile.name;
                                        pos.x = x + x - j;
                                        pos.y = y + y - i;
                                        newTile.position = pos;
                                        FactorioBlueprint.blueprint.tiles.Add(newTile);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            /*
            foreach(string color in debugList)
            {
                Color c = ColorTranslator.FromHtml(color);
                Debug.WriteLine(color + " || R: " + c.R + "  \tG:" + c.G + "  \tB:" + c.B);
            }
            */
        }
        private Bitmap ConvertBlocksToBitmap()
        {
            //Converts the blueprint structure into a bitmap
            //TODO: Optimize this
            int width = (int)CurrentImage.Width*4;      //X
            int height = (int)CurrentImage.Height*4;    //Y
            int minX = 0;
            int minY = 0;

            foreach (Entity e in FactorioBlueprint.blueprint.entities)  //Go through all blocks
            {
                UBlock block = AvailableBlocks.Find(b => b.name == e.name); //Find available block that matches the blueprint block
                int sizeX = Convert.ToInt32(block.occupied_space[0].ToString());
                int sizeY = Convert.ToInt32(block.occupied_space[2].ToString());    //Get block size

                //Get minimum X and Y coordinates of the blueprint
                if((int)e.position.x < minX)
                {
                    minX = (int)e.position.x;
                }
                if ((int)e.position.y < minY)
                {
                    minY = (int)e.position.y;
                }
            }

            foreach(Tile t in FactorioBlueprint.blueprint.tiles)        //Go through all tiles
            {
                if ((int)t.position.x < minX)
                {
                    minX = (int)t.position.x;
                }
                if ((int)t.position.y < minY)
                {
                    minY = (int)t.position.y;
                }
            }

            Bitmap resultBitmap = new Bitmap(width,height);

            //Debug.WriteLine("Bitmap size: " + resultBitmap.Width + "x" + resultBitmap.Height);
            foreach (Entity e in FactorioBlueprint.blueprint.entities)
            {
                UBlock block = AvailableBlocks.Find(b => b.name == e.name);
                Color c = ColorTranslator.FromHtml(block.color);
                int sizeX = Convert.ToInt32(block.occupied_space[0].ToString());
                int sizeY = Convert.ToInt32(block.occupied_space[2].ToString());
                int posX = (int)e.position.x + -1*minX;
                int posY = (int)e.position.y + -1*minY;
                if(sizeX > 1 || sizeY > 1)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            //Debug.WriteLine("Block:" + block.name + " Size:" + sizeX + "x" + sizeY + " Coordinates:" + (posX+x) + ", " + (posY + y));
                            resultBitmap.SetPixel(posX + x - 1, posY + y - 1, c);
                        }
                    }
                }
                else
                {
                    resultBitmap.SetPixel(posX, posY, c);
                }
            }

            foreach (Tile t in FactorioBlueprint.blueprint.tiles)
            {
                UTile tile = AvailableTiles.Find(ti => ti.name == t.name);
                Color c = ColorTranslator.FromHtml(tile.color);
                int posX = (int)t.position.x + -1 * minX;
                int posY = (int)t.position.y + -1 * minY;
                resultBitmap.SetPixel(posX, posY, c);
            }

            return resultBitmap;
        }
        private void CalculateRequiredBlocks()
        {
            int imageScaleMultiplier = 4;       //Currently hardcoded here, I couldn't find it anywhere else
            int originalCount;
            int blockArea = 1;
            bool isTile;
            UBlock block;
            UTile tile;

            Debug.WriteLine("Required blocks:");
            List<string> keys = new List<string>(D_RequiredBlocks.Keys);
            foreach(string key in keys)
            {
                block = AvailableBlocks.Find(b => b.name == key);
                tile = AvailableTiles.Find(t => t.name == key);
                isTile = block == null;

                originalCount = D_RequiredBlocks[key];
                if(!isTile)
                    blockArea = Convert.ToInt32(block.occupied_space[0].ToString()) * Convert.ToInt32(block.occupied_space[2].ToString()); ;
                D_RequiredBlocks[key] = originalCount * (imageScaleMultiplier / blockArea);
                Debug.WriteLine(key + " - " + D_RequiredBlocks[key]);

                block = null;
                tile = null;
            }
        }
        public void ConvertBlocksToJSON(string path)
        {
            //FIX: Can crash, for some reason the file cannot be accessed as it's already being used
            using (StreamWriter sw = File.CreateText(path))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(sw, FactorioBlueprint);
            }
            Debug.WriteLine("saved JSON to \"" + path + "\"");
        }
        private void CompressAndEncodeJSON(string path)
        {
            //Compress the JSON file using zlib deflate compression level 9, then convert to base64

            //This code was mostly copied from Gachl's Factorio Imager https://github.com/Gachl/FactorioImager
            //This program wouldn't be possible without them unless I would spend a lot of time trying to figure out how to make this work
            //I will be honest, at the moment I have no clue how exactly does it work, but I am so glad it does

            string json;
            StreamReader streamReader = new StreamReader(path);
            json = streamReader.ReadToEnd();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] jsonArray = encoding.GetBytes(json);
            byte[] compressedArray;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZOutputStream zOutput = new ZOutputStream(memoryStream, zlibConst.Z_BEST_COMPRESSION))
                {
                    using (Stream stream = new MemoryStream(jsonArray))
                    {
                        byte[] buffer = new byte[2000];
                        int len;
                        while ((len = stream.Read(buffer, 0, 2000)) > 0)
                        {
                            zOutput.Write(buffer, 0, len);
                        }
                        zOutput.Flush();
                        zOutput.finish();
                        compressedArray = memoryStream.ToArray();
                    }
                }
            }
            BlueprintString = "0" + Convert.ToBase64String(compressedArray);
            Debug.WriteLine("\n\n Blueprint \n" + BlueprintString);
        }
        private void LoadAvailableBlocks()
        {
            //Loads Factorio Blocks and their colors from a JSON file into a List
            AvailableBlocks = new List<UBlock>();
            AvailableTiles = new List<UTile>();
            StreamReader sr = new StreamReader(@"..\..\2-Resources\Palette-Normal.json");
            string jsonString = sr.ReadToEnd();
            URoot uRoot = JsonConvert.DeserializeObject<URoot>(jsonString); //Populate the C# structure with JSON data
            foreach (UBlock block in uRoot.UsableBlocks.UBlocks)
            {
                AvailableBlocks.Add(block);
            }
            foreach (UTile tile in uRoot.UsableBlocks.UTiles)
            {
                AvailableTiles.Add(tile);
            }
        }
        private void LoadImageColors()
        {
            //Finds all colors in the loaded image and puts the in a list
            ImageColors = new List<Color>();
            Bitmap bitmap = BitmapImage2Bitmap(CurrentImage);
            for(int y = 0; y < bitmap.Height; y++)
            {
                for(int x = 0; x < bitmap.Width; x++)
                {
                    bool containsColor = false;
                    Color pixelColor = bitmap.GetPixel(x, y);
                    if (ImageColors.Count > 0)
                    {
                        foreach (Color listColor in ImageColors)
                        {
                            if (pixelColor.R < (listColor.R + colorRange) && pixelColor.R > (listColor.R - colorRange) &&
                                pixelColor.G < (listColor.G + colorRange) && pixelColor.G > (listColor.G - colorRange) &&
                                pixelColor.B < (listColor.B + colorRange) && pixelColor.B > (listColor.B - colorRange))
                            {
                                //Already is inside list
                                containsColor = true;
                                break;
                            }
                        }
                        if (!containsColor)
                            ImageColors.Add(pixelColor);
                    }
                    else
                        ImageColors.Add(pixelColor);
                }
            }
        }
        private Color GetClosestColorFromList(Color inputColor,List<Color> colorList)
        {
            //FIX: Sometimes doesn't work due to it calculating RGB values separately
            //and then combining into one color which might not actually be in the list.
            //This is especially noticeable with non-gray colors

            //Debug.WriteLine("Input: " + ColorTranslator.ToHtml(inputColor) + "\tR: " + inputColor.R + "\tG: " + inputColor.G + "\tB:" + inputColor.B);
            int closestR = colorList.Aggregate((x, y) => Math.Abs(x.R - inputColor.R) < Math.Abs(y.R - inputColor.R) ? x : y).R;
            int closestG = colorList.Aggregate((x, y) => Math.Abs(x.G - inputColor.G) < Math.Abs(y.G - inputColor.G) ? x : y).G;
            int closestB = colorList.Aggregate((x, y) => Math.Abs(x.B - inputColor.B) < Math.Abs(y.B - inputColor.B) ? x : y).B;
            Color closestColor = Color.FromArgb(255, closestR, closestG, closestB);
            //Debug.WriteLine("Output: " + ColorTranslator.ToHtml(closestColor) + " \tR: " + closestR + "\tG: " + closestG + "\tB:" + closestB);
            return closestColor;
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public void SaveBitmapImage(BitmapImage bi, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bi));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        public void rbtn_ChangeImage(object sender, RoutedEventArgs e)
        {
            int target;
            System.Windows.Controls.RadioButton senderRbtn = (System.Windows.Controls.RadioButton)sender;
            if (senderRbtn.Content.ToString().Contains("Original"))
            {
                target = 0;
                Debug.WriteLine("Orig");
            }
            else
            {
                target = 1;
                Debug.WriteLine("Resu");
            }
            ChangeCurrentImage(target);
        }
        public void ChangeCurrentImage(int target)  //0 = original, 1 = result
        {
            if (target == 0)
            {
                Debug.Write("C-Orig-");
                CurrentImage = OriginalImage;
                Debug.WriteLine("S");
            }
            else
            {
                Debug.Write("C-Resu-");
                if(ResultImage != null)
                {
                    Debug.WriteLine("S");
                    CurrentImage = ResultImage;
                }
                else
                {
                    Debug.WriteLine("F");
                }
            }
        }
        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Somehow stop accessing the file of the image after it is loaded
            imagePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";    //TODO: Add more image formats
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;     //Opens up where the user chose the last file

            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != null && openFileDialog.FileName != "")
            {
                //Debug.WriteLine(imagePath);
                imagePath = openFileDialog.FileName;

                //Save the image
                OriginalImage = new BitmapImage(new Uri(imagePath));
                ChangeCurrentImage(0);
                
                LoadImageColors();
            }
            //TODO: Image too big warning
        }

        private void btn_Export_Click(object sender, RoutedEventArgs e)     //Export
        {
            if(imagePath != null && imagePath != "")
            {
                ConvertImageToBlocks(CurrentImage);
                ConvertBlocksToJSON(@"..\..\2-Resources\Blueprint.json");
                CompressAndEncodeJSON(@"..\..\2-Resources\Blueprint.json");
                Bitmap ResultBitmap = ConvertBlocksToBitmap();
                ResultBitmap.Save(@"..\..\2-Resources\output.png");
                ResultImage = Bitmap2BitmapImage(ResultBitmap);
                Debug.WriteLine("Original Image Size: " + (int)OriginalImage.Width + "x" + (int)OriginalImage.Height);
                Debug.WriteLine("Original Image Area: " + (int)OriginalImage.Width * (int)OriginalImage.Height);
                Debug.WriteLine("Result Image Size: " + (int)ResultBitmap.Width + "x" + (int)ResultBitmap.Height);
                Debug.WriteLine("Result Image Area: " + (int)ResultBitmap.Width * (int)ResultBitmap.Height);
                CalculateRequiredBlocks();

                ResultWindow resultWindow = new ResultWindow(BlueprintString, D_RequiredBlocks);
                resultWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("No image selected");
            }
        }

        private void btn_ColorConv_Click(object sender, RoutedEventArgs e) //Color Conversion
        {
            if (imagePath != null && imagePath != "")   //Check for valid image path
            {
                ColorConversionWindow colorWindow;
                if(D_colorConversion.Count > 0)     //If there are already entries
                    colorWindow = new ColorConversionWindow(ImageColors, AvailableBlocks, AvailableTiles, D_colorConversion);      //Assign the existing entries to the window
                else                                                        //Otherwise initialize the Dictionary
                    colorWindow = new ColorConversionWindow(ImageColors, AvailableBlocks, AvailableTiles);
                colorWindow.ShowDialog();           //Display the window
                D_colorConversion = colorWindow.D_colorConversion;  //Update the conversion based on the result from the window
            }
            else
            {
                MessageBox.Show("No image selected");
            }
        }
    }
}