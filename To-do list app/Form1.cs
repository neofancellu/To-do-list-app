using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace To_do_list_app
{
    public partial class ToDoList : Form
    {
        public ToDoList()
        {
            InitializeComponent();
        }

        string connectionString = @"Data Source =D:\Programming\databases\to-doList.db;Version = 3;";

        //testing if project talks to database
        public void TestDatabaseConnection()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Successfully connected to SQLite database!");
                    using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='ToDoItems'", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                Console.WriteLine("ToDoItems table exists!");

                                string query = "SELECT * FROM ToDoItems";
                                using (var adapter = new SQLiteDataAdapter(query, connection))
                                {
                                    DataTable dataTable = new DataTable();
                                    adapter.Fill(dataTable);

                                    // Print data from each row
                                    Console.WriteLine("\nTo-Do Items:");
                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        Console.WriteLine("ID: {0}, Title: {1}, Description: {2}",
                                            row["id"], row["Title"], row["Description"]);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("ToDoItems table does not exist!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to database: " + ex.Message);
            }
        }



        DataTable toDoList = new DataTable();
        bool isEditing = false;

        /*
        private void ToDoList_Load(object sender, EventArgs e)
        {
            

            //Colums are made
            toDoList.Columns.Add("Title");
            toDoList.Columns.Add("Description");

            //Link datasource to datagridview
            toDoListView.DataSource = toDoList;
        }
        */

        private void ToDoList_Load(object sender, EventArgs e)
        {
            try
            {
                // Connect to database
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Get table schema (column names)
                    string query = "SELECT id, Title, Description FROM ToDoItems";
                    using (var adapter = new SQLiteDataAdapter(query, connection))
                    {
                        // Fill dataTable with schema
                        toDoList = new DataTable();
                        toDoList.Clear();
                        

                        // Check if there are actually rows retrieved (handle empty table)
                        if (toDoList.Rows.Count > 0)
                        {
                            // Remove the dummy row only if data was retrieved
                            toDoList.Rows.RemoveAt(0);
                        }
                    }

                    // Now populate data
                    query = "SELECT * FROM ToDoItems"; // Fetch all rows this time
                    using (var adapter = new SQLiteDataAdapter(query, connection))
                    {
                        adapter.Fill(toDoList);
                    }

                    // Link data table to the grid view
                    
                    toDoListView.DataSource = toDoList;
                    toDoListView.Columns["id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading data: " + ex.Message);
            }
        }


        private void newButton_Click(object sender, EventArgs e)
        {
            titleTextBox.Text = "";
            descTextBox.Text = "";

            TestDatabaseConnection();
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            isEditing = true;
            //Fill text fields with data from the Table
            titleTextBox.Text = toDoList.Rows[toDoListView.CurrentCell.RowIndex].ItemArray[1].ToString();
            descTextBox.Text = toDoList.Rows[toDoListView.CurrentCell.RowIndex].ItemArray[2].ToString();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the data table row and ID of the selected item
                DataRow selectedRow = toDoList.Rows[toDoListView.CurrentCell.RowIndex];
                long id = (long)selectedRow["id"];

                // Check if a row is actually selected
                if (selectedRow != null)
                {
                    // Delete the entry from the database
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();

                        string deleteQuery = "DELETE FROM ToDoItems WHERE id=@id";
                        using (var command = new SQLiteCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            command.ExecuteNonQuery();
                        }
                    }

                    // Remove the row from the data table
                    selectedRow.Delete();
                    toDoList.AcceptChanges();

                    // Update data grid view
                    toDoListView.Refresh();
                }
                else
                {
                    MessageBox.Show("Please select a row to delete.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting data: " + ex.Message);
                // Add user-friendly error notification in your application
            }
        }


        /*
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                toDoList.Rows[toDoListView.CurrentCell.RowIndex]["Title"] = titleTextBox.Text;
                toDoList.Rows[toDoListView.CurrentCell.RowIndex]["Description"] = descTextBox.Text;
            }
            else
            {
                toDoList.Rows.Add(titleTextBox.Text, descTextBox.Text);
            }

            //Clear temp field after change
            titleTextBox.Text = "";
            descTextBox.Text = "";
            isEditing = false;
        }
          */
        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    if (isEditing)
                    {
                        Console.WriteLine(toDoList.Rows[toDoListView.CurrentCell.RowIndex]["id"].GetType());
                        // Update existing entry
                        long id = (long)toDoList.Rows[toDoListView.CurrentCell.RowIndex]["id"];
                        

                        string updateQuery = "UPDATE ToDoItems SET Title=@title, Description=@desc WHERE id=@id";
                        using (var command = new SQLiteCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            command.Parameters.AddWithValue("@title", titleTextBox.Text);
                            command.Parameters.AddWithValue("@desc", descTextBox.Text);
                            command.ExecuteNonQuery();
                        }

                        // Update data table row
                        toDoList.Rows[toDoListView.CurrentCell.RowIndex]["Title"] = titleTextBox.Text;
                        toDoList.Rows[toDoListView.CurrentCell.RowIndex]["Description"] = descTextBox.Text;

                        toDoList.AcceptChanges();
                    }
                    else
                    {
                        // New entry
                        string insertQuery = "INSERT INTO ToDoItems (Title, Description) VALUES (@title, @desc)";
                        using (var command = new SQLiteCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@title", titleTextBox.Text);
                            command.Parameters.AddWithValue("@desc", descTextBox.Text);
                            command.ExecuteNonQuery();

                            // Retrieve the new ID:

                            // SELECT last_insert_rowid() 
                            string identityQuery = "SELECT last_insert_rowid()";

                            using (var command2 = new SQLiteCommand(identityQuery, connection))
                            {
                                long newId = (long)command2.ExecuteScalar();
                                if (!isEditing)
                                {
                                    // New entry
                                    toDoList.Rows.Add(newId, titleTextBox.Text, descTextBox.Text);
                                    toDoList.AcceptChanges();
                                }

                            }



                        }
                    }

                    // Clear temp fields after change
                    titleTextBox.Text = "";
                    descTextBox.Text = "";
                    isEditing = false;

                    ToDoList_Load(sender, e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving data: " + ex.Message);
                // Add user-friendly error notification in your application
            }
        }

    }

}
