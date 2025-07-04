using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RemYardInventory
{
    public partial class Form1 : Form
    {
        public string TimeStamp { get; set; }
        public string TimeStampLong { get; set; }
        public Form1()
        {
            InitializeComponent();

            comboBox1.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox1.ItemHeight = 28;
            comboBox1.BackColor = Color.FromArgb(40, 40, 40);
            comboBox1.ForeColor = Color.White;  
            comboBox1.FlatStyle = FlatStyle.Flat;
            comboBox1.DropDownHeight = 150;

            comboBox1.DrawItem += comboBox1_DrawItem;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ImportXMLData();
        }

        private void ImportXMLData()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(documentsPath, "Remnant-Data.xml");

            // If the file doesn't exist, create it with the correct XML structure
            if (!File.Exists(filePath))
            {
                string defaultXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Remnants>
                <Remnant>
                    <RemnantNumber></RemnantNumber>
                    <ItemNumber></ItemNumber>
                    <Weight></Weight>
                    <HeatNumber></HeatNumber>
                </Remnant>
            </Remnants>";
                File.WriteAllText(filePath, defaultXml);
            }

            // Load and display the XML data
            using (XmlReader xmlFile = XmlReader.Create(filePath, new XmlReaderSettings()))
            {
                DataSet ds = new DataSet();

                try
                {
                    ds.ReadXml(xmlFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading XML: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if the DataSet contains any tables
                if (ds.Tables.Count > 0)
                {
                    // Set DataGridView's DataSource to the first table in the DataSet
                    dataGridView1.DataSource = ds.Tables[0];
                }
                else
                {
                    MessageBox.Show("No data found in the XML file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void SaveXMLData(object sender, EventArgs e)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(documentsPath, "Remnant-Data.xml");

            // Check if DataGridView has data (at least one row)
            if (dataGridView1.DataSource is DataTable dt)
            {
                // If the table is empty, add a blank row before saving
                if (dt.Rows.Count == 0)
                {
                    DataRow drToAdd = dt.NewRow();
                    drToAdd["RemnantNumber"] = "";
                    drToAdd["ItemNumber"] = "";
                    drToAdd["Weight"] = "";
                    drToAdd["HeatNumber"] = "";
                    dt.Rows.Add(drToAdd);
                }

                // Save the DataTable to XML
                dt.WriteXml(filePath);
            }
            else
            {
                MessageBox.Show("No data to save.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchBox(object sender, EventArgs e)
        {
            string SearchColumn = comboBox1.Text;
            string SearchValue = textBox9.Text;
            SearchValue = SearchValue.Replace("]", "[]]");
            SearchValue = SearchValue.Replace("[", "[[]");
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter =
            string.Format(SearchColumn + " LIKE '{0}%' OR " + SearchColumn + " LIKE '%{0}%'", SearchValue);
         }

        private void HandleRemnantNotFound(string timeStamp, string searchValue)
        {
            MessageBox.Show("Remnant not found.  Please check data entered, or use manual search below.");
            dataGridView2.Rows.Add(timeStamp, "Error occurred while trying to remove remnant " + searchValue + " from outgoing.");

            // Reset the filter
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = " ";
        }

        private void RemoveRemnant(object sender, EventArgs e)
        {
            TimeStamp = DateTime.Today.ToString();
            TimeStampLong = DateTime.Now.ToString();

            if (textBox1.Text.ToString() == "")
            {
                MessageBox.Show("No data entered.");
            }
            else
            {
                string SearchValue = textBox1.Text;
                SearchValue = SearchValue.Replace("]", "[]]");
                SearchValue = SearchValue.Replace("[", "[[]");
                string ItemCode = textBox4.Text;
                string Weight = textBox3.Text;
                string HeatNumber = textBox2.Text;

                (dataGridView1.DataSource as DataTable).DefaultView.RowFilter =
                string.Format("RemnantNumber like '" + SearchValue + "' AND ItemNumber like '" + ItemCode + "' AND Weight like '" + Weight + "' AND HeatNumber like '" + HeatNumber + "'");

                if (dataGridView1.Rows.Count == 1)
                {
                    MessageBox.Show("Cannot delete the last remaining row.");
                }
                else if (dataGridView1.Rows.Count.ToString() == "0")
                {
                    HandleRemnantNotFound(TimeStampLong, SearchValue);
                }
                else
                {
                    try
                    {
                        int index = 0;
                        dataGridView1.Rows.RemoveAt(index);
                        (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = " ";
                        dataGridView1.Refresh();
                        dataGridView2.Rows.Add(TimeStampLong, "Remnant " + SearchValue + " removed from outgoing.");
                    }
                    catch
                    {
                        HandleRemnantNotFound(TimeStampLong, SearchValue);
                        dataGridView1.Refresh();
                    }
                }
            }
        }
        private void AddRemnant(object sender, EventArgs e)
        {
            string RemnantName = textBox8.Text;
            string ItemCode = textBox7.Text;
            string Weight = textBox6.Text;
            string HeatNumber = textBox5.Text;
            TimeStamp = DateTime.Today.ToString();
            TimeStampLong = DateTime.Now.ToString();

            DataTable dataTable = (DataTable)dataGridView1.DataSource;
            DataRow drToAdd = dataTable.NewRow();

            drToAdd["RemnantNumber"] = RemnantName;
            drToAdd["ItemNumber"] = ItemCode;
            drToAdd["Weight"] = Weight;
            drToAdd["HeatNumber"] = HeatNumber;
            dataTable.Rows.Add(drToAdd);
            dataTable.AcceptChanges();

            dataGridView2.Rows.Add(TimeStampLong, "Remnant " + RemnantName + " added to outgoing with following info... Item Number: " + ItemCode + ", Weight: " + Weight + ", Heat Number: " + HeatNumber); ;

            textBox8.Clear();
            textBox7.Clear();
            textBox6.Clear();
            textBox5.Clear();
            int nRowIndex = dataGridView1.Rows.Count - 1;
            int nColumnIndex = 3;

            dataGridView1.Rows[nRowIndex].Selected = true;
            dataGridView1.Rows[nRowIndex].Cells[nColumnIndex].Selected = true;

            dataGridView1.FirstDisplayedScrollingRowIndex = nRowIndex;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // Removing remnant from xml table
            string SearchValue = textBox1.Text;
            try
            {
                ImportXMLData();
                RemoveRemnant(this, new EventArgs());
                SaveXMLData(this, new EventArgs());
                textBox1.Clear();
                textBox4.Clear();
                textBox3.Clear();
                textBox2.Clear();
                textBox1.Select();
            }
            catch
            {
                HandleRemnantNotFound(TimeStampLong, SearchValue);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            // Adding remnant to xml table
            ImportXMLData();
            AddRemnant(this, new EventArgs());
            SaveXMLData(this, new EventArgs());
            textBox8.Select();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            // Remove remnant from xml table 
            int selectedIndex = dataGridView1.CurrentCell.RowIndex;
            string SearchValue = dataGridView1.Rows[selectedIndex].Cells[0].Value.ToString();
            dataGridView1.Rows.RemoveAt(selectedIndex);
            dataGridView1.Refresh();
            SaveXMLData(this, new EventArgs());
            MessageBox.Show("Selected row (s) have been removed.");
            dataGridView2.Rows.Add(TimeStampLong, "Remnant " + SearchValue + " removed from outgoing.");
        }

        private void TextBox9_TextChanged(object sender, EventArgs e)
        {
            SearchBox(this, new EventArgs());
        }

        private void TextBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                textBox7.Select();
            }
        }

        private void TextBox7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                textBox6.Select();
            }
        }

        private void TextBox6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                textBox5.Select();
            }
        }

        private void TextBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                button2.Select();
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            button4.Visible = false;
            button5.Visible = true;
            dataGridView2.Visible = true;
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            button5.Visible = false;
            button4.Visible = true;
            dataGridView2.Visible = false;
        }

        private void SaveLog(object sender, EventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string timeStamp = DateTime.Now.ToString().Replace("/", "").Replace(":", " ");
            string filePath = Path.Combine(documentsPath, timeStamp + ".txt");

            using (StreamWriter file = new StreamWriter(filePath))
            {
                try
                {
                    string sLine = "";
                    for (int r = 0; r <= dataGridView2.Rows.Count - 1; r++)
                    {
                        for (int c = 0; c <= dataGridView2.Columns.Count - 1; c++)
                        {
                            object cellValue = dataGridView2.Rows[r].Cells[c].Value;
                            sLine += cellValue != null ? cellValue.ToString() : "";

                            if (c != dataGridView2.Columns.Count - 1)
                            {
                                sLine += " ";
                            }
                        }
                        file.WriteLine(sLine);
                        sLine = "";
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            SaveLog(this, new EventArgs());
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                textBox4.Select();
            }
        }

        private void TextBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                textBox3.Select();
            }
        }

        private void TextBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                textBox2.Select();
            }
        }

        private void TextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                button1.Select();
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            ImportXMLData();
            MessageBox.Show("Tables have been updated.");
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox cb = (ComboBox)sender;
            e.DrawBackground();

            Color textColor = cb.ForeColor;
            using (SolidBrush brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(cb.Items[e.Index].ToString(), e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }
    }
}
