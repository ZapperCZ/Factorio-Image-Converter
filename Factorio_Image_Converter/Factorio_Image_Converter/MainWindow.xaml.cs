using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using zlib;

namespace Factorio_Image_Converter
{
    public partial class MainWindow : Window
    {
        string imagePath;
        string Blueprint;
        System.Drawing.Image OriginalImage;
        System.Drawing.Image ResultImage;
        List<UBlock> AvailableBlocks;
        List<UTile> AvailableTiles;
        List<System.Drawing.Color> AvailableColors;

        Root FactorioBlueprint;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoad;
        }
        private void OnLoad(object sender,EventArgs e)
        {
            AvailableBlocks = new List<UBlock>();
            AvailableTiles = new List<UTile>();
            AvailableColors = new List<System.Drawing.Color>();
            FactorioBlueprint = new Root();
            LoadAvailableBlocks();
            LoadAvailableColors();

        }
        private void InstantiateRoot()
        {
            //Create an instance of JSON Root
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
        private void ConvertImageToBlocks(System.Drawing.Image inputImage) //1px = 4 blocks
        {
            InstantiateRoot();
            //TODO: Convert pixels of image to blocks
            int index = 1;
            int found = 0;
            int totalPixels = 0;
            Bitmap bitmap = (Bitmap)inputImage;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    //Debug.WriteLine("x > "+x+" y > "+y);
                    System.Drawing.Color pixelColor = bitmap.GetPixel(x, y);
                    string pixelColorHex = ColorTranslator.ToHtml(pixelColor).ToLower();
                    if (pixelColorHex != "#000000") //transparent
                    {
                        totalPixels++;
                        foreach (UBlock block in AvailableBlocks)
                        {
                            //Debug.WriteLine("pixel > " + pixelColorHex + " block > " + block.color);
                            if (pixelColorHex == block.color)
                            {
                                found++;
                                //Debug.WriteLine("block");
                                //All positions must be 0.5 because of rails, rails are 0.0
                                //Coordinates are based on mathematics, not IT
                                //Entities are listed through in pairs of 4, so top left, top right, bottom left, bottom right
                                int sizeX = Convert.ToInt32(block.occupied_space[0].ToString());
                                int sizeY = Convert.ToInt32(block.occupied_space[2].ToString());
                                //Debug.WriteLine("orig > " + block.occupied_space + " x > " + sizeX + " y > " + sizeY);

                                List<Entity> entityList = new List<Entity>();
                                //TODO: Finish and optimize this
                                //Maybe use some "equation" to determine the position and entity amount based on size?

                                #region A_Big_Mess
                                if (sizeX == 1 && sizeY == 1)
                                {
                                    Entity entity1 = new Entity();
                                    Entity entity2 = new Entity();
                                    Entity entity3 = new Entity();
                                    Entity entity4 = new Entity();
                                    Position pos1 = new Position();
                                    Position pos2 = new Position();
                                    Position pos3 = new Position();
                                    Position pos4 = new Position();
                                    pos1.x = x + x - 1.5;
                                    pos1.y = y + y - 1.5;
                                    pos2.x = x + x - 0.5;
                                    pos2.y = y + y - 1.5;
                                    pos3.x = x + x - 1.5;
                                    pos3.y = y + y - 0.5;
                                    pos4.x = x + x - 0.5;
                                    pos4.y = y + y - 0.5;

                                    entity1.position = pos1;
                                    entity2.position = pos2;
                                    entity3.position = pos3;
                                    entity4.position = pos4;
                                    entityList.Add(entity1);
                                    entityList.Add(entity2);
                                    entityList.Add(entity3);
                                    entityList.Add(entity4);
                                }
                                else if (sizeX == 2 && sizeY == 1)
                                {
                                    Entity entity1 = new Entity();
                                    Entity entity2 = new Entity();
                                    Position pos1 = new Position();
                                    Position pos2 = new Position();
                                    pos1.x = x + x - 1;
                                    pos1.y = y + y - 1.5;
                                    pos2.x = x + x - 1;
                                    pos2.y = y + y - 0.5;

                                    entity1.position = pos1;
                                    entity2.position = pos2;
                                    entityList.Add(entity1);
                                    entityList.Add(entity2);
                                }
                                else if (sizeX == 1 && sizeY == 2)
                                {
                                    Entity entity1 = new Entity();
                                    Entity entity2 = new Entity();
                                    Position pos1 = new Position();
                                    Position pos2 = new Position();
                                    pos1.x = x + x - 1.5;
                                    pos1.y = y + y - 1;
                                    pos2.x = x + x - 0.5;
                                    pos2.y = y + y - 1;

                                    entity1.position = pos1;
                                    entity2.position = pos2;
                                    entityList.Add(entity1);
                                    entityList.Add(entity2);
                                }
                                else if (sizeX == 2 && sizeY == 2)
                                {
                                    Entity entity1 = new Entity();
                                    Position pos = new Position();
                                    pos.x = x + x - 1;
                                    pos.y = y + y - 1;
                                    entity1.position = pos;
                                    entityList.Add(entity1);
                                }
                                #endregion

                                foreach (Entity entity in entityList)
                                {
                                    entity.entity_number = index++;
                                    entity.name = block.name;
                                    if (block.name.Contains("underground"))
                                    {
                                        entity.type = "input";
                                        entity.SType = true;
                                    }
                                    if (block.has_direction)
                                    {
                                        entity.direction = 4;
                                        entity.SDirection = true;
                                    }
                                    FactorioBlueprint.blueprint.entities.Add(entity);
                                }
                                break;
                            }
                        }
                        //TODO: only iterate through if we haven't found a block, otherwise it just slows the app down
                        foreach (UTile tile in AvailableTiles)
                        {
                            //Debug.WriteLine("pixel > " + pixelColorHex + " tile > " + tile.color);
                            if (pixelColorHex == tile.color)
                            {
                                //I hate this
                                //TODO: Fix this spaghetti in some nice loop
                                found++;
                                Tile tile1 = new Tile();
                                Tile tile2 = new Tile();
                                Tile tile3 = new Tile();
                                Tile tile4 = new Tile();
                                Position pos1 = new Position();
                                Position pos2 = new Position();
                                Position pos3 = new Position();
                                Position pos4 = new Position();
                                tile1.name = tile.name;
                                tile2.name = tile.name;
                                tile3.name = tile.name;
                                tile4.name = tile.name;
                                pos1.x = x + x - 2;
                                pos1.y = y + y - 2;
                                pos2.x = x + x - 1;
                                pos2.y = y + y - 2;
                                pos3.x = x + x - 2;
                                pos3.y = y + y - 1;
                                pos4.x = x + x - 1;
                                pos4.y = y + y - 1;
                                tile1.position = pos1;
                                tile2.position = pos2;
                                tile3.position = pos3;
                                tile4.position = pos4;
                                FactorioBlueprint.blueprint.tiles.Add(tile1);
                                FactorioBlueprint.blueprint.tiles.Add(tile2);
                                FactorioBlueprint.blueprint.tiles.Add(tile3);
                                FactorioBlueprint.blueprint.tiles.Add(tile4);

                                break;
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
            //This program wouldn't be possible without him unless I would spend atleast another week on trying to figure out how to make this work
            //I will be honest, at the moment I have no clue how exactly does it work, but I am so glad it does

            string json;
            StreamReader streamReader = new StreamReader(path);
            json = streamReader.ReadToEnd();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] jsonArray = encoding.GetBytes(json);
            byte[] compressedArray;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZOutputStream zOutput = new ZOutputStream(memoryStream, zlibConst.Z_DEFAULT_COMPRESSION))
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
            Blueprint = "0" + Convert.ToBase64String(compressedArray);
            Debug.WriteLine("\n\n Blueprint \n" + Blueprint);
        }
        private void LoadAvailableBlocks()
        {
            //Loads Factorio Blocks and their colors from a JSON file into a List
            StreamReader sr = new StreamReader(@"..\..\2-Resources\Usable-Blocks.json");
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
        private void LoadAvailableColors()
        {
            //Reads all colors from available blocks and puts them into a list
            foreach (UBlock block in AvailableBlocks)
            {
                System.Drawing.Color newColor = ColorTranslator.FromHtml(block.color);
                AvailableColors.Add(newColor);
            }
            foreach (UTile tile in AvailableTiles)
            {
                System.Drawing.Color newColor = ColorTranslator.FromHtml(tile.color);
                AvailableColors.Add(newColor);
            }
        }
        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            imagePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";    //Add more image formats
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;     //Opens up where the user chose the last file

            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                Debug.WriteLine(imagePath);
                imagePath = openFileDialog.FileName;

                //Save the image
                OriginalImage = System.Drawing.Image.FromFile(imagePath);
                ResultImage = System.Drawing.Image.FromFile(imagePath);
            }
            //TODO: Image too big warning
        }
        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            if(imagePath != null)
            {
                ConvertImageToBlocks(ResultImage);       //This will convert only colors that are present in UsableBlocks.json, currently there is no color conversion
                ConvertBlocksToJSON(@"..\..\2-Resources\Blueprint.json");
                CompressAndEncodeJSON(@"..\..\2-Resources\Blueprint.json");
            }
            else
            {
                MessageBox.Show("No image selected");
            }
        }
    }
}
