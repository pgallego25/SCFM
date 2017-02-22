using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;

using System.Runtime.Serialization.Formatters.Binary;

namespace SCFM
{

        public class myClase
        {
            /// <summary>
            /// BASURA
            /// </summary>
            /// <returns></returns>
            public void Funcionquehacecosas()

            {

                int[,] multiDimensionalArray2 = new int[2, 2] { { 1, 2 }, { 4, 5 } };
                //DisplayPolygon(multiDimensionalArray2);

                //MessageBox.Show("palomico " + multiDimensionalArray2[1, 0]);


            }


            /// <summary>
            /// BASURA
            /// </summary>
            /// <param name="array"></param>
            /// <returns></returns>
            public string DisplayPolygon(float[,] array)
            {
                int rowLength = array.GetLength(0);
                int colLength = array.GetLength(1);
                string st = "Dimensión del Array " + rowLength.ToString() + " Columnas por " + colLength.ToString() + " Filas \n ";
                for (int i = 0; i < colLength; i++)
                {
                    for (int j = 0; j < rowLength; j++)
                    {
                        //   Console.Write(string.Format("{0} ", array[i, j]));
                        st = st + "  " + array[j, i].ToString();
                    }
                    st = st + "\n";
                    //   Console.Write(Environment.NewLine + Environment.NewLine);
                }
                Console.ReadLine();
                MessageBox.Show(st);

                return st;
            }

            /// <summary>
            /// Import a text file and returns a string containing all the file.
            /// </summary>
            /// <param name="Path">Path to the File</param>
            /// <returns></returns>
            public string ImportTextFile(string Path)
            {
                String Text = "";
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(Path))
                    {
                        // Read the stream to a string.
                        Text = sr.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);


                }
                return Text;
            }

            /// <summary>
            /// This Method Reads a PDD Eclipse File and return a 
            /// double[][] multidimensional jagged array.
            /// </summary>
            /// <param name="PDDpath">  Path to PDD Files</param>
            /// <returns></returns>
            public double[][] ReadPDDfromFile(string PDDpath)

            {
                string Text = ImportTextFile(PDDpath);

                //Check if the file contains PDD tags "%AXIS Y" or "%Type OPD"
                if (Text.IndexOf("%AXIS Z", StringComparison.OrdinalIgnoreCase) >= 0 == false ||
                        Text.IndexOf("%Type OPD", StringComparison.OrdinalIgnoreCase) >= 0 == false)
                {
                    MessageBox.Show("ERROR : The required file do not include the line %AXIS Z or the line %Type OPD"
                   + "Are you sure this is a PDD file?");
                }

                string SubString = Text.Substring(Text.IndexOf('<'));
                string[] SubSubString = (SubString.Split('\n', ' ', '<', '>'));
                double[][] PDD = new double[2][];
                int Contador = 0;
                PDD[0] = new double[SubString.Split('\n').Length - 1];
                PDD[1] = new double[SubString.Split('\n').Length - 1];

                for (int i = 3; i < SubSubString.Length; i = i + 6)
                {
                    PDD[0][Contador] = Convert.ToDouble(SubSubString[i]) / 10.0;
                    PDD[1][Contador] = Convert.ToDouble(SubSubString[i + 1]);


                    Contador++;
                }

                return PDD;

            }

            /// <summary>
            /// This method reads a ScpTable Eclipse file and returns a Tuple
            /// which contains a double[][] Jagged array with the Scp Data and
            ///  a double[] array with the Field Sizes in the table.
            /// </summary>
            /// <param name="ScpPath">  Path to Scp File </param>
            /// <returns></returns>
            public Tuple<double[][], double[]> ReadScpTable(string ScpPath)
            {
                string Text = ImportTextFile(ScpPath);
                double[] Sizes = new double[1];
                int cont = 0;
                string[] TextInLines = (Text.Split('\n'));
                for (int i = 0; i < TextInLines.GetLength(0); i++)
                {

                    if (TextInLines[i][0] != '#' && TextInLines[i][0] != '%' && TextInLines[i][0] != ' ')
                    {
                        Sizes = (Array.ConvertAll(TextInLines[i].Split(','), Double.Parse));
                        cont = i;
                        break;
                    }
                }
                double[] Aux = new double[1];
                double[][] ScpTable = new double[Sizes.Length][];
                int cont2 = 0;
                for (int i = cont + 1; i < TextInLines.GetLength(0); i++)
                {

                    if (TextInLines[i][0] != '#' && TextInLines[i][0] != '%' && TextInLines[i][0] != ' ')
                    {
                        Aux = (Array.ConvertAll(TextInLines[i].Split(','), Double.Parse));
                        ScpTable[cont2] = Aux.Skip(1).ToArray();
                        cont2 = cont2 + 1;
                    }
                }
                return new Tuple<double[][], double[]>(ScpTable, Sizes);


            }

            /// <summary>
            /// This method reads a previously created SP table and return a Tuple
            /// which contains a double[][] Jagged array with the Sp Data and
            ///  a double[] array with the Field Sizes in the table. 
            /// </summary>
            /// <param name="SpPath"> Path to Sp File</param>
            /// <returns></returns>
            public Tuple<double[], double[]> ReadSpTable(string SpPath)
            {
                //Check if the file contains PDD tags "%AXIS Y" or "%Type OPD"


                string Text = ImportTextFile(SpPath);
                string[] TextInLines = (Text.Split('\n'));
                int NFields = TextInLines.Length - 4;
                double[] Sizes = new double[NFields];
                double[] Sp = new double[NFields];

                int Contador = 0;
                for (int i = 0; i < TextInLines.GetLength(0); i++)
                {

                    if (TextInLines[i][0] != '#' && TextInLines[i][0] != ' ')
                    {
                        Sizes[Contador] = Convert.ToDouble(TextInLines[i].Split(' ')[0]);
                        Sp[Contador] = Convert.ToDouble(TextInLines[i].Split(' ')[1]);
                        Contador++;

                    }



                }
                return new Tuple<double[], double[]>(Sizes, Sp);







            }

            /// <summary>
            /// This method returns the Scp Value for a fixed Field Size
            /// given by its X and Y sizes by interpolating the Scp tables.
            /// </summary>
            /// <param name="Fx"> Field Size X direction</param>
            /// <param name="Fy">Field Size  Y direction</param>
            /// <param name="ScpPath">  Path to Scp File</param>
            /// <returns></returns>
            public double GetScpFromFieldSize(double Fx, double Fy, string ScpPath)
            {

                Tuple<double[][], double[]> Resultado = ReadScpTable(ScpPath);
                double[][] ScpTable = Resultado.Item1;
                //  ShowMultiArray(ScpTable);
                double[] Sizes = Resultado.Item2;
                int[] ListPos = SearchScalarInArray(Fx, Sizes);
                double x = Fx;
                double x0 = Sizes[ListPos[0]];
                double x1 = Sizes[ListPos[1]];
                double[] ScpInterp = new double[Sizes.Length];
                double Result = 0;
                //  ShowArray(Sizes);
                // MessageBox.Show(ListPos[0].ToString() + " " + ListPos[1].ToString());
                if (Fx < Sizes.Last() && Fx > Sizes.First())
                {
                    for (int i = 0; i < Sizes.Length; i++)
                    {
                        double y0 = ScpTable[i][ListPos[0]];
                        double y1 = ScpTable[i][ListPos[1]];
                        ScpInterp[i] = linear(x, x0, x1, y0, y1);

                    }
                }
                else if (Fx >= Sizes.Last())
                {
                    ScpInterp = ScpTable.Last();
                }
                else if (Fx <= Sizes.First())
                {
                    ScpInterp = ScpTable.First();
                }

                // ShowArray(ScpInterp);

                ListPos = SearchScalarInArray(Fy, Sizes);
                x = Fy;
                x0 = Sizes[ListPos[0]];
                x1 = Sizes[ListPos[1]];

                if (Fy < Sizes.Last() && Fy > Sizes.First())
                {

                    double y0 = ScpInterp[ListPos[0]];
                    double y1 = ScpInterp[ListPos[1]];
                    Result = linear(x, x0, x1, y0, y1);

                }
                else if (Fx >= Sizes.Last())
                {
                    Result = ScpInterp.Last();
                }
                else if (Fx <= Sizes.First())
                {
                    Result = ScpInterp.First();
                }
                return Result;
            }

            /// <summary>
            /// This method returns the Sp Valur for a field size, interpolated
            /// from the Sp tables.
            /// </summary>
            /// <param name="Size"> Field Size</param>
            /// <param name="SpPath">  Path to Sp File </param>
            /// <returns></returns>
            public double GetSPfromFieldSize(double Size, string SpPath)
            {
                double Resultado = 0;
                Tuple<double[], double[]> SpResults = ReadSpTable(SpPath);
                double[] Sizes = SpResults.Item1;
                double[] Sp = SpResults.Item2;

                if (Size >= Sizes.Last())
                {

                    Resultado = Sp.Last();

                }
                else if (Size <= Sizes.First())
                {
                    Resultado = Sp.First();
                }
                else
                {
                    int[] ListPos = SearchScalarInArray(Size, Sizes);
                    double x = Size;
                    double x0 = Sizes[(ListPos[0])];
                    double x1 = Sizes[(ListPos[1])];
                    double y0 = Sp[(ListPos[0])];
                    double y1 = Sp[(ListPos[1])];
                    Resultado = linear(x, x0, x1, y0, y1);
                    /* MessageBox.Show(ListPos[0].ToString() + " " + ListPos[1].ToString());
                     MessageBox.Show(x0.ToString() + " " + x1.ToString());
                     MessageBox.Show(y0.ToString() + " " + y1.ToString());*/
                }

                return Resultado;
            }

            /// <summary>
            /// This method returns the PDD or TPR from a fixed Field Size.
            /// If the Path is a TPRtable.osl, it will return a TPR. In other case
            /// returns a PDD. In both cases, it gets the result interpolating
            /// between the known PDD's or TPR's. The structure of the return is a Tuple, with
            /// The TPR/PDD data, the field sizes and the distance from the surface 
            /// points (called Positions).
            /// </summary>
            /// <param name="Size"> Field Size</param>
            /// <param name="Path"> Path To PDD/TPR files</param>
            /// <returns></returns>
            public Tuple<double[], double[], double[]> GetPDDorTPRfromFieldSize(double Size, string Path)
            {
                Tuple<double[][], double[], double[]> Resultados;
                double[] Sizes;
                double[] Positions;
                double[][] PDDTable;
                if (Path.Substring(Path.Length - 3) == "osl")
                {

                    Resultados = (Tuple<double[][], double[], double[]>)Deserialize(Path);
                    PDDTable = Resultados.Item1;
                    //ShowMultiArray(PDDTable);
                    Sizes = Resultados.Item2;
                    Positions = Resultados.Item3;
                }
                else
                {
                    Resultados = CreatePDDTable(Path);
                    PDDTable = Resultados.Item1;
                    Sizes = Resultados.Item2;
                    Positions = Resultados.Item3;
                }

                double[] pddFinal = new double[Positions.Length];
                int[] ListPos = SearchScalarInArray(Size, Sizes);
                if (Size >= Sizes.Last())
                {
                    pddFinal = PDDTable.Last();

                }
                else if (Size <= Sizes.First())
                {
                    pddFinal = PDDTable.First();

                }
                else
                {
                    for (int j = 0; j < Positions.Length; j++)
                    {
                        double x = Size;
                        double x0 = Sizes[ListPos[0]];
                        double x1 = Sizes[ListPos[1]];
                        double y0 = PDDTable[ListPos[0]][j];
                        double y1 = PDDTable[ListPos[1]][j];
                        pddFinal[j] = linear(x, x0, x1, y0, y1);

                    }


                }

                return new Tuple<double[], double[], double[]>(pddFinal, Sizes, Positions);

            }

            /// <summary>
            /// This method returns a double TPR result, for a given Field Size and distance
            /// from the surface, interpolating between the known TPR's.
            /// </summary>
            /// <param name="Size"> Field Size </param>
            /// <param name="d"> Distance from the surface of water</param>
            /// <param name="TPRpath"> Path to TPR Files</param>
            /// <returns></returns>
            public double GetTPRfromFieldSizeAndDistance(double Size, double d, string TPRpath)
            {

                Tuple<double[], double[], double[]> Results = GetPDDorTPRfromFieldSize(Size, TPRpath);

                double[] TPRResults = Results.Item1;
                double[] Positions = Results.Item3;
                int[] ListPos = SearchScalarInArray(d, Positions);
                // ShowArray(TPRResults);
                //  MessageBox.Show(ListPos[0].ToString() + " " + ListPos[1].ToString());
                double x = Size;
                double x0 = Positions[ListPos[0]];
                double x1 = Positions[ListPos[1]];
                double y0 = TPRResults[ListPos[0]];
                double y1 = TPRResults[ListPos[1]];
                //   MessageBox.Show(x0.ToString() + " " + x1.ToString());
                //  MessageBox.Show(y0.ToString() + " " + y1.ToString());

                double TprFinal = linear(x, x0, x1, y0, y1);
                //  ShowArray(Positions);
                return TprFinal;

            }

            /// <summary>
            /// This method create a PDD table from PDD Eclipse format files. It DON'T create
            /// any permanent file. You only need to use this function once in order to create
            /// the TPRtable, which creates a permanent File. The structure of the return is a Tuple, with
            /// The PDD data, the field sizes and the distance from the surface 
            /// points (called Positions).
            /// </summary>
            /// <param name="PDDPath">Path to PDD Files</param>
            /// <returns></returns>
            public Tuple<double[][], double[], double[]> CreatePDDTable(string PDDPath)
            {
                //string sourceDirectory = @"J:\\TRANSFER\\FISICA\\VarianScripting\\DatosClinacs\\600\\PDDs\\";
                // MessageBox.Show(sourceDirectory);

                var txtFiles = Directory.GetFiles(PDDPath);
                double[][] PDDTable = new double[txtFiles.Length][];
                double[] Positions = new double[1];
                double[] Sizes = new double[txtFiles.Length];
                int Contador = 0;
                foreach (string currentFile in txtFiles)
                {
                    //   MessageBox.Show(currentFile);
                    string Text = ImportTextFile(currentFile);
                    int Index = Text.IndexOf("FLSZ");
                    Sizes[Contador] = Convert.ToInt32(Text.Substring(Index + 5, 3)) / 10.0;


                    double[][] SinglePDD = ReadPDDfromFile(PDDPath);
                    if (Contador == 0)
                    {
                        Positions = SinglePDD[0];
                    }

                    PDDTable[Contador] = NormalizeArray(SinglePDD[1]);
                    Contador++;

                }
                Array.Sort(Sizes, PDDTable);
                return new Tuple<double[][], double[], double[]>(PDDTable, Sizes, Positions);

            }

            /// <summary>
            /// This method returns a TPRTable created from Sp and PDD's, for each field Size and
            /// distance from the surface. This metodology is based on The Physics of Radiation Therapy ,
            /// Faiz M. Khan, Fifth Edition. The structure of the return is a Tuple, with
            /// The TPR/PDD data, the field sizes and the distance from the surface 
            /// points (called Positions).
            /// </summary>
            /// <param name="PDDpath"> >Path to PDD Files</param>
            /// <param name="SPPath"> >Path to Sp File</param>
            /// <returns></returns>
            public Tuple<double[][], double[], double[]> CreateTPRTableFromPDDandSP(string PDDpath, string SPPath)
            {
                Tuple<double[][], double[], double[]> Resultados = CreatePDDTable(PDDpath);
                double[][] PDDTable = Resultados.Item1;
                double[] Sizes = Resultados.Item2;
                double[] Positions = Resultados.Item3;
                double[] PDD_d_Ad;
                double[] PDD_dref_Ad;
                double Sp_Ac1_SSD1;
                double Sp_Ac2_SSD1;

                int cont = 0;
                double MinimunSizeforPDD = 3.0;
                double MinimunSizeforSp = 4.4;
                double MaximunSizeforSp = 40.0;
                double dref = 10;
                double SSD1 = 90;

                double[][] TPR_d_Ad = new double[Sizes.Length][];
                double[] Aux = new double[Positions.Length];
                foreach (double Ac1 in Sizes)
                {


                    if (Ac1 <= MinimunSizeforPDD)
                    {
                        Tuple<double[], double[], double[]> Results = GetPDDorTPRfromFieldSize(MinimunSizeforPDD, PDDpath);
                        PDD_d_Ad = Results.Item1;
                    }
                    else
                    {
                        Tuple<double[], double[], double[]> Results = GetPDDorTPRfromFieldSize(Ac1, PDDpath);
                        PDD_d_Ad = Results.Item1;

                    }

                    if (Ac1 <= MinimunSizeforSp)
                    {


                        Sp_Ac1_SSD1 = GetSPfromFieldSize(MinimunSizeforSp, SPPath);

                    }
                    else if (Ac1 >= MaximunSizeforSp)
                    {

                        Sp_Ac1_SSD1 = GetSPfromFieldSize(MaximunSizeforSp, SPPath);
                    }
                    else
                    {

                        Sp_Ac1_SSD1 = GetSPfromFieldSize((Ac1 * (SSD1 + dref) / SSD1), SPPath);

                    }

                    TPR_d_Ad[cont] = new double[PDD_d_Ad.Length];
                    for (int j = 0; j < Positions.Length; j++)
                    {
                        double d = Positions[j];
                        double Ad = Ac1 * ((SSD1 + d) / SSD1);

                        if (Ad <= MinimunSizeforSp)
                        {
                            Sp_Ac2_SSD1 = GetSPfromFieldSize(MinimunSizeforSp, SPPath);
                        }
                        else if (Ad >= 40)
                        {
                            Sp_Ac2_SSD1 = GetSPfromFieldSize(MaximunSizeforSp, SPPath);
                        }
                        else
                        {
                            Sp_Ac2_SSD1 = GetSPfromFieldSize(Ad, SPPath);

                        }


                        if (Ad < MinimunSizeforPDD)
                        {
                            Tuple<double[], double[], double[]> Results = GetPDDorTPRfromFieldSize(MinimunSizeforPDD, PDDpath);
                            PDD_dref_Ad = Results.Item1;


                        }
                        else
                        {
                            Tuple<double[], double[], double[]> Results = GetPDDorTPRfromFieldSize(Ad, PDDpath);

                            PDD_dref_Ad = Results.Item1;

                        }//??90

                        TPR_d_Ad[cont][j] = (PDD_d_Ad[j] / PDD_dref_Ad[90]) * (Math.Pow((SSD1 + d), 2) / Math.Pow((SSD1 + dref), 2)) * 1 / (Sp_Ac2_SSD1 / Sp_Ac1_SSD1);
                        if (Ac1 == Sizes[4] && j == 74)
                        {
                            /*  MessageBox.Show(PDD_d_Ad[74].ToString());

                              MessageBox.Show(SSD1.ToString());

                              MessageBox.Show(PDD_dref_Ad[90].ToString());*/
                            MessageBox.Show((Ac1 * (SSD1 + dref) / SSD1).ToString());
                            MessageBox.Show(Ad.ToString());

                            MessageBox.Show(dref.ToString());
                            MessageBox.Show(Sp_Ac2_SSD1.ToString());
                            MessageBox.Show(Sp_Ac1_SSD1.ToString());
                            MessageBox.Show(Aux[j].ToString());
                            MessageBox.Show(cont.ToString());

                        }

                    }

                    // TPR_d_Ad[cont] = Array.Copy(Aux,TPR_d_Ad[cont], Aux.Length;TPR_d_Ad[cont]
                    /*  if (cont >= 4)
                      {
                          MessageBox.Show(TPR_d_Ad[4][74].ToString());
                          MessageBox.Show(cont.ToString());
                      }*/
                    //           MessageBox.Show(cont.ToString());

                    cont++;

                }
                MessageBox.Show(TPR_d_Ad[4][74].ToString());

                //                ShowArray(Positions);
                return new Tuple<double[][], double[], double[]>(TPR_d_Ad, Sizes, Positions);
            }


            /// <summary>
            /// This is an Auxiliary method to find the position of one given Number, 
            /// in an array. For example for a double[] {1,2,3,4}, and the Number 3.5,
            /// the Method will return the position ListPos int[]{2,3}, Which means 
            /// that the number 3.5 is in between 3 and 4. This is helpful for the
            /// interpolation carry on in the monitor unit calculations.
            /// </summary>
            /// <param name="Number">Number which position we would like to know</param>
            /// <param name="array">Array</param>
            /// <returns></returns>
            public int[] SearchScalarInArray(double Number, double[] array)
            {
                double[] ListaAbs = new double[array.Length];


                for (int i = 0; i < array.Length; i++)
                {

                    ListaAbs[i] = Math.Abs(array[i] - Number);

                }


                int Pos1 = Array.IndexOf(ListaAbs, ListaAbs.Min());
                int Pos2 = Pos1;


                if (Number <= array.First())
                {
                    Pos1 = 0;
                    Pos2 = 0;

                }
                else if (Number >= array.Last())
                {

                    Pos1 = array.Length - 1;
                    Pos2 = array.Length - 1;


                }
                else
                {

                    if ((Number >= array[Convert.ToInt32(Pos1)] || Pos1 == 0) && Pos1 != array.Length - 1)
                    {

                        Pos2 = Pos1 + 1;

                    }

                    else
                    {
                        Pos2 = Pos1 - 1;

                    }
                }


                int[] listPos = new int[] { Pos1, Pos2 };

                return listPos;

            }

            /// <summary>
            /// Linear interpolation between two points.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="x0"></param>
            /// <param name="x1"></param>
            /// <param name="y0"></param>
            /// <param name="y1"></param>
            /// <returns></returns>
            static public double linear(double x, double x0, double x1, double y0, double y1)
            {
                if ((x1 - x0) == 0)
                {
                    return (y0 + y1) / 2;
                }
                return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
            }

            /// <summary>
            /// Method to create a Messagebox showing the data from a Multidimensional Array.
            /// This is useful debuggin the code in Eclipse.
            /// </summary>
            /// <param name="MultiArray"></param>
            public void ShowMultiArray(double[][] MultiArray)
            {
                string String = "";

                for (int i = 0; i < MultiArray.Length; i++)
                {
                    for (int j = 0; j < MultiArray[0].Length; j++)
                    {

                        String = String + MultiArray[i][j].ToString() + "  ";

                    }
                    String = String + "\n";
                }
                MessageBox.Show(String);
            }

            /// <summary>
            ///  Method to create a Messagebox showing the data from an  Array.
            /// This is useful debuggin the code in Eclipse.
            /// </summary>
            /// <param name="Array"></param>
            public void ShowArray(double[] Array)
            {
                string String = "";

                for (int i = 0; i < Array.Length; i++)
                {


                    String = String + Array[i].ToString() + "  ";


                }
                MessageBox.Show(String);
            }

            /// <summary>
            /// This method normalizes an Array.
            /// </summary>
            /// <param name="Array"></param>
            /// <returns></returns>
            public double[] NormalizeArray(double[] Array)
            {
                double[] ArrayNormalized = new double[Array.Length];
                double MaxArray = Array.Max();
                for (int i = 0; i < Array.Length; i++)
                {

                    ArrayNormalized[i] = Array[i] / MaxArray;
                }
                return ArrayNormalized;

            }

            /// <summary>
            /// This method write a binary file for a given object.
            /// </summary>
            /// <param name="t"> Object to be serialized</param>
            /// <param name="path">Path to save the file</param>
            public void Serialize(object t, string path)
            {
                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, t);
                }
            }


            /// <summary>
            /// This method read a binary file and create an object.
            /// </summary>
            /// <param name="path">Path to save the file</param>
            public object Deserialize(string path)
            {
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    return bformatter.Deserialize(stream);
                }
            }
        

    }
}











