using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Threading;

namespace LearnResistorColors
{


    /// <summary>
    /// Form principal.
    /// </summary>
    public class MainForm : Form
    {

        // ---------------------------------------------------------------------------
        // DECLARATIONS:

        private Color[] _colors;
        private string[] _colorNames;
        private double[] _e24, _e48, _e96;
        private Random _rand;
        private Rectangle[] _stripes;
        private Point[] _texts;
        private bool _waitingMode, _formClosing;
        private Font _font;
        private string _help, _formTitle;
        private int _counter;

        // ---------------------------------------------------------------------------
        // CONSTRUCTEURS:

        public MainForm()
        {

            // Initialisation des variables:
            _colors = new Color[]{Color.Silver, Color.Gold, Color.Black, Color.Brown,
                Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple, Color.Gray, Color.White};
            //_colorNames = new string[]{"argent", "or", "noir", "brun", "rouge", "orange", "jaune", "vert", "bleu", "violet",
            //    "gris", "blanc"};
            _colorNames = new string[]{"silver", "gold", "black", "brown", "red", "orange", "yellow", "green", "blue", "violet",
                "gray", "white"};
            _rand = new Random();
            _e24 = new double[] { 10, 11, 12, 13, 15, 16, 18, 20, 22, 24, 27, 30, 33, 36, 39, 43, 47, 51, 56, 62, 68, 75, 82, 91 };
            _e48 = new double[] { 100, 105, 110, 115, 121, 127, 133, 140, 147, 154, 162, 169, 178, 187, 196, 205, 215, 226, 237, 249, 261, 274, 287, 301, 316, 332, 348, 365, 383, 402, 422, 442, 464, 487, 511, 536, 562, 590, 619, 649, 681, 715, 750, 787, 825, 866, 909, 953 };
            _e96 = new double[] { 100, 102, 105, 107, 110, 113, 115, 118, 121, 124, 127, 130, 133, 137, 140, 143, 147, 150, 154, 158, 162, 165, 169, 174, 178, 182, 187, 191, 196, 200, 205, 210, 215, 221, 226, 232, 237, 243, 249, 255, 261, 267, 274, 280, 287, 294, 301, 309, 316, 324, 332, 340, 348, 357, 365, 374, 383, 392, 402, 412, 422, 432, 442, 453, 464, 475, 487, 499, 511, 523, 536, 549, 562, 576, 590, 604, 619, 634, 649, 665, 681, 698, 715, 732, 750, 768, 787, 806, 825, 845, 866, 887, 909, 931, 953, 976 };
            _waitingMode = false; _formClosing = false;
            _texts = new Point[] { new Point(10, 10), new Point(10, 140) };
            _stripes = new Rectangle[5]; int start = 10;
            for (int i = 0; i < 5; i++) { _stripes[i] = new Rectangle(start, 50, 40, 80); start += 50; }
            _font = new Font(this.Font.FontFamily, 16F);
            //_help = "Appuyer sur \"Espace\" ou cliquer pour afficher une question (anneaux ou valeur).\r\n";
            //_help += "Appuyer sur \"B\" pour afficher une question (anneaux).\r\n";
            //_help += "Apuyer sur \"V\" pour afficher une question (valeur).";
            _help = "Press \"Space\" or clic to show a question (bands or value).\r\n";
            _help += "Press \"B\" to show a band question.\r\n";
            _help += "Press \"V\" to show a value question.";
            _formTitle = "Learn Resistor Colors";
            _counter = 0;

            // Initialisation du form:
            this.Text = _formTitle;
            this.Width = 400;
            this.Height = 210;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown);
            this.FormClosing += delegate { _formClosing = true; };
            this.Click += delegate { SendKeys.Send(" "); };

            // Affichage de l'aide:
            this.Paint += new PaintEventHandler(MainForm_Paint);

            // En français:
            Application.CurrentCulture = new CultureInfo("en-US");

        }


        // ---------------------------------------------------------------------------
        // METHODES:


        /// <summary>
        /// Affiche le texte d'aide lors du premier affichage de la fenêtre.
        /// </summary>
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            this.Paint -= MainForm_Paint;
            Graphics g = this.CreateGraphics();
            g.DrawString(_help, this.Font, new SolidBrush(Color.Black), _texts[0]);
        }


        /// <summary>
        /// Réagit aux touches du claviers.
        /// </summary>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {

            // Si Waiting mode, affiche la réponse, quelque soit la touche:
            if (_waitingMode) { _waitingMode = false; return; }

            // Si Echap, sort:
            // Si espace, S ou V, affiche une question au harsard:
            switch (e.KeyCode)
            {
                case Keys.Escape: this.Close(); return;
                case Keys.Space: AskQuestion(2); break;
                case Keys.B: AskQuestion(0); break;
                case Keys.V: AskQuestion(1); break;
                default: MessageBox.Show(_help); break;
            }

        }


        /// <summary>
        /// Affiche des anneaux de couleurs et attend une réponse.
        /// </summary>
        private void AskQuestion(int type)
        {

            // Choisit la tolérance puis choisit un nombre dans une série, en fonction de la tolérance:
            int tol = _rand.Next(1, 9), colIndexTol;
            double[] arr;
            switch (tol)
            {
                case 1: arr = _e96; colIndexTol = 3; break;
                case 2: arr = _e48; colIndexTol = 4; break;
                default: arr = _e24; tol = 5; colIndexTol = 1; break; // pour tol différent de 1 ou 2, on met à 5
            }
            double e_val = arr[_rand.Next(0, arr.Length)];

            // Choisit un multiplicateur et calcul la valeur totale:
            int mul = _rand.Next(-2, 7);
            double val = e_val * Math.Pow(10, mul);

            // Récupère les 3 premiers chiffres significatifs:
            string str = e_val.ToString();
            int stripe1 = Int32.Parse(str.Substring(0, 1));
            int stripe2 = Int32.Parse(str.Substring(1, 1));
            int stripe3 = (tol == 5 ? -101 : Int32.Parse(str.Substring(2, 1)));

            // No de la question:
            this.Text = String.Format("{0} - #{1}", _formTitle, ++_counter);

            // Il ne reste plus qu'à afficher les données, en choisissant soit les couleurs, soit les nombres
            // de façon aléatoire, puis d'attendre une touche, et puis enfin d'afficher la réponse:
            Action<bool> drawStripes = delegate (bool clear) { DrawStripes(new int[] { stripe1 + 2, stripe2 + 2, stripe3 + 2, mul + 2, colIndexTol }, clear); };
            Action<bool> drawValue = delegate (bool clear) { DrawValue(val, tol, clear); };
            if (type != 0 && type != 1) { type = _rand.Next(0, 2); }
            if (type == 0)
            {
                drawStripes(true);
                _waitingMode = true;
                while (_waitingMode) { Thread.Sleep(1); Application.DoEvents(); if (_formClosing) { return; } }
                drawValue(false);
            }
            else
            {
                drawValue(true);
                _waitingMode = true;
                while (_waitingMode) { Thread.Sleep(1); Application.DoEvents(); if (_formClosing) { return; } }
                drawStripes(false);
            }


        }


        /// <summary>
        /// Dessine à l'écran les anneaux.
        /// </summary>
        private void DrawStripes(int[] colorIndex, bool clear)
        {
            // Dessine les anneaux, en sautant le 3e si tol de 5%:
            Graphics g = this.CreateGraphics();
            if (clear) { g.Clear(this.BackColor); }
            string names = "";
            int c = 0;
            for (int i = 0; i < 5; i++)
            {
                if (colorIndex[i] == -99) { continue; } // Saute si 5% et si 3e anneaux (3 chiffre absent en 5%)
                g.FillRectangle(new SolidBrush(_colors[colorIndex[i]]), _stripes[c++]);
                names = names + _colorNames[colorIndex[i]] + " ";
            }
            // Dessine les noms des couleurs:
            g.DrawString(names, _font, new SolidBrush(Color.Black), _texts[1]);
            // Libère ressources:
            g.Dispose();
        }


        /// <summary>
        /// Dessine à l'écran le texte de la valeur.
        /// </summary>
        private void DrawValue(double val, int tol, bool clear)
        {
            Graphics g = this.CreateGraphics();
            if (clear) { g.Clear(this.BackColor); }
            string s = String.Format("{1} ({0} Ω), Tol: {2} %", FormatFunctions.NumberToString(val, 3),
                        FormatFunctions.GetMetricSystemPrefix((decimal)val, "Ω", 0, null), tol);
            g.DrawString(s, _font, new SolidBrush(Color.Black), _texts[0]);
            g.Dispose();
        }


    }



    /// Fournit des méthodes de mise en forme de texte et de nombre.
    /// </summary>
    public static class FormatFunctions
    {
        /// <summary>


        /// <summary>
        /// Contient les préfixes métriques de Y (1E24) à y (1E-24).
        /// </summary>
        public enum MetricSystemPrefixes
        {
            y = -8,
            z = -7,
            a = -6,
            f = -5,
            p = -4,
            n = -3,
            µ = -2,
            m = -1,
            none = 0,
            k = 1,
            M = 2,
            G = 3,
            T = 4,
            P = 5,
            E = 6,
            Z = 7,
            Y = 8,
        }


        // ---------------------------------------------------------------------------


        /// <summary>
        /// Retourne une chaîne contenant le nombre nb avec le nombre maximal de décimales decimalPlaces, en suivant les patterns intPatten (partie entière) et decPattern (partie décimale) utilisant la culture ci. Si le nombre ne contient pas de décimal, retourne simplement la partie entière. S'il y a moins de décimales que decimalPlaces, retourne simplement le nombre requis de décimal, sans ajouter de 0. S'il y a plus de décimales, la dernière renvoyée est arrondie. Si ci vaut null, la culture de l'application est utilisée.
        /// </summary>
        public static string NumberToString(double nb, int decimalPlaces, string intPattern, string decPattern, CultureInfo ci)
        {
            if (ci == null) { ci = Application.CurrentCulture; }
            if (nb - Math.Truncate(nb) == 0) { return nb.ToString(intPattern, ci); }
            else { return Math.Round(nb, decimalPlaces).ToString(decPattern, ci); }
        }

        /// <summary>
        /// Voir surcharge.
        /// </summary>
        public static string NumberToString(double nb, int decimalPlaces, CultureInfo ci)
        { return NumberToString(nb, decimalPlaces, "#,0", "#,0.###############", ci); }

        /// <summary>
        /// Voir surcharge.
        /// </summary>		
        public static string NumberToString(double nb, int decimalPlaces)
        { return NumberToString(nb, decimalPlaces, "#,0", "#,0.###############", null); }

        /// <summary>
        /// Voir surcharge.
        /// </summary>
        public static string NumberToString(double nb, int decimalPlaces, string intPattern, string decPattern)
        { return NumberToString(nb, decimalPlaces, intPattern, decPattern, null); }


        // ---------------------------------------------------------------------------


        /// <summary>
        /// Retourne une chaîne contenant le nombre val mis en forme avec un préfixe métrique. unit indique l'unité à utiliser et format le format. Si val=12340, unit="g" et format=0, retourne "12.34kg". Si format=1, retourne "12kg34". La mise en forme du nombre s'effectue avec NumberToString, et la définition du préfixe avec GetMetricSystemPrefix. Si ci est null, alors la culture de l'application est utilisée.
        /// </summary>
        public static string GetMetricSystemPrefix(decimal val, string unit, int format, CultureInfo ci)
        {
            decimal newVal; MetricSystemPrefixes pref;
            if (unit == null) { unit = ""; }
            GetMetricSystemPrefix(val, out newVal, out pref);
            string p = (pref == MetricSystemPrefixes.none ? "" : pref.ToString());
            string decSeparator = Application.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string s = NumberToString((double)newVal, 3, ci);
            if (format == 0 || !s.Contains(decSeparator)) { return String.Format("{0}{1}", s, p + unit); }
            else { return s.Replace(decSeparator, p + unit); }
        }


        // ---------------------------------------------------------------------------


        /// <summary>
        /// Retourne dans newVal un nombre arrondi qui est la transformation de val adaptée au préfixe métrique pref. La partie entière a généralement au maximum 3 chiffre, sauf si val est un nombre plus grand que le plus grand préfixe de MetricSystemPrefixes. La partie décimale est toujours arrondie à 3 décimales (ou à 0, 1 ou 2 si 3 ne sont pas nécessaires). Si val est plus petit que le plus petit préfixe de MetricSystemPrefixes - 1 préfixe, alors retourne 0. (Par exemple, si le plus petit préfixe est µ et que val=123E-9, retourne 0.123, mais si val=123E-12, retourne 0.) 
        /// </summary>
        public static void GetMetricSystemPrefix(decimal val, out decimal newVal, out MetricSystemPrefixes pref)
        {
            int index = 0; decimal d = val;


            // Si la partie entière vaut 0, on descend dans les sous-multiples:
            if (val != 0 && Math.Truncate(val) == 0)
            {
                // Procédure pour supprimer la partie entière:
                Func<decimal, decimal> delIntegerPart = delegate (decimal r) { return r - Math.Truncate(r); };
                int intPart = 0;
                // Boucle:
                while (true)
                {
                    // Trois premiers chiffres après la virgule:
                    intPart = (int)Math.Truncate(delIntegerPart(d) * 1000);
                    // Mise à jour de d:
                    d *= 1000;
                    // Décrémente le compteur:
                    index--;
                    // Si intPart n'est pas nul, alors il faut s'arrêté et sortir:
                    if (intPart != 0 || index == -8)
                    {
                        newVal = (decimal)intPart + Decimal.Round(delIntegerPart(d), 3);
                        pref = (MetricSystemPrefixes)index;
                        return;
                    }
                    // Sinon, on continue:
                }
            }

            // Sinon, monte dans les multiplies:
            else
            {
                // Récupère les 3 premiers chiffres après la virgule:
                int decPart = (int)Math.Truncate((val - Math.Truncate(val)) * 1000);
                // Boucle:
                while (true)
                {
                    // S'il n'y a plus de chiffres significatifs avant les trois derniers chiffres avant la virgule,
                    // sort en retournant le résultat:
                    if (Math.Truncate(d / 1000) == 0 || index == 8)
                    {
                        newVal = Math.Truncate(d) + (decimal)decPart / 1000;
                        pref = (MetricSystemPrefixes)index;
                        return;
                    }
                    // Sinon, continue en incrémentant l'index des préfixes:
                    else
                    {
                        // Trois derniers chiffres avant la virgule:
                        decPart = (int)Math.Truncate(((d / 1000) - Math.Truncate(d / 1000)) * 1000);
                        // Met à jour d:
                        d = d / 1000;
                        // Incrémente:
                        index++;
                    }
                }
            }

        }


    }



}
