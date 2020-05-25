using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SSI_300420
{
    class Network
    {
        private Layer[] listOfLayers;
        public Synapse[][][] synaps;
        private int numLayers;
        private int numInputs;
        private int numOut;

        public Network(int numLayers, int numInputs, int numOut, Func<double, double> actFunc) //numLayers=liczba warstw wewnetrznych (mających tyle neuronów co inputów)
        {
            this.numLayers = numLayers;
            this.numInputs = numInputs;
            this.numOut = numOut;
            listOfLayers = new Layer[numLayers+1];
            for (int i = 0; i < listOfLayers.Length; i++)
            {
                listOfLayers[i] = new Layer(numInputs, actFunc);
            }
            listOfLayers[numLayers] = new Layer(numOut, actFunc); // ostatnia warstwa; neuronów tyle, co outów

            synaps = new Synapse[numLayers+1][][]; 
            for (int i = 0; i < synaps.Length; i++)
            {
                synaps[i] = new Synapse[numInputs][];
                for (int j = 0; j < synaps[i].Length; j++)
                {
                    synaps[i][j] = new Synapse[numInputs];

                }
            }
            synaps[numLayers] = new Synapse[numInputs][]; //ostatnia macierz synaps ma jeden inny wymiar (bo out)
            for (int i = 0; i < synaps[numLayers].Length; i++)
            {
                synaps[numLayers][i] = new Synapse[numOut];
            }

            for (int i = 0; i < listOfLayers[0].ListOfNeurons.Length; i++) //synapsy do 1 warstwy, petla po kazdym neuronie w 1 war
            {
                for (int j = 0; j < numInputs; j++) //po kazdym inpucie
                {
                    synaps[0][j][i] = new Synapse(listOfLayers[0].ListOfNeurons[i]);
                }
            }
            //budujemy prawostronne synapsy
            for (int i = 0; i < listOfLayers.Length-1; i++) //po każdej warstwie, pomijajac ostatnia
            {
                for (int j = 0; j < listOfLayers[i].ListOfNeurons.Length; j++) // po kazdym neuronie
                {
                    for (int k = 0; k < listOfLayers[i+1].ListOfNeurons.Length; k++) // po kazdym neuronie z nastepnej warstwy
                    {
                        synaps[i + 1][j][k] = new Synapse(listOfLayers[i].ListOfNeurons[j], listOfLayers[i + 1].ListOfNeurons[k]);
                    }
                }
            }
        }

        public Layer[] ListOfLayers
        {
            get { return listOfLayers; }
        }

        public double[] Calc(double[] inputs)
        {
            for (int k = 0; k < listOfLayers.Length; k++) //po wszystkich warstwach 
            {
                for (int i = 0; i < listOfLayers[k].ListOfNeurons.Length; i++) //po wszystkich neuronach w warstwie
                {
                    listOfLayers[k].ListOfNeurons[i].Input = 0;
                    if (k==0) //jesli mamy pierwsza warstwe
                    {
                        for (int j = 0; j < inputs.Length; j++)
                        {
                            listOfLayers[0].ListOfNeurons[i].Input += inputs[j] * synaps[0][j][i].W;
                        }

                    }
                    else
                    {
                        for (int j = 0; j < inputs.Length; j++)
                        {
                            listOfLayers[k].ListOfNeurons[i].Input += listOfLayers[k-1].ListOfNeurons[j].Out() * synaps[k][j][i].W;
                        }
                    }

                }
  
            }
            double[] output = new double[numOut];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = listOfLayers[listOfLayers.Length - 1].ListOfNeurons[i].Out();
            }

            return output;


        }

        public void pokaSynaps()
        {
            for (int i = 0; i < synaps.Length; i++)
            {
                for (int j = 0; j < synaps[i].Length; j++)
                {
                    for (int k = 0; k < synaps[i][j].Length; k++)
                    {
                        Console.Write($"{synaps[i][j][k].W} ");
                    }
                    Console.Write("##");
                }
                Console.WriteLine(" ");
            }
        }

    }
    class Layer
    {
        private Neuron[] listOfNeurons;

        public Layer(int num, Func<double, double> actFunc)
        {
            listOfNeurons = new Neuron[num];
            for (int i = 0; i < listOfNeurons.Length; i++)
            {
                listOfNeurons[i] = new Neuron(actFunc);
            }
        }

        public Neuron[] ListOfNeurons
        {
            get { return listOfNeurons; }
        }
    }
    class Neuron
    {
        private Func<double, double> actFunc;
        private double input;
        public Neuron(Func<double,double> actFunc)
        {
            this.actFunc = actFunc;
            input = 0;
        }

        public double Out()
        {
            return actFunc(input);
        }
        public double Input
        {
            get { return input; }
            set { input = value; }
        }
    }
    class Synapse
    {
        private Neuron neuronL;
        private Neuron neuronR;
        private double w;
        Random random = new Random();  
     

        public Synapse(Neuron neuronR, double w)
        {
            this.neuronR = neuronR;
            this.w = w;
        }
        public Synapse(Neuron neuronR)
        {
            this.neuronR = neuronR;
            w=random.Next(0,100)/100.0;
        }
        public Synapse(Neuron neuronL, Neuron neuronR, double w)
        {
            this.neuronL = neuronL;
            this.neuronR = neuronR;
            this.w = w;
        }
        public Synapse(Neuron neuronL, Neuron neuronR)
        {
            this.neuronL = neuronL;
            this.neuronR = neuronR;
            w = random.Next(0, 100)/100.0;
        }

        public double W
        {
            get { return w; }
            set { w = value; }
        }

    }
    
    class Program
    {
        static double[][] pobierz(string addr) //pobiera lokalizacje pliku i zamienia dane na macierz 
        {
            string[] lines = File.ReadAllLines(addr);
            double[][] data = new double[lines.Length][];

            for (int i = 0; i < lines.Length; i++)
            {
                string[] tmp = lines[i].Split(','); //dane rozdzielone są przecinkiem. kropka oznacza część niecałkowitą
                data[i] = new double[tmp.Length + 2]; //o 2 wiekszy, bo dodajemy 3-el ciąg 0/1 opisujący gatunek
                for (int j = 0; j < tmp.Length - 1; j++) // wszystkie elementy oprócz nazwy 
                {
                    data[i][j] = Convert.ToDouble(tmp[j].Replace(".", ",")); //odczytuje liczby z przecinkiem, więc trzeba zmienić
                }
                // iris setosa: 001, iris versicolor: 010, Iris-virginica: 100
                if (tmp[4] == "Iris-setosa")
                {
                    data[i][4] = 0;
                    data[i][5] = 0;
                    data[i][6] = 1;

                }
                else if (tmp[4] == "Iris-versicolor")
                {
                    data[i][4] = 0;
                    data[i][5] = 1;
                    data[i][6] = 0;
                }
                else
                {
                    data[i][4] = 1;
                    data[i][5] = 0;
                    data[i][6] = 0;
                }
            }

            return data;
        }

        static double[][] normalizuj(double[][] data)
        {
            double[] min = new double[4]; //chcemy min/max 4 pierwszych kolumn
            double[] max = new double[4];
            for (int i = 0; i < min.Length; i++)
            {
                min[i] = data[0][i];
                max[i] = data[0][i];
            }

            //szukamy max min dla kazdej kolumny
            for (int i = 0; i < data.Length; i++) // i wiersz, j kolumna 
            {
                for (int j = 0; j < 4; j++)
                {
                    if (data[i][j] > max[j])
                        max[j] = data[i][j];
                    if (data[i][j] < min[j])
                        min[j] = data[i][j];
                }


            }
            //normalizacja
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    data[i][j] = ((data[i][j] - min[j]) / (max[j] - min[j])) * (1 - 0) + 0;
                }
            }
            return data;
        }

        static void Backpropagation(Network network, double[] input, double[] target, double alpha)
        {
            double Etotal = 0;
            for (int i = 0; i < target.Length; i++)
            {
                Etotal += (1.0 / 2) * Math.Pow((target[i] - network.Calc(input)[i]), 2);
            }
            Console.WriteLine($"Błąd: {Etotal}");

            //algorytm dla 2 warstw
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < target.Length; j++)
                {
                    network.synaps[1][i][j].W -= alpha* deltaR(target[j],
                                                               network.Calc(input)[j],
                                                               network.ListOfLayers[1].ListOfNeurons[j].Input,
                                                               network.ListOfLayers[0].ListOfNeurons[i].Out());
                }
            }

            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < input.Length; j++)
                {
                    for (int k = 0; k < target.Length; k++) //po outputach
                    {
                        network.synaps[0][i][j].W -= alpha * deltaL(target[k],
                                                            network.Calc(input)[k],
                                                            network.ListOfLayers[1].ListOfNeurons[k].Input,
                                                            network.synaps[1][j][k].W,
                                                            network.ListOfLayers[0].ListOfNeurons[j].Input,
                                                            input[i]);
                    }
                    
                                                            
                }
            }

        }
        static double deltaR(double target, double out1, double in1, double out0 )
        {
            return -(target - out1) * DerivativeOfAF(in1) * out0;
        }
        static double deltaL(double target, double out1, double in1, double w, double in0, double input)
        {
            //double sum = 0;
            //for (int i = 0; i < target.Length; i++)
            //{
            //    sum += -(target[i] - out1[i]) * DerivativeOfAF(in1[i]) * w[i];
            //}
            double sum= -(target - out1) * DerivativeOfAF(in1) * w;
            return sum * DerivativeOfAF(in0) * input;
        }
        static double ActivationFunction(double x)
        {
            return 1 / (1 + Math.Pow(Math.E, -x));
        }
        static double DerivativeOfAF(double x)
        {
            return ActivationFunction(x) * (1 - ActivationFunction(x));
        }
        

        static void Main(string[] args)
        {


            Network network = new Network(1, 4, 3, ActivationFunction);
            

            string addr = @"C:\Users\Dell\OneDrive\Pulpit\Systemy sztucznej inteligencji\dane.txt";
            double[][] data = normalizuj(pobierz(addr));
            for (int j = 0; j < 150; j++)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    double[] input = new double[4];
                    Array.Copy(data[i], input, 4);
                    double[] target = new double[3];
                    Array.Copy(data[i], 4, target, 0, 3);
                    Backpropagation(network, input, target, 0.2);
                }
            }


            //4.3,3.0,1.1,0.1,Iris - setosa 001
            // 6.0,2.9,4.5,1.5,Iris-versicolor 010
            //6.9,3.1,5.1,2.3,Iris-virginica 100
            //5.8,2.7,5.1,1.9,Iris - virginica
            Console.WriteLine("#####################################");

            network.pokaSynaps();
            Console.WriteLine("#####################################");
            double[] proba1 = new double[] { 4.3, 3.0, 1.1, 0.1 };
            for (int i = 0; i < 3; i++)
            {
                Console.Write($"{network.Calc(proba1)[i]} ");
            }
            Console.WriteLine("#####################################");
            double[] proba2 = new double[] { 6.0, 2.9, 4.5, 1.5 };
            for (int i = 0; i < 3; i++)
            {
                Console.Write($"{network.Calc(proba2)[i]} ");
            }
            Console.WriteLine("#####################################");
            double[] proba3 = new double[] { 6.9, 3.1, 5.1, 2.3 };
            for (int i = 0; i < 3; i++)
            {
                Console.Write($"{network.Calc(proba3)[i]} ");
            }
            Console.WriteLine("#####################################");
            double[] proba4 = new double[] { 5.8, 2.7, 5.1, 1.9 };
            for (int i = 0; i < 3; i++)
            {
                Console.Write($"{network.Calc(proba4)[i]} ");
            }
            Console.WriteLine("#####################################");



            Console.ReadKey();
        }

    }
}
