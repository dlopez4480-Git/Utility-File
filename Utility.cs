using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace Utility
{
    //  Utility Guide
    /*
     * The Utility Guide is a program designed to automate many of the tasks I found tedious.
     * I wanted a single script that self-contained everything I needed so that I would very seldom need to import/export anything
     * Below is a list of the sections and capabilities of this script.
     * 
     *      STRUCTS
     * Below is a list of commonly used structures
     * 
     *      Coords: Represents a pair of coordinates on a 2D Matrix
     * 
     *      HANDLER METHODS
     * Below is a list of commonly used objects which can be statically called.
     * 
     *  
     *      >Files: SavES, uploads and downloads files from directories.
     *          >JSONAccess: File Manager for JSON files
     *          
     *      >Lists: Manipulates the contents of arrays and arraylists.
     *      
     *      >Matrices: manages and manipulates 2D arrays
     *          >Pathfinding: Performs pathfinding, searches and expansions in a 2D array
     *          >Misc: Misc usage cases for matrices
     *      
     *      >Perlin: Generates pseudorandom 2D matrix noise.
     *      
     *      >Images (TODO): ImageHandler is capable of image manipulation and modification
     * 
     * 
     * **/



    public struct Coords
    {
        public int x { get; set; }      //  Equivalent to ROW
        public int y { get; set; }      //  Equivalent to COL
        public Coords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString() => $"({x}, {y})";
    }
    public struct DiceRoll
    {
        public int diceNum { get; set; }

    }



    public class Files
    {
        public class JSONAccess()
        {
            public static T ReadJsonFile<T>(string filePath)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"JSON file not found: {filePath}");
                }

                try
                {
                    // Read the JSON file content
                    string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

                    // Deserialize the JSON content to the specified type
                    T result = JsonSerializer.Deserialize<T>(jsonContent);

                    return result;
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Failed to parse JSON file: {ex.Message}", ex);
                }
                catch (IOException ex)
                {
                    throw new IOException($"Error reading file: {ex.Message}", ex);
                }
            }
        }


        #region Basic Handlers
        public static readonly string sourceDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName).FullName).FullName).FullName;

        //  This function converts a string filepath to one that works as a valid filepath
        public static string GetDirectory(string path)
        {
            return sourceDirectory + path;
        }


        //  This function converts a valid directory into a short directory String
        public static string GetDirectoryString(string path)
        {
            // Check if filepath is already dynamic; if true, return path, if false, proceed
            try
            {
                if (!path.Substring(0, sourceDirectory.Length).Equals(sourceDirectory))
                {
                    Console.WriteLine("ERROR: Path does NOT begin with @C:..., returning path");
                    return path;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}: returning valid filepath ");
            }

            string result = path.Remove(0, sourceDirectory.Length);
            return result;
        }


        //  Creates a directory at the specified filepath
        public static void CreateDirectory(string path)
        {
            string filepath = GetDirectory(path);
            try
            {
                //  Test if Directory Exists
                if (Directory.Exists(filepath))
                {
                    Console.WriteLine("Directory " + path + " Already Exists");
                    return;
                }
                //  Create Directory
                DirectoryInfo di = Directory.CreateDirectory(filepath);
                Console.WriteLine("The directory " + path + " was created successfully at {0}.", Directory.GetCreationTime(filepath));
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }


        //  Read all lines of a .txt file, returns an array of Strings
        public static string[] ReadAllLines(string path)
        {
            string filepath = GetDirectory(path);
            string[] strings = File.ReadAllLines(filepath);
            return strings;
        }

        public static void WriteAllLines(string path, string[] contents)
        {
            string filepath = GetDirectory(path);
            File.WriteAllLines(filepath, contents);
        }
        //  Creates an alphabetized list .txt file from specified array at specified location
        #endregion
    }
    public class Lists
    {
        //  Removes the largest list withing a list of any objects
        public static void RemoveLargestList<T>(List<List<T>> listOfLists)
        {
            if (listOfLists == null || listOfLists.Count == 0)
                return;

            int maxIndex = 0;
            int maxCount = listOfLists[0].Count;

            for (int i = 1; i < listOfLists.Count; i++)
            {
                if (listOfLists[i].Count > maxCount)
                {
                    maxIndex = i;
                    maxCount = listOfLists[i].Count;
                }
            }

            listOfLists.RemoveAt(maxIndex);
        }

        //  Given a list of list of any objects, remove lists with an object count ABOVE minSize
        public static void RemoveListAboveSize<T>(List<List<T>> listOfLists, int minSize)
        {
            if (listOfLists == null) throw new ArgumentNullException(nameof(listOfLists));

            listOfLists.RemoveAll(subList => subList.Count > minSize);
        }

        //  Given a list of list of any objects, remove lists with an object count BELOW maxSize
        public static void RemoveListBelowSize<T>(List<List<T>> listOfLists, int maxSize)
        {
            if (listOfLists == null) throw new ArgumentNullException(nameof(listOfLists));

            listOfLists.RemoveAll(subList => subList.Count < maxSize);
        }

        //  Returns a random selection from a list
        public static List<T> GetRandomSelection<T>(List<T> source, int count, bool duplicatable, int seed)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
            if (!duplicatable && count > source.Count)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot exceed source size when duplicates are not allowed.");

            Random rng = new Random(seed);
            List<T> result = new List<T>(count);

            if (duplicatable)
            {
                // Sampling with replacement
                for (int i = 0; i < count; i++)
                {
                    int index = rng.Next(source.Count);
                    result.Add(source[index]);
                }
            }
            else
            {
                // Sampling without replacement (Fisher–Yates shuffle)
                List<T> copy = new List<T>(source);
                for (int i = 0; i < count; i++)
                {
                    int j = rng.Next(i, copy.Count);
                    (copy[i], copy[j]) = (copy[j], copy[i]);
                    result.Add(copy[i]);
                }
            }

            return result;
        }

        //  Shuffle a list into a new list
        public static IList<T> Shuffle<T>(IList<T> input, int seed)
        {
            var copy = new List<T>(input); // copy into a List<T>
            Random rand = new Random(seed);
            int n = copy.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T temp = copy[n];
                copy[n] = copy[k];
                copy[k] = temp;
            }
            return copy;
        }

        

    }
    public class Matrices
    {
        #region Simple Matrix Manipulation
        //  Get a list of Coords within a circular region around a center, in radius radius 
        public static List<Coords> GetCoordsWithinCircle<T>(T[,] array, Coords center, int radius)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            List<Coords> coordsList = new List<Coords>();

            for (int r = Math.Max(0, center.x - radius); r <= Math.Min(rows - 1, center.x + radius); r++)
            {
                for (int c = Math.Max(0, center.y - radius); c <= Math.Min(cols - 1, center.y + radius); c++)
                {
                    int deltaX = r - center.x;
                    int deltaY = c - center.y;
                    if (deltaX * deltaX + deltaY * deltaY <= radius * radius)
                    {
                        coordsList.Add(new Coords(r, c));
                    }
                }
            }

            return coordsList;
        }
        public static List<Coords> SelectOvalRegion<T>(T[,] array, int radiusHorizontal, int radiusVertical)
        {
            List<Coords> result = new List<Coords>();

            if (array == null || radiusVertical <= 0 || radiusHorizontal <= 0)
            {
                return result;
            }


            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            // Calculate center coordinates
            double centerX = (cols - 1) / 2.0;
            double centerY = (rows - 1) / 2.0;

            // Calculate squared radii for comparison (avoiding square roots for performance)
            double radiusVSquared = radiusVertical * radiusVertical;
            double radiusHSquared = radiusHorizontal * radiusHorizontal;

            // Determine the bounding box to iterate through (for efficiency)
            int minRow = Math.Max(0, (int)(centerY - radiusVertical));
            int maxRow = Math.Min(rows - 1, (int)(centerY + radiusVertical));
            int minCol = Math.Max(0, (int)(centerX - radiusHorizontal));
            int maxCol = Math.Min(cols - 1, (int)(centerX + radiusHorizontal));

            // Iterate through the bounding box
            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    // Calculate normalized coordinates relative to center
                    double dx = col - centerX;
                    double dy = row - centerY;

                    // Check if point is inside the oval using ellipse equation
                    // (dx/radiusHorizontal)^2 + (dy/radiusVertical)^2 <= 1
                    if ((dx * dx) / radiusHSquared + (dy * dy) / radiusVSquared <= 1.0)
                    {
                        result.Add(new Coords(col, row));
                    }
                }
            }

            return result;
        }
        //  Get a subsection of an array given a start and dimension parameters
        public static T[,] getArraySection<T>(T[,] array, int cols, int rows, Coords start)
        {
            T[,] section = new T[cols, rows];

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    int x = start.x + i;
                    int y = start.y + j;

                    if (x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1))
                    {
                        section[i, j] = array[x, y];
                    }
                }
            }

            return section;
        }
        //  See if a region of coords is contiguous
        static bool IsContiguous(List<Coords> coords)
        {
            if (coords.Count == 0) return false;

            HashSet<Coords> visited = new HashSet<Coords>();
            Queue<Coords> queue = new Queue<Coords>();
            queue.Enqueue(coords[0]);
            visited.Add(coords[0]);

            while (queue.Count > 0)
            {
                Coords current = queue.Dequeue();
                Coords[] directions = { new Coords(0, 1), new Coords(1, 0), new Coords(0, -1), new Coords(-1, 0) };

                foreach (var dir in directions)
                {
                    Coords neighbor = new Coords(current.x + dir.x, current.y + dir.y);
                    if (coords.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return visited.Count == coords.Count;
        }
        //  Given a coords section, find the path
        public static List<Coords> FindPath(List<Coords> coordsList, Coords source, Coords target)
        {
            List<Coords> path = new List<Coords>();
            int dx = Math.Sign(target.x - source.x);
            int dy = Math.Sign(target.y - source.y);

            int x = source.x;
            int y = source.y;

            while (x != target.x || y != target.y)
            {
                if (!coordsList.Contains(new Coords(x, y)))
                {
                    coordsList.Add(new Coords(x, y));
                }
                path.Add(new Coords(x, y));

                if (x != target.x) x += dx;
                if (y != target.y) y += dy;
            }

            if (!coordsList.Contains(target))
            {
                coordsList.Add(target);
            }
            path.Add(target);

            return coordsList;
        }
        //  Given a 2D array of doubles, normalize the array to a new scale
        public static int[,] NormalizeToInteger(double[,] inputArray, int min, int max)
        {
            int row = inputArray.GetLength(0);
            int col = inputArray.GetLength(1);


            int newmax = max;
            int[,] intArray = new int[row, col];

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            // Find min and max values in the double array
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    if (inputArray[x, y] < minValue) minValue = inputArray[x, y];
                    if (inputArray[x, y] > maxValue) maxValue = inputArray[x, y];
                }
            }

            // Normalize and scale values to the new range
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    intArray[x, y] = (int)(min + (inputArray[x, y] - minValue) / (maxValue - minValue) * (newmax - min));
                }
            }

            return intArray;
        }

        //  Given a List of List of Coords, convert it into an array of values. Each list is represented by an int; unclaimed cells are represented by a zero
        public static int[,] ConvertCoordstoArray(List<List<Coords>> superlistCoords, int rows, int cols)
        {
            int[,] array = new int[rows, cols];
            //  Initialize array

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] = 0;
                }
            }
            int ID = 1;
            foreach (List<Coords> coordslist in superlistCoords)
            {
                foreach (Coords coord in coordslist)
                {
                    array[coord.x, coord.y] = ID;
                }
                ID++;

            }



            return array;
        }

        #endregion

        #region Matrix Console Representation
        //  Note: This is more for debugging. TODO: Improve and expand this section
        public static void PrintIntArray(int[,] array)
        {
            //  Get Max Value
            int maxvalue = 0;
            foreach (int integer in array)
            {
                if (integer > maxvalue) { maxvalue = integer; }
            }

            //  Get digit count

            int digitCount = 0;
            if (maxvalue >= 0)
            {
                digitCount = maxvalue.ToString().Length;
            }
            else
            {
                digitCount = maxvalue.ToString().Length - 1;

            }
            String pad = "D" + digitCount;


            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(0); j++)
                {
                    if (array[i, j] >= 0)
                    {
                        Console.Write(" +" + array[i, j].ToString(pad) + " ");
                    }
                    else
                    {
                        Console.Write(" " + array[i, j].ToString(pad) + " ");
                    }
                }
                Console.WriteLine();

            }

        }

        //  TODO: Modify for greater than 0-9 
        public static void PrintIntArrayBounded(int[,] array, int boundary)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] >= boundary)
                    {
                        Console.Write(array[i, j] + " ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();

            }
        }

        public static void PrintCoordsListsAsMatrix(string[] labels, int height, int width, List<List<Coords>> territories, Boolean bracketed)
        {
            // Determine max label length
            int maxLen = 0;
            foreach (var label in labels)
            {
                if (label.Length > maxLen)
                {
                    maxLen = label.Length;
                }
            }

            // Cell width (including brackets if enabled)
            int cellWidth = bracketed ? maxLen + 2 : maxLen;

            // Initialize grid with nulls (to distinguish empties)
            string[,] grid = new string[height, width];

            // Fill grid with territory markers
            for (int team = 0; team < territories.Count; team++)
            {
                string symbol = labels[team];
                foreach (var coord in territories[team])
                {
                    if (coord.y >= 0 && coord.y < height &&
                        coord.x >= 0 && coord.x < width)
                    {
                        grid[coord.y, coord.x] = symbol;
                    }
                }
            }

            // Print grid to console
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    if (grid[r, c] == null) // empty cell
                    {
                        if (bracketed)
                            Console.Write(new string(' ', cellWidth));
                        else
                            Console.Write(new string(' ', cellWidth));
                    }
                    else
                    {
                        string content = grid[r, c].PadRight(maxLen, ' ');
                        if (bracketed)
                            Console.Write("[" + content + "]");
                        else
                            Console.Write(content);
                    }
                }
                Console.WriteLine();
            }
        }

        #endregion

        public class Pathfinding()
        {
            #region Islands Manager
            //  Given a 2D array of integers and an range of intergers, find all "islands" within that range, targets inclusive 
            public static List<List<Coords>> FindIslandsInRange(int[,] array, int targetRangeStart, int targetRangeEnd, bool hWrapping, bool vWrapping)
            {
                int rows = array.GetLength(0);
                int cols = array.GetLength(1);

                bool[,] visited = new bool[rows, cols];
                List<List<Coords>> islands = new List<List<Coords>>();

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (!visited[r, c] && array[r, c] >= targetRangeStart && array[r, c] <= targetRangeEnd)
                        {
                            List<Coords> island = new List<Coords>();
                            ExploreIsland(array, r, c, targetRangeStart, targetRangeEnd, visited, island, hWrapping, vWrapping);
                            islands.Add(island);
                        }
                    }
                }

                return islands;
            }
            private static void ExploreIsland(int[,] array, int startRow, int startCol, int targetRangeStart, int targetRangeEnd, bool[,] visited, List<Coords> island, bool hWrapping, bool vWrapping)
            {
                int rows = array.GetLength(0);
                int cols = array.GetLength(1);
                Queue<Coords> queue = new Queue<Coords>();
                queue.Enqueue(new Coords(startRow, startCol));

                while (queue.Count > 0)
                {
                    Coords current = queue.Dequeue();
                    int r = current.x;
                    int c = current.y;

                    if (visited[r, c]) continue;

                    visited[r, c] = true;
                    island.Add(new Coords(r, c));

                    int[] dr = { -1, 1, 0, 0 };
                    int[] dc = { 0, 0, -1, 1 };

                    for (int i = 0; i < 4; i++)
                    {
                        int newRow = r + dr[i];
                        int newCol = c + dc[i];

                        if (hWrapping)
                        {
                            newCol = (newCol + cols) % cols;
                        }

                        if (vWrapping)
                        {
                            newRow = (newRow + rows) % rows;
                        }

                        if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols && !visited[newRow, newCol]
                            && array[newRow, newCol] >= targetRangeStart && array[newRow, newCol] <= targetRangeEnd)
                        {
                            queue.Enqueue(new Coords(newRow, newCol));
                        }
                    }
                }
            }



            //  Given a 2D array of integers and a range of intergers, find the border of all islands within thickness range
            public static List<List<Coords>> FindIslandBordersInRange(int[,] array, int targetRangeStart, int targetRangeEnd, bool hWrapping, bool vWrapping, int thickness)
            {
                int rows = array.GetLength(0);
                int cols = array.GetLength(1);
                bool[,] visited = new bool[rows, cols];
                List<List<Coords>> borders = new List<List<Coords>>();

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (!visited[r, c] && array[r, c] >= targetRangeStart && array[r, c] <= targetRangeEnd)
                        {
                            List<Coords> border = new List<Coords>();
                            ExploreBorder(array, r, c, targetRangeStart, targetRangeEnd, visited, border, hWrapping, vWrapping, thickness);
                            borders.Add(border);
                        }
                    }
                }

                return borders;
            }
            private static void ExploreBorder(int[,] array, int startRow, int startCol, int targetRangeStart, int targetRangeEnd, bool[,] visited, List<Coords> border, bool hWrapping, bool vWrapping, int thickness)
            {
                int rows = array.GetLength(0);
                int cols = array.GetLength(1);
                Queue<Coords> queue = new Queue<Coords>();
                queue.Enqueue(new Coords(startRow, startCol));

                while (queue.Count > 0)
                {
                    Coords current = queue.Dequeue();
                    int r = current.x;
                    int c = current.y;

                    if (visited[r, c]) continue;

                    visited[r, c] = true;

                    if (IsOnBorder(array, r, c, targetRangeStart, targetRangeEnd, hWrapping, vWrapping, thickness))
                    {
                        border.Add(new Coords(r, c));
                    }

                    int[] dr = { -1, 1, 0, 0 };
                    int[] dc = { 0, 0, -1, 1 };

                    for (int i = 0; i < 4; i++)
                    {
                        int newRow = r + dr[i];
                        int newCol = c + dc[i];

                        if (hWrapping)
                        {
                            newCol = (newCol + cols) % cols;
                        }

                        if (vWrapping)
                        {
                            newRow = (newRow + rows) % rows;
                        }

                        if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols && !visited[newRow, newCol]
                            && array[newRow, newCol] >= targetRangeStart && array[newRow, newCol] <= targetRangeEnd)
                        {
                            queue.Enqueue(new Coords(newRow, newCol));
                        }
                    }
                }
            }
            private static bool IsOnBorder(int[,] array, int row, int col, int targetRangeStart, int targetRangeEnd, bool hWrapping, bool vWrapping, int thickness)
            {
                int rows = array.GetLength(0);
                int cols = array.GetLength(1);
                int[] dr = { -1, 1, 0, 0 };
                int[] dc = { 0, 0, -1, 1 };

                for (int t = 1; t <= thickness; t++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int newRow = row + dr[i] * t;
                        int newCol = col + dc[i] * t;

                        if (hWrapping)
                        {
                            newCol = (newCol + cols) % cols;
                        }

                        if (vWrapping)
                        {
                            newRow = (newRow + rows) % rows;
                        }

                        if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols || array[newRow, newCol] < targetRangeStart || array[newRow, newCol] > targetRangeEnd)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            #endregion

            #region Voronoi Expansions

            public static List<List<Coords>> VoronoiExpandTerritories(int[,] grid, List<Coords> seeds, int minimum, int maximum, bool horizontalWrapping, bool verticalWrapping, int seed)
            {
                int rows = grid.GetLength(0);
                int cols = grid.GetLength(1);

                // 1) Precompute which cells are valid (in range).
                bool[,] valid = new bool[rows, cols];
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        valid[y, x] = grid[y, x] >= minimum && grid[y, x] <= maximum;

                // 2) Remove duplicates: any coordinate that appears more than once is entirely discarded.
                var dupCounts = new Dictionary<(int x, int y), int>();
                foreach (var s in seeds)
                    dupCounts[(s.x, s.y)] = dupCounts.GetValueOrDefault((s.x, s.y), 0) + 1;

                var validSeeds = new List<Coords>();
                var seedToTeamIndex = new List<int>(); // maps validSeeds index back to input order (result order matches valid seeds’ input order)

                for (int i = 0; i < seeds.Count; i++)
                {
                    var s = seeds[i];
                    // duplicates are "dead"
                    if (dupCounts[(s.x, s.y)] > 1)
                        continue;

                    // in-bounds & in-range only
                    if (s.x < 0 || s.x >= cols || s.y < 0 || s.y >= rows)
                        continue;
                    if (!valid[s.y, s.x])
                        continue;

                    seedToTeamIndex.Add(validSeeds.Count);
                    validSeeds.Add(s);
                }

                // If nothing to do, return empty.
                if (validSeeds.Count == 0)
                    return new List<List<Coords>>();

                // Territories: one per valid seed, in the order they survived filtering.
                var territories = new List<List<Coords>>(validSeeds.Count);
                for (int i = 0; i < validSeeds.Count; i++)
                    territories.Add(new List<Coords>());

                // 3) Label connected components ("islands") over valid cells with the same wrapping rules.
                int[,] compId = new int[rows, cols];
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        compId[y, x] = -1;

                int[][] dirs = new int[][] 
                {
                    new []{0,-1}, // Up
                    new []{1,0},  // Right
                    new []{0,1},  // Down
                    new []{-1,0}  // Left
                };

                int currentComp = 0;
                for (int y0 = 0; y0 < rows; y0++)
                {
                    for (int x0 = 0; x0 < cols; x0++)
                    {
                        if (!valid[y0, x0] || compId[y0, x0] != -1) continue;

                        // BFS flood fill to mark this component.
                        var q = new Queue<(int x, int y)>();
                        compId[y0, x0] = currentComp;
                        q.Enqueue((x0, y0));

                        while (q.Count > 0)
                        {
                            var (cx, cy) = q.Dequeue();
                            foreach (var d in dirs)
                            {
                                int nx = cx + d[0];
                                int ny = cy + d[1];

                                if (horizontalWrapping) nx = (nx % cols + cols) % cols;
                                if (verticalWrapping) ny = (ny % rows + rows) % rows;

                                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) continue;
                                if (!valid[ny, nx]) continue;
                                if (compId[ny, nx] != -1) continue;

                                compId[ny, nx] = currentComp;
                                q.Enqueue((nx, ny));
                            }
                        }

                        currentComp++;
                    }
                }

                // 4) Group seeds by component.
                var compToSeeds = new Dictionary<int, List<int>>(); // compId -> list of team indices (index into validSeeds/territories)
                for (int t = 0; t < validSeeds.Count; t++)
                {
                    var s = validSeeds[t];
                    int cId = compId[s.y, s.x];
                    if (!compToSeeds.ContainsKey(cId)) compToSeeds[cId] = new List<int>();
                    compToSeeds[cId].Add(t);
                }

                // 5) Global owner map (per cell, which team owns it) initialized to -1
                int[,] owner = new int[rows, cols];
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        owner[y, x] = -1;

                // 6) Deterministic RNG and deterministic processing order.
                var rng = new Random(seed);

                // 7) For each component that has seeds, run a synchronous, wave-based multi-source BFS restricted to that component.
                //    This guarantees "equal expansion" and ensures no seed overwrites another.
                foreach (var kvp in compToSeeds.OrderBy(k => k.Key)) // process components in ascending id for determinism
                {
                    int cId = kvp.Key;
                    var teamsInComp = kvp.Value.OrderBy(t => t).ToList(); // team indices sorted for determinism

                    // Initialize frontier with the seeds in this component.
                    var frontier = new List<(int x, int y, int team)>();
                    foreach (var team in teamsInComp)
                    {
                        var s = validSeeds[team];
                        if (owner[s.y, s.x] == -1) // not owned yet
                        {
                            owner[s.y, s.x] = team;
                            territories[team].Add(s);
                            frontier.Add((s.x, s.y, team));
                        }
                        // if already owned, it must be by the same team due to deterministic earlier pass
                    }

                    // Wave expansion
                    while (frontier.Count > 0)
                    {
                        // Collect claims for next wave
                        // Key = cell, Value = set of teams attempting to claim
                        var claims = new Dictionary<(int x, int y), HashSet<int>>();

                        // Process current frontier in a deterministic order
                        foreach (var (x, y, team) in frontier.OrderBy(n => n.y).ThenBy(n => n.x).ThenBy(n => n.team))
                        {
                            foreach (var d in dirs)
                            {
                                int nx = x + d[0];
                                int ny = y + d[1];

                                if (horizontalWrapping) nx = (nx % cols + cols) % cols;
                                if (verticalWrapping) ny = (ny % rows + rows) % rows;

                                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) continue;
                                if (compId[ny, nx] != cId) continue;  // stay within this island
                                if (!valid[ny, nx]) continue;         // must be in valid range
                                if (owner[ny, nx] != -1) continue;    // already owned

                                var key = (nx, ny);
                                if (!claims.TryGetValue(key, out var set))
                                {
                                    set = new HashSet<int>();
                                    claims[key] = set;
                                }
                                set.Add(team);
                            }
                        }

                        if (claims.Count == 0)
                            break;

                        // Resolve claims deterministically:
                        // - Cells resolved in row-major order
                        // - Teams list sorted before RNG pick
                        var nextFrontier = new List<(int x, int y, int team)>(claims.Count);
                        foreach (var cell in claims.Keys.OrderBy(p => p.y).ThenBy(p => p.x))
                        {
                            var teamList = claims[cell].ToList();
                            teamList.Sort();

                            int chosenTeam = teamList.Count == 1
                                ? teamList[0]
                                : teamList[rng.Next(teamList.Count)];

                            owner[cell.y, cell.x] = chosenTeam;
                            var c = new Coords(cell.x, cell.y);
                            territories[chosenTeam].Add(c);
                            nextFrontier.Add((cell.x, cell.y, chosenTeam));
                        }

                        frontier = nextFrontier;
                    }
                }

                return territories;
            }

            #endregion



        }

        public class Misc()
        {
            public static int MaxColDistance(List<Coords> points)
            {
                if (points == null || points.Count < 2) return 0;

                int minY = int.MaxValue;
                int maxY = int.MinValue;

                foreach (var p in points)
                {
                    if (p.y < minY) minY = p.y;
                    if (p.y > maxY) maxY = p.y;
                }

                return Math.Abs(maxY - minY);
            }

            public static int MaxRowDistance(List<Coords> points)
            {
                if (points == null || points.Count < 2) return 0;

                int minX = int.MaxValue;
                int maxX = int.MinValue;

                foreach (var p in points)
                {
                    if (p.x < minX) minX = p.x;
                    if (p.x > maxX) maxX = p.x;
                }

                return Math.Abs(maxX - minX);
            }

        }

        
    }
    public class Perlin {
        // Gradients 
        private static readonly (int x, int y)[] Gradients = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, 1), (1, -1), (-1, -1) };
        public static double[,] GeneratePerlinNoise(int rows, int cols, double frequency, int seed)
        {
            // TODO: Height and Width appear to get crossed with each other
            //  As a tenative fix I am artificially swapping them here
            //  This needs to be investiated and the issue resolved 
            Boolean swapparameters = true;
            if (swapparameters)
            {
                int newrow = cols;
                int newcol = rows;
                cols = newcol;
                rows = newrow;
            }

            double[,] noise = new double[cols, rows];
            var permutation = GeneratePermutation(seed);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    double xf = x * frequency;
                    double yf = y * frequency;
                    noise[x, y] = Perlinize(xf, yf, permutation);
                }
            }
            return noise;
        }

        // Perlinization function
        private static double Perlinize(double x, double y, int[] permutation)
        {
            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            double sx = Fade(x - x0);
            double sy = Fade(y - y0);

            double n00 = DotGridGradient(x0, y0, x, y, permutation);
            double n10 = DotGridGradient(x1, y0, x, y, permutation);
            double n01 = DotGridGradient(x0, y1, x, y, permutation);
            double n11 = DotGridGradient(x1, y1, x, y, permutation);

            double ix0 = Lerp(n00, n10, sx);
            double ix1 = Lerp(n01, n11, sx);
            return Lerp(ix0, ix1, sy);
        }

        // Smoothstep interpolation
        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static double Lerp(double a, double b, double t)
        {
            return a + t * (b - a);
        }

        private static double DotGridGradient(int ix, int iy, double x, double y, int[] permutation)
        {
            int gradientIndex = permutation[(permutation[ix & 255] + iy) & 255] % Gradients.Length;
            var grad = Gradients[gradientIndex];
            double dx = x - ix;
            double dy = y - iy;
            return (dx * grad.x + dy * grad.y);
        }

        private static int[] GeneratePermutation(int seed)
        {
            int[] p = new int[256];
            for (int i = 0; i < 256; i++) p[i] = i;
            var random = new Random(seed);
            for (int i = 255; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }
            int[] perm = new int[512];
            for (int i = 0; i < 512; i++)
                perm[i] = p[i % 256];
            return perm;
        }
    }
    public class ImageHandler
    {
        #region Image File Manipulation

        //  Get Bitmap at specified string
        public static Bitmap getBitmap(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "The path cannot be null.");
            }
            Bitmap bitmap = new Bitmap(Utility.Files.GetDirectory(path));
            return bitmap;
        }


        // Given a string of filepaths, load the Bitmaps at each filepath and return the array
        public static Bitmap[] getBitmapArray(String[] paths)
        {
            Bitmap[] bitmaps = new Bitmap[paths.Length];
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths), "The Paths array cannot be null.");
            }

            foreach (string path in paths)
            {
                if (path is null)
                {
                    throw new ArgumentNullException(nameof(paths), "The Paths array cannot be null.");
                }
            }
            String[] temp_paths = paths;
            for (int i = 0; i < temp_paths.Length; i++)
            {
                bitmaps[i] = new Bitmap(Utility.Files.GetDirectory(temp_paths[i]));
            }

            return bitmaps;
        }

        //  Save the bitmap as a .png file at the specificed path
        public static void saveImage(Bitmap image, String path)
        {
            if (path == null)
            {
                Console.WriteLine("Error: provided path is null");
                return;
            }

            Console.WriteLine("Saving Image: ");

            String filepath = Utility.Files.GetDirectory(path);
            Console.WriteLine(filepath);

            try { image.Save(filepath, System.Drawing.Imaging.ImageFormat.Png); }
            catch (Exception e)
            {
                Console.WriteLine("Error: filepath " + filepath + "is not valid");
            }

        }


        //  Given an array of Bitmaps, combine them in order and return the combined bitmap
        public static Bitmap CombineBitmaps(Bitmap[] images)
        {
            if (images == null || images.Length == 0)
            {
                throw new ArgumentException("The array of images cannot be null or empty.");
            }

            // Determine the dimensions of the output bitmap based on the first image
            int width = images[0].Width;
            int height = images[0].Height;

            // Ensure all images are the same size
            foreach (var image in images)
            {
                if (image.Width != width || image.Height != height)
                {
                    throw new ArgumentException("All images must have the same dimensions.");
                }
            }

            // Create a new bitmap to hold the combined image
            Bitmap combinedImage = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(combinedImage))
            {
                // Set the background of the combined image to transparent
                g.Clear(Color.Transparent);

                // Draw each image in order
                foreach (var image in images)
                {
                    g.DrawImage(image, new Rectangle(0, 0, width, height));
                }
            }

            return combinedImage;
        }

        #endregion

        #region Image Modification
        //  Get rotated idiot
        public static Bitmap RotateBitmap(Bitmap source, int amount)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Normalize rotations (every 4 is a full rotation)
            int normalizedRotations = (amount % 4 + 4) % 4;

            if (normalizedRotations == 0)
                return (Bitmap)source.Clone(); // No rotation needed

            Bitmap rotated = (Bitmap)source.Clone();
            switch (normalizedRotations)
            {
                case 1:
                    rotated.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 2:
                    rotated.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 3:
                    rotated.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            return rotated;
        }





        #endregion

    }
}
