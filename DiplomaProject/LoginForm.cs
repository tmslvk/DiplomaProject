using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiplomaProject.Model;

namespace DiplomaProject
{

    public partial class LoginForm : Form
    {
        DiplomaBDContext db = new DiplomaBDContext();
        public bool showHideChecker = false;
        public LoginForm()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            GreetWindow form = new GreetWindow();
            form.Show();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            errorLabel.Visible = false;
            if (CheckPassword(passwordTextBox.Text) && CheckLogin(loginTextBox.Text))
            {
                MainForm form = new MainForm(FindIdByLogin(), db);
                this.Hide();
                form.Show();
            }
            else
            {
                if (!CheckPassword(passwordTextBox.Text))
                {
                    errorLabel.Visible = true;
                    passwordTextBox.BorderColor = Color.Red;
                }
                else if (!CheckLogin(loginTextBox.Text))
                {
                    errorLabel.Visible = true;
                    loginTextBox.BorderColor = Color.Red;
                }
            }
        }

        public int FindIdByLogin()
        {
            return db.Users.First<User>(u => u.Login == loginTextBox.Text).Id;
        }

        public bool CheckLogin(string login)
        {
            var user = db.Users.FirstOrDefault(u => u.Login == login);
            return user != null;
        }
        public bool CheckPassword(string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Password == password);
            return user != null;
        }

        private void showHidePasswordButton_Click(object sender, EventArgs e)
        {
            if (!showHideChecker)
            {
                showHidePasswordButton.Image = Properties.Resources.showPic as Image;
                passwordTextBox.PasswordChar = '\0';
                passwordTextBox.UseSystemPasswordChar = false;
                showHideChecker = true;
                return;
            }
            if (showHideChecker)
            {
                showHidePasswordButton.Image = Properties.Resources.hidePic as Image;
                passwordTextBox.PasswordChar = '●';
                passwordTextBox.UseSystemPasswordChar = true;
                showHideChecker = false;
                return;
            }
        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
