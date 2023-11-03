using SPRNetTool.Data;
using SPRNetTool.Domain;
using SPRNetTool.Domain.Base;
using SPRNetTool.Domain.Utils;
using SPRNetTool.Utils;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;
using static SPRNetTool.Utils.BitmapUtil;

namespace SPRNetToolTest.Utils
{
    public class BitmapUtilTest
    {
        [SetUp]
        public void Setup()
        {
        }

        struct MS
        {
            public int x;
            public int y;
        }

        public enum Variable
        {
            NUM1 = 0b00000001,
            NUM2 = 0b00000010,
            NUM3 = 0b00000100,
            NUM4 = 0b00001000,
        }

        [Test]
        public void test_CountPerformance()
        {
            var srcObs = new ObservableCollection<string>();

            for (int i = 0; i < 10000000; i++)
            {
                srcObs.Add("1");
            }

            var st6 = DateTime.Now;
            var count6 = (srcObs as Collection<string>).Count;
            var time6 = (DateTime.Now - st6).TotalMilliseconds;
            Debug.WriteLine($"{count6}, time = {time6}ms");

            var st5 = DateTime.Now;
            var count5 = (srcObs as IList<string>).Count;
            var time5 = (DateTime.Now - st5).TotalMilliseconds;
            Debug.WriteLine($"{count5}, time = {time5}ms");

            var st3 = DateTime.Now;
            var count3 = srcObs.Count;
            var time3 = (DateTime.Now - st3).TotalMilliseconds;
            Debug.WriteLine($"{count3}, time = {time3}ms");

            var st = DateTime.Now;
            var count1 = srcObs.Count();
            var time = (DateTime.Now - st).TotalMilliseconds;
            Debug.WriteLine($"{count1}, time = {time}ms");

            var st2 = DateTime.Now;
            var count2 = (srcObs as ICollection).Count;
            var time2 = (DateTime.Now - st2).TotalMilliseconds;
            Debug.WriteLine($"{count2}, time = {time2}ms");

            var st4 = DateTime.Now;
            var count4 = (srcObs as ICollection<string>).Count;
            var time4 = (DateTime.Now - st4).TotalMilliseconds;
            Debug.WriteLine($"{count4}, time = {time4}ms");
        }

        [Test]
        public void test_StringFormula()
        {
            var input = new double[] { 1, 3, 5 };
            var expression = "X1*X2-X3";
            var pattern = @"(?<O>[\+\-\*\/]*)(?<N>X\d+)";
            var matches = Regex.Matches(expression, pattern);
            List<string> numbers = new List<string>();
            List<string> operators = new List<string>();
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    if (match.Groups["O"].Value != "")
                    {
                        operators.Add(match.Groups["O"].Value);
                    }

                    if (match.Groups["N"].Value != "")
                    {
                        numbers.Add(match.Groups["N"].Value);
                    }
                }
            }
            Assert.That(operators.Count == numbers.Count - 1);
        }

        [Test]
        public void test_StringFormula2()
        {
            var input = string.Format("( {0} - {1} ) * {2}", new object[] { 1, 3, 5 });
            var expression = "( 1 - 2 ) * 3";
            var in2Pos = InfixToPostfix(expression);
            var x = EvaluatePostfixExpression(in2Pos);
        }
        static string InfixToPostfix(string infix)
        {
            Dictionary<string, int> precedence = new Dictionary<string, int>
            {
                { "+", 1 },
                { "-", 1 },
                { "*", 2 },
                { "/", 2 },
            };

            Stack<string> operators = new Stack<string>();
            List<string> output = new List<string>();

            string[] tokens = infix.Split(' ');

            foreach (string token in tokens)
            {
                if (double.TryParse(token, out double number))
                {
                    output.Add(token);
                }
                else if (token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    while (operators.Count > 0 && operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Pop();
                }
                else if (operators.Count == 0 || operators.Peek() == "(" || precedence[token] > precedence[operators.Peek()])
                {
                    operators.Push(token);
                }
                else
                {
                    while (operators.Count > 0 && operators.Peek() != "(" && precedence[token] <= precedence[operators.Peek()])
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
            }

            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }

            return string.Join(" ", output);
        }


        static double EvaluatePostfixExpression(string postfix)
        {
            Stack<double> stack = new Stack<double>();
            string[] tokens = postfix.Split(' ');

            foreach (string token in tokens)
            {
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else
                {
                    double operand2 = stack.Pop();
                    double operand1 = stack.Pop();

                    switch (token)
                    {
                        case "+":
                            stack.Push(operand1 + operand2);
                            break;
                        case "-":
                            stack.Push(operand1 - operand2);
                            break;
                        case "*":
                            stack.Push(operand1 * operand2);
                            break;
                        case "/":
                            stack.Push(operand1 / operand2);
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported operator: " + token);
                    }
                }
            }

            return stack.Pop();
        }


        [Test]
        public void test_Performance()
        {
            var imgSize = 100000;

            var time1 = new DateTime();
            var time2 = time1.AddDays(1);
            var time3 = time2 - time1;
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                watch.Restart();
                var globalData = new PaletteColor[imgSize];
                for (long datidx = 0; datidx < imgSize; datidx++)
                {
                    globalData[datidx].Red = 0xFF;
                    globalData[datidx].Green = 0xFF;
                    globalData[datidx].Blue = 0xFF;
                    globalData[datidx].Alpha = 0xFF;
                }
                var initTime = watch.ElapsedMilliseconds;
                Debug.WriteLine($"initTime={initTime}");
                watch.Restart();
                var newArray = new PaletteColor[imgSize];
                Array.Copy(globalData, newArray, imgSize);
                var CopyTime = watch.ElapsedMilliseconds;
                Debug.WriteLine($"CopyTime={CopyTime}");
            }
            var x = 10;

        }

        [Test]
        public void test_SaveBitmapSourceToSprFile()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            ISprWorkManager swm = new SprWorkManager();
            swm.SaveBitmapSourceToSprFile(bmpSource, "Resources\\test.spr");
        }

        [Test]
        public void test_ConvertBitmapSourceToPaletteColorArray()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            var bdm = new BitmapDisplayManager();
            Assert.NotNull(bmpSource);
            var bytearray = bdm.ConvertBitmapSourceToByteArray(bmpSource);
            var palarray = bdm.ConvertBitmapSourceToPaletteColorArray(bmpSource);
            var palarrayToByte = bdm.ConvertPaletteColourArrayToByteArray(palarray);
            Assert.That(bdm.AreByteArraysEqual(bytearray, palarrayToByte));
        }

        [Test]
        public void test()
        {
            var n1 = Convert.ToInt64(Variable.NUM1);
            Variable num = Variable.NUM1 | Variable.NUM2 | Variable.NUM3;
            if (num.HasAllFlagsOf(Variable.NUM1, Variable.NUM2))
            {
                var x = 1;
            }

            if (num.HasFlag(Variable.NUM2 | Variable.NUM4))
            {
                var x = 1;
            }

            if (num.HasFlag(Variable.NUM3))
            {
                var x = 1;
            }
            if (num.HasFlag(Variable.NUM4))
            {
                var x = 1;
            }
        }

        [Test]
        public void TestLoadBitmapSourceFromFile()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            Assert.That(bmpSource.PixelWidth * bmpSource.PixelHeight, Is.EqualTo(90000));
        }

        [Test]
        public async Task TestCountColors()
        {
            string imagePath = "Resources\\test.png".FullPath();
            var bmpSource = LoadBitmapFromFile(imagePath);
            Assert.NotNull(bmpSource);
            var src = await CountColorsAsync(bmpSource);
            Assert.That(src.Count, Is.EqualTo(4));

            var redColor = Color.FromArgb(255, 237, 28, 36);
            var yellowColor = Color.FromArgb(255, 255, 242, 0);
            var greenColor = Color.FromArgb(255, 34, 177, 76);
            var blueColor = Color.FromArgb(255, 0, 162, 232);

            Assert.That(src[redColor], Is.EqualTo(21456));
            Assert.That(src[yellowColor], Is.EqualTo(23999));
            Assert.That(src[greenColor], Is.EqualTo(22499));
            Assert.That(src[blueColor], Is.EqualTo(22046));
        }
    }
}