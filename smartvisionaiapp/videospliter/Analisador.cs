using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.ML.Data;
using System.Drawing;

namespace videospliter
{
    public class Analisador
    {
        static readonly string _assetsPath = Path.Combine(Environment.CurrentDirectory, "assets");
        static readonly string _trainTagsTsv = Path.Combine(_assetsPath, "inputs-train", "data", "tags.tsv");
        static readonly string _predictImageListTsv = Path.Combine(_assetsPath, "inputs-predict", "data", "image_list.tsv");
        static readonly string _trainImagesFolder = Path.Combine(_assetsPath, "inputs-train", "data");
        static string _predictSingleImage = Path.Combine(_assetsPath, "inputs-predict-single", "data", "toaster3.jpg");
        static readonly string _inceptionPb = Path.Combine(_assetsPath, "inputs-train", "inception", "tensorflow_inception_graph.pb");
        static readonly string _inputImageClassifierZip = Path.Combine(_assetsPath, "inputs-predict", "imageClassifier.zip");
        static readonly string _outputImageClassifierZip = Path.Combine(_assetsPath, "outputs", "imageClassifier.zip");
        private static string LabelTokey = nameof(LabelTokey);
        private static string PredictedLabelValue = nameof(PredictedLabelValue);
        MLContext mlContext;
        ITransformer model;

        public Analisador()
        {
            mlContext = new MLContext(seed: 1);
            mlContext.Log += MlContext_Log; ;
            Console.WriteLine("=============== Training classification model ===============");
            model = ReuseAndTuneInceptionModel(mlContext, _trainTagsTsv, _trainImagesFolder, _inceptionPb, _outputImageClassifierZip);
        }

        public classificacao AnalisarImagem(string path)
        {
            return ClassifySingleImage(mlContext, Path.GetFullPath(path), _outputImageClassifierZip, model);
        }
        
        private void MlContext_Log(object sender, LoggingEventArgs e)
        {

        }

        private struct InceptionSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const float Mean = 117;
            public const float Scale = 1;
            public const bool ChannelsLast = true;
        }

        private static ITransformer ReuseAndTuneInceptionModel(MLContext mlContext, string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            var data = mlContext.Data.LoadFromTextFile<ImageData>(path: dataLocation, hasHeader: false);
            var estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey, inputColumnName: "Label")
            .Append(mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: _trainImagesFolder, inputColumnName: nameof(ImageData.ImagePath)))
            .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: InceptionSettings.ImageWidth, imageHeight: InceptionSettings.ImageHeight, inputColumnName: "input"))
            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: InceptionSettings.ChannelsLast, offsetImage: InceptionSettings.Mean))
            .Append(mlContext.Model.LoadTensorFlowModel(inputModelLocation).
            ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
            .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: LabelTokey, featureColumnName: "softmax2_pre_activation"))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"))
            .AppendCacheCheckpoint(mlContext);
            ITransformer model = estimator.Fit(data);
            var predictions = model.Transform(data);
            var imageData = mlContext.Data.CreateEnumerable<ImageData>(data, false, true);
            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);
            var multiclassContext = mlContext.MulticlassClassification;
            var metrics = multiclassContext.Evaluate(predictions, labelColumnName: LabelTokey, predictedLabelColumnName: "PredictedLabel");
            Console.WriteLine("=============== Classification metrics ===============");
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");
            return model;
        }

        private class ImageData
        {
            [LoadColumn(0)]
            public string ImagePath;

            [LoadColumn(1)]
            public string Label;
        }

        private class ImagePrediction : ImageData
        {
            public float[] Score;

            public string PredictedLabelValue;
        }

        private static classificacao ClassifySingleImage(MLContext mlContext, string imagePath, string outputModelLocation, ITransformer model)
        {
            var imageData = new ImageData()
            {
                ImagePath = imagePath
            };
            // Make prediction function (input = ImageData, output = ImagePrediction)
            var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
            var prediction = predictor.Predict(imageData);
            classificacao c = new classificacao();
            c.tipo = prediction.PredictedLabelValue;
            c.porcentagem = prediction.Score.Max();
            return c;
        }

        public List<classificacao> analisar(string path, int deltacor,int instancia)
        {
            List<classificacao> listaclassific = new List<classificacao>();
            Bitmap bitmap = new Bitmap(new Bitmap(path), 250, 250);
            List<Point> listap = new List<Point>();
            //int deltanominal = 10;
            int deltanominal = deltacor;
            Bitmap b = new Bitmap(bitmap);
            for (int i = 0; i < 250; i++)
            {
                for (int i2 = 0; i2 < 250; i2++)
                {
                    double media = (b.GetPixel(i, i2).R + b.GetPixel(i, i2).G + b.GetPixel(i, i2).B) / 3;
                    int delta = Convert.ToInt32(media - deltanominal);
                    if (delta < -1)
                    {
                        delta = delta * -1;
                    }
                    if (delta <= 30)
                    {
                        listap.Add(new Point(i, i2));
                    }

                }
            }

            List<pointtipe> lpt = new List<pointtipe>();
            List<pointtipe> lptwatch = new List<pointtipe>();

            List<Point> listap2 = new List<Point>();

            for (int i = 0; i < 250; i += 3)
            {
                for (int i2 = 0; i2 < 250; i2 += 3)
                {
                    if (listap.FirstOrDefault(x => x.X == i && x.Y == i2) != null)
                    {
                        listap2.Add(listap.FirstOrDefault(x => x.X == i && x.Y == i2));
                    }
                }
            }


            int deltad = 10;
            foreach (var item in listap2)
            {
                lpt.Add(new pointtipe(item, -1));
            }


            int classeatual = 0;
            for (int i = 0; i < lpt.Count(); i++)
            {
                if (lpt[i].classe == -1)
                {
                    lpt[i].classe = classeatual;
                    lptwatch.Add(lpt[i]);
                    do
                    {
                        for (int i2 = 0; i2 < lptwatch.Count(); i2++)
                        {
                            foreach (var item2 in lpt.Where(x => x.classe == -1 && x.ponto != lptwatch[i2].ponto))
                            {
                                double distancia;
                                double n1 = (lptwatch[i2].ponto.X - item2.ponto.X) * (lptwatch[i2].ponto.X - item2.ponto.X);
                                double n2 = (lptwatch[i2].ponto.Y - item2.ponto.Y) * (lptwatch[i2].ponto.Y - item2.ponto.Y);
                                distancia = Math.Sqrt(n1 + n2);
                                if (distancia <= deltad && distancia >= deltad - 1)
                                {
                                    item2.classe = classeatual;
                                    lptwatch.Add(item2);

                                }
                                else if (distancia <= deltad && distancia < deltad - 1)
                                {
                                    item2.classe = classeatual;
                                }
                            }
                            lptwatch.Remove(lptwatch.First());
                        }
                    } while (lptwatch.Count() > 0);
                    classeatual++;
                }
            }
            Bitmap b2 = new Bitmap(250, 250);
            List<pointtipe> listapontosreais = new List<pointtipe>();
            foreach (var item in lpt)
            {
                if (lpt.Where(x => x.classe == item.classe).ToList().Count() > 7)
                {
                    listapontosreais.Add(item);
                }

            }

            List<List<pointtipe>> llp = new List<List<pointtipe>>();
            for (int i = 0; i < lpt.ToList().Count(); i++)
            {
                if (listapontosreais.Where(x => x.classe == i).ToList().Count() > 0)
                {
                    llp.Add(listapontosreais.Where(x => x.classe == i).ToList());
                }
            }
            int direc = 0;
            do
            {
                try
                {
                    Directory.CreateDirectory($"imagensgrid{instancia}");
                    listaclassific = new List<classificacao>();
                    int n = 0;
                    foreach (var item in llp)
                    {
                        pointtipe ltb = item.OrderBy(z => z.ponto.X).First();
                        ltb.ponto = new Point(ltb.ponto.X, item.OrderBy(z => z.ponto.Y).First().ponto.Y);
                        pointtipe rbb = item.OrderByDescending(z => z.ponto.X).First();
                        rbb.ponto = new Point(rbb.ponto.X, item.OrderByDescending(z => z.ponto.Y).First().ponto.Y);
                        if (ltb.ponto.X != rbb.ponto.X && ltb.ponto.Y != rbb.ponto.Y)
                        {
                            Bitmap bitb = new Bitmap((rbb.ponto.X - ltb.ponto.X), (rbb.ponto.Y - ltb.ponto.Y));
                            for (int i = ltb.ponto.X; i < rbb.ponto.X; i++)
                            {
                                for (int i2 = ltb.ponto.Y; i2 < rbb.ponto.Y; i2++)
                                {
                                    bitb.SetPixel(i - ltb.ponto.X, i2 - ltb.ponto.Y, bitmap.GetPixel(i, i2));
                                }
                            }
                            bitb.Save($"imagensgrid{instancia}/imgdetect{n}.png");
                            classificacao c = new classificacao();
                            c.x = ltb.ponto.X;
                            c.y = ltb.ponto.Y;
                            c.width = rbb.ponto.X - ltb.ponto.X;
                            c.height = rbb.ponto.Y - ltb.ponto.Y;
                            c.path = $"imagensgrid{instancia}/imgdetect{n}.png";
                            listaclassific.Add(c);
                            n++;
                        }
                    }
                    break;

                }
                catch (Exception)
                { }
            } while (true);
            foreach (var item in listaclassific)
            {
                classificacao s = ClassifySingleImage(mlContext, Path.GetFullPath(item.path), _outputImageClassifierZip, model);
                item.porcentagem = s.porcentagem;
                item.tipo = s.tipo;
            }
            return listaclassific;
        }

        public class pointtipe
        {
            public Point ponto { get; set; }
            public int classe { get; set; }
            public pointtipe(Point p, int n)
            {
                ponto = p;
                classe = n;
            }
        }

        private static IEnumerable<ImageData> ReadFromTsv(string file, string folder)
        {
            return File.ReadAllLines(file)
            .Select(line => line.Split('\t'))
            .Select(line => new ImageData()
            {
                ImagePath = Path.Combine(folder, line[0])
            });
        }

    }
}
