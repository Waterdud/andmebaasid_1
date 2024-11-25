using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace andmebaasid
{
    public partial class Form1 : Form
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AppData\Tooded.mdf;Integrated Security=True");
        SqlCommand command;
        SqlDataAdapter adapter, adapter2;
        int Id = 0;

        public Form1()
        {
            InitializeComponent();
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                connect.Open();

                // Загрузка данных из таблицы товаров
                DataTable table = new DataTable();
                adapter = new SqlDataAdapter("SELECT * FROM Tootetable", connect);
                adapter.Fill(table);
                dataGridView1.DataSource = table;

                // Установка изображения по умолчанию
                pictureBox1.Image = Image.FromFile(Path.Combine(@"..\..\Images", "piim.jpg"));

                // Загрузка категорий в ComboBox
                adapter2 = new SqlDataAdapter("SELECT Kategooria_nimetus FROM Kategooria", connect);
                DataTable kak_tabel = new DataTable();
                adapter2.Fill(kak_tabel);
                comboBox1.Items.Clear();
                foreach (DataRow row in kak_tabel.Rows)
                {
                    comboBox1.Items.Add(row["Kategooria_nimetus"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
            finally
            {
                connect.Close();
            }
        }

        private void ClearData()
        {
            Id = 0;
            Toodetxt.Text = "";
            Kogustxt.Text = "";
            Hindtxt.Text = "";
            comboBox1.SelectedIndex = -1;
            pictureBox1.Image = Image.FromFile(Path.Combine(@"..\..\Images", "maslo.jpg"));
        }

        private void btn_Insert_Click(object sender, EventArgs e)
        {
            if (Toodetxt.Text != "" && Kogustxt.Text != "" && Hindtxt.Text != "" && comboBox1.SelectedItem != null)
            {
                try
                {
                    command = new SqlCommand("INSERT INTO Tootetable(Toodenimetus, Kogus, Hind, Pilt, Kategooria_Id) VALUES (@toode, @kogus, @hind, @pilt, @kat)", connect);
                    connect.Open();

                    command.Parameters.AddWithValue("@toode", Toodetxt.Text);
                    command.Parameters.AddWithValue("@kogus", int.Parse(Kogustxt.Text));
                    command.Parameters.AddWithValue("@hind", decimal.Parse(Hindtxt.Text.Replace(",", ".")));
                    string file_pilt = Toodetxt.Text + ".jpg";
                    command.Parameters.AddWithValue("@pilt", file_pilt);
                    command.Parameters.AddWithValue("@kat", comboBox1.SelectedIndex + 1);

                    command.ExecuteNonQuery();
                    connect.Close();

                    ClearData();
                    DisplayData();
                    MessageBox.Show("Товар успешно добавлен!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении товара: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля!");
            }
        }

        private void btn_Update_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Toodetxt.Text) || string.IsNullOrWhiteSpace(Kogustxt.Text) || string.IsNullOrWhiteSpace(Hindtxt.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены!");
                return;
            }

            if (Id == 0)
            {
                MessageBox.Show("Выберите запись для обновления!");
                return;
            }

            try
            {
                // Создание команды
                command = new SqlCommand("UPDATE Tootetable SET Toodenimetus=@toode, Kogus=@kogus, Hind=@hind, Pilt=@pilt WHERE ID=@id", connect);

                // Открытие соединения
                connect.Open();

                // Добавление параметров
                command.Parameters.AddWithValue("@id", Id);
                command.Parameters.AddWithValue("@toode", Toodetxt.Text);
                command.Parameters.AddWithValue("@kogus", int.Parse(Kogustxt.Text));
                command.Parameters.AddWithValue("@hind", decimal.Parse(Hindtxt.Text.Replace(",", ".")));

                // Проверка наличия изображения
                string filePath = Toodetxt.Text + ".jpg";
                if (File.Exists(Path.Combine(@"..\..\Images", filePath)))
                {
                    command.Parameters.AddWithValue("@pilt", filePath);
                }
                else
                {
                    MessageBox.Show("Изображение не найдено! Убедитесь, что оно существует.");
                    command.Parameters.AddWithValue("@pilt", DBNull.Value);
                }

                // Выполнение команды
                command.ExecuteNonQuery();

                MessageBox.Show("Товар успешно обновлен!");
            }
            catch (FormatException)
            {
                MessageBox.Show("Некорректный формат числовых данных в полях 'Количество' или 'Цена'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            finally
            {
                // Закрытие соединения
                if (connect.State == ConnectionState.Open)
                {
                    connect.Close();
                }

                // Очистка данных и обновление таблицы
                ClearData();
                DisplayData();
            }
        }


        private void btn_Delete_Click(object sender, EventArgs e)
        {
            if (Id != 0)
            {
                try
                {
                    command = new SqlCommand("DELETE FROM Tootetable WHERE ID=@id", connect);
                    connect.Open();

                    command.Parameters.AddWithValue("@id", Id);
                    command.ExecuteNonQuery();
                    connect.Close();

                    ClearData();
                    DisplayData();
                    MessageBox.Show("Товар успешно удален!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления!");
            }
        }

        private void btn_Lisapilt_Click(object sender, EventArgs e)
        {
            if (Id != 0)
            {
                OpenFileDialog open = new OpenFileDialog
                {
                    Filter = "Image Files (*.jpeg;*.bmp;*.png;*.jpg)|*.jpeg;*.bmp;*.png;*.jpg",
                    InitialDirectory = Path.GetFullPath(@"..\..\Images")
                };

                if (open.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string destinationPath = Path.Combine(Path.GetFullPath(@"..\..\Images"), Toodetxt.Text + ".jpg");
                        File.Copy(open.FileName, destinationPath, true);

                        pictureBox1.Image = Image.FromFile(destinationPath);
                        MessageBox.Show("Изображение успешно добавлено!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при копировании файла: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для добавления изображения.");
            }
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                Id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                Toodetxt.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                Kogustxt.Text = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                Hindtxt.Text = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();

                string imagePath = Path.Combine(@"..\..\Images", dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString());
                if (File.Exists(imagePath))
                {
                    pictureBox1.Image = Image.FromFile(imagePath);
                }
                else
                {
                    pictureBox1.Image = null;
                    MessageBox.Show("Изображение не найдено!");
                }

                string v = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                comboBox1.SelectedIndex = int.Parse(v) - 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке строки: " + ex.Message);
            }
        }
    }
}
