using Newtonsoft.Json;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace GeradorMassaRobo
{
    public partial class Form1 : Form
    {

        bool json;
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            inicializarOpenFile();

            DialogResult dr = this.openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                txtOpenFile.Text = openFileDialog1.FileName;
                try
                {
                    LerEConverterJsonEmXML();
                }
                catch(Exception Ex)
                {
                    txtOpenFile.Text = string.Empty;
                    MessageBox.Show(Ex.Message, "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                AtualizarBotaoGerarXML();
            }
            else
            {
                inicializarOpenFile();
            }
        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!System.IO.File.Exists(openFileDialog1.FileName)){
                    MessageBox.Show("Favor informar um arquivo", "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    inicializarOpenFile();
                }
                else
                {
                    XmlDocument xml = LerEConverterJsonEmXML();
                    String conteudoXML = xml.InnerXml;
                    formatarSaidaArquivo(conteudoXML, 52);

                    if (checkBoxGeraXML.Checked)
                    {
                        geraArquivoXML(xml);
                    }

                    MessageBox.Show("Processo concluído com sucesso", "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    inicializarOpenFile();
                }
            }
            catch(Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                inicializarOpenFile();
            }
        }

        private string RemoverAcentuacao(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private XmlDocument LerEConverterJsonEmXML()
        {
            string conteudo = string.Empty;
            try
            {
                StreamReader x;
                string Caminho = openFileDialog1.FileName;
                x = File.OpenText(Caminho);
                Console.OpenStandardOutput();
                while (x.EndOfStream != true)
                {
                    String conteudoLinha = x.ReadLine();
                    conteudo += conteudoLinha.Trim();
                }
                conteudo = RemoverAcentuacao(conteudo);
                conteudo = Regex.Replace(conteudo, "[^\\][^0-9A-Za-z-</>'\",-.{}()+:; ]", "");
                x.Close();
            }
            catch
            {
                throw new Exception("Ocorreu um erro na leitura do arquivo");
            }
                try
                {
                    XmlDocument retorno = (XmlDocument)JsonConvert.DeserializeXmlNode(conteudo, "root");
                    json = true;
                    return retorno;
                }
                catch
                {
                    try
                    {

                        XmlDocument retorno = new XmlDocument();
                        retorno.LoadXml(conteudo);
                        json = false;
                        return retorno;
                    }
                    catch
                    {
                        throw new Exception("Favor informar um arquivo no formato XML ou JSON");
                    }
                }
        }

        private void geraArquivoXML(XmlDocument xml)
        {
            String caminho = openFileDialog1.FileName;
            String nomeArquivo = openFileDialog1.SafeFileName.Split('.')[0];
            caminho = caminho.Replace(openFileDialog1.SafeFileName, "");
            string CaminhoNome = caminho + "XML - " + nomeArquivo + ".xml";
            xml.Save(CaminhoNome);
        }

        public void formatarSaidaArquivo(String entrada,
        int quantidadeCaracteres)
        {
            try
            {
                StreamWriter arquivo;
                String caminho = openFileDialog1.FileName;
                String nomeArquivo = openFileDialog1.SafeFileName.Split('.')[0];
                caminho = caminho.Replace(openFileDialog1.SafeFileName, "");
                string CaminhoNome = caminho + "FORMATADO - " + nomeArquivo + ".txt";
                arquivo = File.CreateText(CaminhoNome);

                if (!string.Empty.Equals(entrada))
                {

                    var split = entrada.Select((c, index) => new { c, index })
                        .GroupBy(x => x.index / quantidadeCaracteres)
                        .Select(group => group.Select(elem => elem.c))
                        .Select(chars => new string(chars.ToArray()));

                   arquivo.WriteLine("          05 WRK-BLOCO-MENSAGEM.                                        ");
                    int cont = 0;
                    foreach (var str in split)
                    {
                        cont++;
                        String espaco = "      ";
                        String contTamanho = Convert.ToString(cont);
                        int tamanho = 6;
                        tamanho = tamanho - contTamanho.Length;
                        espaco = espaco.Substring(0, tamanho);

                        arquivo.WriteLine("             10  WRK-MENSAGEM-P" + cont + espaco + "PIC X(00" + RetornaTamanhoVariavel(str.Length) + ") VALUE                  ");
                        arquivo.WriteLine("             '" + str + "'.    ");
                    }
                }
                arquivo.Close();
            }
            catch
            {
                throw new Exception("Ocorreu um erro ao processar esta ação");
            }
        }
        private string RetornaTamanhoVariavel(int valor)
        {
            {
                String tamanho = Convert.ToString(valor);

                if (tamanho.Length < 2)
                {
                    return "0" + tamanho.Length;
                }
                return tamanho;
            }
        }

        private void rb_Json_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxGeraXML.Visible = true;
        }

        private void rb_Xml_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxGeraXML.Visible = false;
        }

        private void inicializarOpenFile()
        {
            openFileDialog1 = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Selecionar Arquivo",
                InitialDirectory = @"C:\",
                Filter = "Todos os arquivos (*.*)|*.*|Documentos de texto (*.txt;*.json;*.xml)|*.txt;*.TXT;*.json;*.Json;*.xml;*.XML",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 2,
                RestoreDirectory = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            txtOpenFile.Text = string.Empty;
            json = false;
            AtualizarBotaoGerarXML();
        }


        private void AtualizarBotaoGerarXML()
        {
            checkBoxGeraXML.Enabled = json;
            checkBoxGeraXML.Checked = json;
        }
    }
}
