using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace Budget_Buddy
{
    public partial class Form1 : Form
    {
        string version = "1.2";

        public Form1()
        {
            //Loading code
            IFormatter formatter = new BinaryFormatter();
            Stream stream;
            try
            {
                stream = new FileStream("Data.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                DataObject tempObject = (DataObject)formatter.Deserialize(stream);
                Data.accounts = tempObject.accounts;
                Data.activeAccount = tempObject.activeAccount;
                stream.Close();
            }
            catch
            {
                Data.activeAccount = -1;
                Data.accounts = new List<Account>();
            }
                Data.mainForm = this;

            InitializeComponent();
            comboBox2.Items.AddRange(Data.accounts.ToArray());
            if (Data.activeAccount > -1)
                comboBox2.SelectedIndex = Data.activeAccount;
            UpdateAll();
        }

        public int getSelectedBudget()
        {
            if(listView1.SelectedIndices.Count > 0)
            {
                return listView1.SelectedIndices[0];
            }
            else
            {
                return -1;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(rBBudget.Checked)
            {
                //Deposit to Budget

                //Check if there is a selected index
                if (listView1.SelectedIndices.Count > 0)
                {
                    if (listView1.SelectedIndices[0] >= 0)
                    {
                        Data.getSelectedBudget().balance += Convert.ToDecimal(string.Format("{0:0.00}", numericUpDown1.Value));

                        //Update the display
                        int select = listView1.SelectedIndices[0];
                        Update_Budgetview();
                        listView1.SelectedIndices.Add(select);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a budget to deposit to");
                }
            }
            Save.Saveall();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Data.activeAccount = comboBox2.SelectedIndex;
            UpdateAll();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NewAccount myForm = new NewAccount();
            myForm.ShowDialog();
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(Data.accounts.ToArray());
            if(!Data.isCancelled)
            {
                //comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
                Data.isCancelled = true;
            }
            //Save.Saveall();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex >= 0)
            {
                //Ask user if sure
                if(MessageBox.Show(
                    "Are you sure you want to delete this account? All data stored in this account will be lost!", "Delete Account", MessageBoxButtons.YesNo)
                    == DialogResult.Yes
                )
                {
                    //Check if active account is being deleted
                    if(comboBox2.SelectedIndex == Data.activeAccount)
                    {
                        //Set active account to nothing
                        Data.activeAccount = -1;
                    }
                }


                Data.accounts.RemoveAt(comboBox2.SelectedIndex);
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(Data.accounts.ToArray());
                UpdateAll();
            }
            Save.Saveall();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Check if there's an active account
            if(Data.activeAccount != -1)
            {
                NewBudget form = new NewBudget();
                form.ShowDialog();
                Update_Budgetview();
            }
            //display error if there's no active account
            else
            {
                MessageBox.Show("Please select an account");
            }
            Save.Saveall();
        }

        private void Update_Budgetview()
        {
            listView1.Items.Clear();
            if(Data.activeAccount >= 0)
            {

                //Cycle through budgets in active account
                for(int x = 0; x < Data.getActiveBList().Count; x++)
                {
                    string money = '$' + string.Format("{0:0.00}", Data.getActiveBList().ElementAt(x).balance);
                    ListViewItem item = new ListViewItem(new [] {Data.getActiveBList().ElementAt(x).name, money});
                    listView1.Items.Add(item);
                }
            }

        }

        private void UpdateAll()
        {
            Update_Budgetview();
        }

        //Remove Selected Budget
        private void button6_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedIndices.Count > 0)
            {
                if(listView1.SelectedIndices[0] >= 0)
                {
                    if(MessageBox.Show("Are you sure you want to delete this budget?", "Delete Budget", MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
                    {
                        if (Data.getActiveBList().ElementAt(listView1.SelectedIndices[0]) != Data.getDefaultBudget())
                        {
                            if (MessageBox.Show(
                                "Are you sure you want to remove this budget?", "Remove Budget", MessageBoxButtons.YesNo)
                                == DialogResult.Yes)
                            {
                                Data.getActiveBList().RemoveAt(listView1.SelectedIndices[0]);
                                Update_Budgetview();
                            }
                            else return;
                        }
                        else
                        {
                            MessageBox.Show("You may not remove the default budget. However, you may rename it if you like.");
                        }
                    }


                }
            }
            else
            {
                MessageBox.Show("Please select a budget to remove");
            }
            Save.Saveall();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //Edit Budget

            //Check if there is a selected index
            if(listView1.SelectedIndices.Count > 0)
            {
                if(listView1.SelectedIndices[0] >= 0)
                {
                    NewBudget form = new NewBudget(Data.getSelectedBudget());
                    form.ShowDialog();
                    Update_Budgetview();
                }
            }
            else
            {
                MessageBox.Show("Please select a budget to edit");
            }
            Save.Saveall();
        }

        private void rBBudget_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Continue if budget is selected
            if(listView1.SelectedIndices.Count > 0)
            {
                if(listView1.SelectedIndices[0] >= 0)
                {
                    //Check if budget has requested balance
                    if(Data.getSelectedBudget().balance >= numericUpDown2.Value)
                    {
                        //subtract balance
                        Data.getSelectedBudget().balance -= numericUpDown2.Value;
                    }
                    else
                    {
                        //Ask user if they are OK with negative number
                        if(MessageBox.Show("Budget balance will be overdrawn. Is this OK?", "Budget Overdrawn", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Data.getSelectedBudget().balance -= numericUpDown2.Value;
                        }
                    }

                    //Update the display
                    int select = listView1.SelectedIndices[0];
                    Update_Budgetview();
                    listView1.SelectedIndices.Add(select);
                }
                

            }
            Save.Saveall();
        }


        private void buttonEditAccount_Click(object sender, EventArgs e)
        {
            if(comboBox2.SelectedIndex >= 0)
            {
                NewAccount form = new NewAccount(Data.getActiveAccount());
                form.ShowDialog();
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(Data.accounts.ToArray());
            }
            Save.Saveall();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

    }


    [Serializable]
    public class Account
    {
        public string name;
        public List<BudgetPlan> planList;
        public List<Budget> budgetList;
        public Budget defaultBudget;

        public Account(string name)
        {
            this.name = name;

            planList = new List<BudgetPlan>();
            budgetList = new List<Budget>();
            defaultBudget = new Budget("Default");
            budgetList.Add(defaultBudget);
            
        }

        override public string ToString()
        {
            return name;
        }
    }

    [Serializable]
    public class BudgetPlan
    {
        public BudgetPlan()
        {
            list = new List<Budgeter>();
        }

        public override string ToString()
        {
            return this.name + " - $" + this.minimumDeposit;
        }

        public string name;
        public List<Budgeter> list;
        public decimal minimumDeposit = 0;

    }

    [Serializable]
    public class Budget
    {

        public Budget(string name)
        {
            this.name = name;
            balance = 0;
        }

        public override string ToString()
        {
            return this.name;
        }

        public string name;
        public decimal balance;
    }

    [Serializable]
    public class Budgeter
    {
        //the budget this this points to
        public Budget budget;
        //The type of budgeter this is (% or $)
        public int type;
        //How much to send to the budget
        public decimal value;

        public override string ToString()
        {
            return budget.name;
        }
    }

    
    public static class Save
    {
        public static void Saveall()
        {
            DataObject tempObject = new DataObject();
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("Data2.bin", FileMode.Create, FileAccess.Write, FileShare.Write);
            formatter.Serialize(stream, tempObject);
            stream.Close();
        }
    }

    [Serializable]
    public static class Data
    {
        public static List<Account> accounts;
        public static int activeAccount;
        public static Form1 mainForm;
        public static bool isCancelled = true;


        public static Account getActiveAccount()
        {
            return accounts.ElementAt(activeAccount);
        }

        public static List<Budget> getActiveBList()
        {
            if (activeAccount >= 0)
            {
                return accounts.ElementAt(activeAccount).budgetList;
            }
            else
                return null;
            
        }

        public static Budget getSelectedBudget()
        {
            if (mainForm.getSelectedBudget() >= 0)
            {
                return getActiveBList().ElementAt(mainForm.getSelectedBudget());
            }
            else return null;
        }

        public static Budget getDefaultBudget()
        {
            return getActiveAccount().defaultBudget;
        }
    }

    [Serializable]
    public class DataObject
    {
        public List<Account> accounts;
        public int activeAccount;

        public DataObject()
        {
            this.accounts = Data.accounts;
            this.activeAccount = Data.activeAccount;
        }
    }
}


    