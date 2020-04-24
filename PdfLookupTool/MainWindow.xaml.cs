using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace PdfLookupTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IList<string> PdfPaths = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var searchTerms = (TextBox)this.FindName("search_terms");
            if (PdfPaths.Count == 0 || string.IsNullOrWhiteSpace(searchTerms.Text))
            {
                MessageBox.Show("No ha seleccionado archivos o ingresado terminos de busqueda");
            } else
            {
                var terms = searchTerms.Text.Split(Environment.NewLine);
                try
                {
                    AnalizePDFs(terms);
                }
                catch (Exception err) {
                    MessageBox.Show(err.ToString());
                    System.Console.WriteLine(err.ToString());
                }
            }
        }

        public class SearchResult
        {
            public string Name { get; set; }
            public Dictionary<string, int> Ocurrences { get; set; }
        }

        private IList<SearchResult> results;
        private void AnalizePDFs(IList<string> terms)
        {
            results = new List<SearchResult>();
            var errors = new StringBuilder();
            var searches = (from term in terms
                        from document in PdfPaths
                        where !string.IsNullOrWhiteSpace(term) && term != Environment.NewLine
                        select  new { Pdf = document, Terms = terms });
            foreach (var search in searches)
            {
                try
                {
                    var docReader = new PdfSearcher(search.Pdf);
                    var result = new SearchResult { Name = search.Pdf, Ocurrences = new Dictionary<string, int>() };
                    search.Terms.ToList<string>()
                        .ForEach(term => result.Ocurrences[term] = docReader.Ocurrences(term));
                    docReader.Close();
                    results.Add(result);
                } catch(Exception error)
                {
                    // Found errors
                    errors.Append(search.Pdf);
                    errors.Append(Environment.NewLine);
                }
            }
            ShowResults(results);
        }

        private void ShowResults(IList<SearchResult> results)
        {
            TextBlock selectedPdfs = (TextBlock)this.FindName("search_results");
            var builder = new StringBuilder();
            var sortedResults = from result in results
                                let count = result.Ocurrences.Values.Aggregate((a, b) => a + b)
                                orderby count descending
                                select result;

            var panel = (StackPanel)this.FindName("opener");
            panel.Children.Clear();

            foreach (var result in sortedResults)
            {
                builder.Append(result.Name);
                builder.Append(" - ");
                foreach(var key in result.Ocurrences.Keys)
                {
                    builder.Append($" {key} {result.Ocurrences[key]} ");
                }

                builder.Append(Environment.NewLine);
                builder.Append(Environment.NewLine);


                var openBtn = new Button();
                openBtn.Content = builder.ToString();

                panel.Children.Add(openBtn);

            }
            selectedPdfs.Text = builder.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var file = new OpenFileDialog();
            file.Multiselect = true;
            file.Title = "Seleccion directorio de PDF a buscar";
            var result = file.ShowDialog();
            if (result.HasValue)
            {
                var selectedFiles = new StringBuilder();
                PdfPaths.Clear();
                foreach(var name in file.FileNames.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    PdfPaths.Add(name);
                    selectedFiles.Append(name).Append(Environment.NewLine);
                }
                TextBlock selectedPdfs = (TextBlock)this.FindName("selected_pdfs");
                selectedPdfs.Text = selectedFiles.ToString();
            } 
        }
    }
}
