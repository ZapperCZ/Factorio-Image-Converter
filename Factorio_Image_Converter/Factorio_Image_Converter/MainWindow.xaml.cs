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
    public partial class MainWindow : INotifyPropertyChanged
    {
        string imagePath;
        int colorRange = 20;
        public string BlueprintString;          //The result string
        BitmapImage _bitmapImage;
        List<UBlock> AvailableBlocks;           //Currently loaded blocks from palette
        List<UTile> AvailableTiles;             //Currently loaded tiles from palette
        List<Color> ImageColors;                //All colors in the image
        Root FactorioBlueprint;                 //Root for the result json
        public Dictionary<string, string> D_colorConversion;    //TODO: Change this to store block name instead of just color

        public BitmapImage ResultImage
        {
            get { return _bitmapImage; }
            set
            {
                if (_bitmapImage != value)
                {
                    _bitmapImage = value;
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
            FactorioBlueprint.blueprint.version = 281479273906176;     //Factorio map version number, not sure how to get it so I harcoded the current version
            Icon defaultIcon = new Icon();
            defaultIcon.signal = new Signal();
            defaultIcon.signal.type = "item";
            defaultIcon.signal.name = "wooden-chest";
            defaultIcon.index = 1;
            FactorioBlueprint.blueprint.icons.Add(defaultIcon);
        }
        private void ConvertImageToBlocks(BitmapImage inputImage) //1 image px = 4 factorio blocks
        {
            //FIX: Completely shits itself when there is an undefined color conversion
            InstantiateRoot();
            int index = 1;
            int found = 0;
            int totalPixels = 0;
            UBlock resultBlock = new UBlock();
            UTile resultTile = new UTile();
            Color originalColor = new Color();
            Color resultColor = new Color();
            Bitmap bitmap = BitmapImage2Bitmap(inputImage);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    //FIX: Code can't detect alpha channel in the image
                    Color pixelColor = bitmap.GetPixel(x, y);
                    string pixelColorHex = ColorTranslator.ToHtml(pixelColor).ToLower();
                    //TODO: Implement NiX3r's pixel compression code
                    //FIX: There is no distinction between black and transparent

                    if (D_colorConversion.ContainsKey(pixelColorHex))
                    {
                        if(AvailableBlocks.Count(block => block.name == D_colorConversion[pixelColorHex]) > 0)
                        {
                            resultBlock = AvailableBlocks.Find(block => block.name == D_colorConversion[pixelColorHex]);
                            resultColor = ColorTranslator.FromHtml(resultBlock.color);
                            //Debug.WriteLine(resultBlock.name);
                        }
                        else
                        {
                            resultTile = AvailableTiles.Find(tile => tile.name == D_colorConversion[pixelColorHex]);
                            resultColor = ColorTranslator.FromHtml(resultTile.color);
                            //Debug.WriteLine(resultTile.name);
                        }
                    }
                    else
                    {
                        resultColor = ColorTranslator.FromHtml(pixelColorHex);
                    }
                    originalColor = ColorTranslator.FromHtml(pixelColorHex);

                    //FIX: The generation code breaks itself when one or more colors aren't converted
                    if (true /*pixelColorHex != "#000000"*/) //TODO: Change this to compare alpha channel
                    {
                        totalPixels++;
                        bool foundBlock = false;
                        foreach (UBlock block in AvailableBlocks)
                        {
                            Color blockColor = ColorTranslator.FromHtml(block.color);
                            if (resultColor.R < (blockColor.R + colorRange) && resultColor.R > (blockColor.R - colorRange) &&
                                resultColor.G < (blockColor.G + colorRange) && resultColor.G > (blockColor.G - colorRange) &&
                                resultColor.B < (blockColor.B + colorRange) && resultColor.B > (blockColor.B - colorRange))
                            {
                                found++;
                                //Entities are listed through in pairs of 4, so top left, top right, bottom left, bottom right
                                int sizeX = Convert.ToInt32(block.occupied_space[0].ToString());
                                int sizeY = Convert.ToInt32(block.occupied_space[2].ToString());
                                bool tempX = Convert.ToBoolean(sizeX - 1);
                                bool tempY = Convert.ToBoolean(sizeY - 1);
                                tempX = !tempX;
                                tempY = !tempY;
                                sizeX = Convert.ToInt32(tempX)+1;
                                sizeY = Convert.ToInt32(tempY)+1;

                                List<Entity> entityList = new List<Entity>();

                                //Filling up all the 4 spacess
                                for (int i = sizeY; i > 0; i--)
                                {
                                    for(int j = sizeX; j > 0; j--)
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
                                if (resultColor.R < (tileColor.R + colorRange) && resultColor.R > (tileColor.R - colorRange) &&
                                    resultColor.G < (tileColor.G + colorRange) && resultColor.G > (tileColor.G - colorRange) &&
                                    resultColor.B < (tileColor.B + colorRange) && resultColor.B > (tileColor.B - colorRange))
                                {
                                    found++;
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
            }
            Debug.WriteLine("total normal pixels > " + totalPixels + " pixels recognized > " + found);
        }
        public void ConvertBlocksToJSON(string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(sw, FactorioBlueprint);
            }
            Debug.WriteLine("saved JSON to \"" + path + "\"");
        }
        private void CompressAndEncodeJSON(string path)
        {
            //Compress the JSON file using zlib deflate compression level 9, then convert to base64 and put '0' at the start

            //This code was inspired / copied from Gachl's Factorio Imager https://github.com/Gachl/FactorioImager
            //This program wouldn't be possible without them unless I would spend atleast another week on trying to figure out how to make this work
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
            Bitmap bitmap = BitmapImage2Bitmap(ResultImage);
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
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            //FIX: It's possible that alpha channel is lost here
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
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

            if (openFileDialog.FileName != "")  //FIX: Doesn't work, still crashes with no image
            {
                Debug.WriteLine(imagePath);
                imagePath = openFileDialog.FileName;

                //Save the image
                ResultImage = new BitmapImage(new Uri(imagePath));
            }
            LoadImageColors();
            //TODO: Image too big warning
        }
        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Implement color conversion
            if(imagePath != null)
            {
                ConvertImageToBlocks(ResultImage);       //This will convert only colors that are present in UsableBlocks.json, currently there is no color conversion
                ConvertBlocksToJSON(@"..\..\2-Resources\Blueprint.json");
                CompressAndEncodeJSON(@"..\..\2-Resources\Blueprint.json");

                ResultWindow resultWindow = new ResultWindow(BlueprintString);
                resultWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("No image selected");
            }
        }

        private void btn_ColorConv_Click(object sender, RoutedEventArgs e)
        {
            //FIX: Crash when no image loaded
            ColorConversionWindow colorWindow = new ColorConversionWindow(ImageColors,AvailableBlocks,AvailableTiles);
            colorWindow.ShowDialog();
            D_colorConversion = colorWindow.D_colorConversion;

            foreach(KeyValuePair<string,string> entry in D_colorConversion)
            {
                Debug.WriteLine(entry.Key + " - " + entry.Value);
            }
        }
    }
}