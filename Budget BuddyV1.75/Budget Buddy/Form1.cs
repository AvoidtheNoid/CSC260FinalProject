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
        string version = "1.5";

        public Form1()
        {
            //Loading code
            IFormatter formatter = new BinaryFormatter();
            Stream stream;
            try
            {
                stream = new FileStream("Data4.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
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
        public int getSelectedCapital()
        {
            if (listView2.SelectedIndices.Count > 0)
            {
                return listView2.SelectedIndices[0];
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
                        if (listView1.SelectedIndices[0] > 0)
                        {
                            Data.getSelectedBudget().balance += Convert.ToDecimal(string.Format("{0:0.00}", numericUpDown1.Value));
                            Data.getActiveBList().ElementAt(0).balance += Convert.ToDecimal(string.Format("{0:0.00}", numericUpDown1.Value));
                        }
                        else
                        {
                            Data.getSelectedBudget().balance += Convert.ToDecimal(string.Format("{0:0.00}", numericUpDown1.Value));
                        }

                        //Update the display
                        int select = listView1.SelectedIndices[0];
                        Update_Budgetview();
                        listView1.SelectedIndices.Add(select);
                    }
                }
                else
                {
                    MessageBox.Show("Please select an expense to deposit to");
                }
            }
            else if(rBPlan.Checked)
            {
                //Deposit through plan

                //Check if plan is selected
                if(comboBox1.SelectedIndex >= 0)
                {
                    //get reference to selected plan
                    BudgetPlan plan = Data.getActiveAccount().planList.ElementAt(comboBox1.SelectedIndex);


                    decimal toDefault = numericUpDown1.Value;
                    for (int x = 0; x < plan.list.Count; x++)
                    {
                        Budgeter budgeter = plan.list.ElementAt(x);
                        // Add money if it's dollars
                        if (budgeter.type == 0)
                        {
                            budgeter.budget.balance += budgeter.value;
                            toDefault -= budgeter.value;
                        }
                        //Add money if it's percent
                        if (budgeter.type == 1)
                        {
                            budgeter.budget.balance += numericUpDown1.Value * (budgeter.value / 100);
                            toDefault -= numericUpDown1.Value * (budgeter.value / 100);
                        }
                    }
                    //Data.getDefaultBudget().balance += toDefault;
                    Data.totalBudget();
                    //Update the display
                    int select = -1;
                    if(listView1.SelectedIndices.Count > 0)
                    {
                        select = listView1.SelectedIndices[0];
                    }
                    Update_Budgetview();
                    if(select > -1)
                        listView1.SelectedIndices.Add(select);
                }
                else
                {
                    MessageBox.Show("Please select a income plan to deposit to.");
                }

            }
            else if(rBCapital.Checked)
            {
                if (listView2.SelectedIndices.Count > 0)
                {
                    if (listView2.SelectedIndices[0] >= 0)
                    {

                        if (listView2.SelectedIndices[0] > 0)
                        {
                            Data.getSelectedCapital().balance += Convert.ToDecimal(string.Format("{0:0.00}", numericUpDown1.Value));
                            Data.getActiveCList().ElementAt(0).balance += Convert.ToDecimal(string.Format("{0:0.00}", numericUpDown1.Value));
                        }
                        else
                        {
                            Data.getSelectedCapital().balance += Convert.ToDecimal(string.Format("{0:0.00}", numericUpDown1.Value));
                        }
                        //Update the display
                        int select = listView2.SelectedIndices[0];
                        Update_Capitalview();
                        listView2.SelectedIndices.Add(select);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a capital source to deposit to");
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
            Save.Saveall();
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            //Check if account is selected
            if(Data.activeAccount > -1)
            {
                New_BPLan myForm = new New_BPLan();
                myForm.ShowDialog();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(Data.getActiveAccount().planList.ToArray());
            }
            Save.Saveall();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if(comboBox1.SelectedIndex >= 0)
            {
                if (MessageBox.Show("Are you sure you want to delete this budget plan?", "Delete Plan", MessageBoxButtons.YesNo)
                   == DialogResult.Yes)
                {
                    Data.getActiveAccount().planList.RemoveAt(comboBox1.SelectedIndex);
                    UpdateAll();
                }
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

        private void Update_Capitalview()
        {
            listView2.Items.Clear();
            if (Data.activeAccount >= 0)
            {

                //Cycle through budgets in active account
                for (int x = 0; x < Data.getActiveCList().Count; x++)
                {
                    string money = '$' + string.Format("{0:0.00}", Data.getActiveCList().ElementAt(x).balance);
                    ListViewItem item = new ListViewItem(new[] { Data.getActiveCList().ElementAt(x).name, money });
                    listView2.Items.Add(item);
                }
            }

        }

        private void UpdateAll()
        {
            Update_Budgetview();
            Update_BudgetPlans();
            Update_Capitalview();
        }

        private void Update_BudgetPlans()
        {
            comboBox1.Items.Clear();
            if(Data.activeAccount > -1)
            {
                if (comboBox2.SelectedIndex >= 0)
                {
                    comboBox1.Items.AddRange(Data.getActiveAccount().planList.ToArray());
                }
            }
            
        }

        private void rBBudget_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonBplanEdit_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex >= 0)
            {
                New_BPLan form = new New_BPLan(Data.getActiveAccount().planList.ElementAt(comboBox1.SelectedIndex));
                form.ShowDialog();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(Data.getActiveAccount().planList.ToArray());
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

        private void button7_Click(object sender, EventArgs e)
        {
            if (withdrawCapital.Checked)
            {
                //Continue if budget is selected
                if (listView2.SelectedIndices.Count > 0)
                {
                    if (listView2.SelectedIndices[0] >= 0)
                    {
                        //Check if budget has requested balance
                        if (Data.getSelectedCapital().balance >= numericUpDown2.Value)
                        {
                            //subtract balance
                            if(Data.getSelectedCapital() != Data.getDefaultCapital())
                            {
                                Data.getActiveCList().ElementAt(0).balance -= numericUpDown2.Value;
                                Data.getSelectedCapital().balance -= numericUpDown2.Value;
                            }
                            else
                            {
                                Data.getSelectedCapital().balance -= numericUpDown2.Value;
                            }

                        }
                        else
                        {
                            //Ask user if they are OK with negative number
                            if (MessageBox.Show("Budget balance will be overdrawn. Is this OK?", "Budget Overdrawn", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Data.getSelectedCapital().balance -= numericUpDown2.Value;
                            }
                        }

                        //Update the display
                        int select = listView2.SelectedIndices[0];
                        Update_Capitalview();
                        listView2.SelectedIndices.Add(select);
                    }
                }
            }
            else if (withdrawExpense.Checked)
            {
                if (listView1.SelectedIndices.Count > 0)
                {
                    if (listView1.SelectedIndices[0] >= 0)
                    {
                        //Check if budget has requested balance
                        if (Data.getSelectedBudget().balance >= numericUpDown2.Value)
                        {
                            //subtract balance
                            if (Data.getSelectedBudget() != Data.getDefaultBudget())
                            {
                                Data.getActiveBList().ElementAt(0).balance -= numericUpDown2.Value;
                                Data.getSelectedBudget().balance -= numericUpDown2.Value;
                            }
                            else
                            {
                                Data.getSelectedBudget().balance -= numericUpDown2.Value;
                            }
                        }
                        else
                        {
                            //Ask user if they are OK with negative number
                            if (MessageBox.Show("Budget balance will be overdrawn. Is this OK?", "Budget Overdrawn", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            }
            Save.Saveall();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Check if there's an active account

            if (MessageBox.Show("Add Expense?", "Add Expense", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                NewBudget form = new NewBudget();
                form.ShowDialog();
                Update_Budgetview();
            }
            else if (MessageBox.Show("Add Capital?", "Add Capital", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                New_Capital form = new New_Capital();
                form.ShowDialog();
                Update_Capitalview();
            }
            //display error if there's no active account
            else
            {
                MessageBox.Show("Please select an account");
            }
            Save.Saveall();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //Edit Budget

            //Check if there is a selected index
            if (listView1.SelectedIndices.Count > 0)
            {
                if (listView1.SelectedIndices[0] >= 0)
                {
                    NewBudget form = new NewBudget(Data.getSelectedBudget());
                    form.ShowDialog();
                    Update_Budgetview();
                }
            }
            else if(listView2.SelectedIndices.Count > 0)
            {
                if (listView2.SelectedIndices[0] >= 0)
                {
                    New_Capital form = new New_Capital(Data.getSelectedCapital());
                    form.ShowDialog();
                    Update_Capitalview();
                }
            }
            else
            {
                MessageBox.Show("Please select an item to edit");
            }
            Save.Saveall();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                if (listView1.SelectedIndices[0] >= 0)
                {
                    if (MessageBox.Show("Are you sure you want to delete this expense?", "Delete Expense", MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
                    {
                        if (Data.getActiveBList().ElementAt(listView1.SelectedIndices[0]) != Data.getDefaultBudget())
                        {
                            if (MessageBox.Show(
                                "Are you sure you want to remove this expense?", "Remove Expense", MessageBoxButtons.YesNo)
                                == DialogResult.Yes)
                            {
                                Data.getActiveBList().RemoveAt(listView1.SelectedIndices[0]);
                                Data.totalBudget();
                                Update_Budgetview();
                            }
                            else return;
                        }
                        else
                        {
                            MessageBox.Show("You may not remove the default expense. However, you may rename it if you like.");
                        }
                    }


                }
            }
            else if (listView2.SelectedIndices.Count > 0)
            {
                if (listView2.SelectedIndices[0] >= 0)
                {
                    if (MessageBox.Show("Are you sure you want to delete this source?", "Delete Source", MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
                    {
                        if (Data.getActiveCList().ElementAt(listView2.SelectedIndices[0]) != Data.getDefaultCapital())
                        {
                            if (MessageBox.Show(
                                "Are you sure you want to remove this source?", "Remove Source", MessageBoxButtons.YesNo)
                                == DialogResult.Yes)
                            {
                                Data.totalCapital(Data.getActiveCList().ElementAt(listView2.SelectedIndices[0]));
                                Data.getActiveCList().RemoveAt(listView2.SelectedIndices[0]);
                                Update_Capitalview();
                            }
                            else return;
                        }
                        else
                        {
                            MessageBox.Show("You may not remove the total.");
                        }
                    }


                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove");
            }
            Save.Saveall();
        }
    }


    [Serializable]
    public class Account
    {
        public string name;
        public List<BudgetPlan> planList;
        public List<Budget> budgetList;
        public Budget defaultBudget;
        public Capital defaultCapital;
        public List<Capital> capitalList;
        public Budget budgetTotal;
        public Account(string name)
        {
            this.name = name;

            planList = new List<BudgetPlan>();
            budgetList = new List<Budget>();
            budgetTotal = new Budget("Total Expenses");
            budgetList.Add(budgetTotal);
            defaultBudget = new Budget("Default");
            budgetList.Add(defaultBudget);
            capitalList = new List<Capital>();
            defaultCapital = new Capital("Total");
            capitalList.Add(defaultCapital);

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
            return this.name;
        }

        public string name;
        public List<Budgeter> list;
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

    [Serializable]
    public class Capital
    {

        public Capital(string name)
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

    public static class Save
    {
        public static void Saveall()
        {
            DataObject tempObject = new DataObject();
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("Data4.bin", FileMode.Create, FileAccess.Write, FileShare.Write);
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
        public static decimal total_Budget;
        public static decimal total_Capital;

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

        public static Budget totalBudget()
        {
            total_Budget = 0;
            for(int i = 1; i < getActiveBList().Count; i++)
            {
                total_Budget += getActiveBList().ElementAt(i).balance;
            }
            accounts.ElementAt(activeAccount).budgetList.ElementAt(0).balance = total_Budget;
            return null;
        }

        public static List<Capital> getActiveCList()
        {
            if (activeAccount >= 0)
            {
                return accounts.ElementAt(activeAccount).capitalList;
            }
            else
                return null;

        }

        public static Capital getSelectedCapital()
        {
            if (mainForm.getSelectedCapital() >= 0)
            {
                return getActiveCList().ElementAt(mainForm.getSelectedCapital());
            }
            else return null;
        }

        public static Capital getDefaultCapital()
        {
            return getActiveAccount().defaultCapital;
        }

        public static Capital totalCapital(Capital capital)
        {
            decimal tmp = 0;

            for (int i = 1; i < getActiveCList().Count; i++)
            {
                tmp += getActiveCList().ElementAt(i).balance;
            }
            tmp = getActiveCList().ElementAt(0).balance - tmp;

            for (int i = 1; i < getActiveCList().Count; i++)
            {
                if(getActiveCList().ElementAt(i) != capital)
                {
                    tmp += getActiveCList().ElementAt(i).balance;
                }
            }
            total_Capital = tmp;
            accounts.ElementAt(activeAccount).capitalList.ElementAt(0).balance = total_Capital;

            return null;
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


    