using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiplomaProject.Model;

namespace DiplomaProject
{
    public partial class RegistrationForm : Form
    {
        public int progress = 0;
        public bool commonInfoComplete = false;
        public Regex validator = new Regex("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&].{8,15}$");
        DiplomaBDContext db = new DiplomaBDContext();
        public bool showHideChecker = false;
        public RegistrationForm()
        {
            InitializeComponent();
            DefaultHide();
        }

        #region[UpperBar]
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
            if (commonInfoComplete)
            {
                lastnameTextBox.Show();
                firstnameTextBox.Show();
                middlenameTextBox.Show();
                universityTextBox.Show();
                jobPostTextBox.Show();
                departmentTextBox.Show();
                completeCommonInformationButton.Show();

                completeRegistrationButton.Hide();
                passwordTextBox.Hide();
                loginTextBox.Hide();
                showHidePasswordButton.Hide();
                headerLabel.Text = "Общая информация";
            }
            else
            {
                this.Hide();
                GreetWindow form = new GreetWindow();
                form.Show();
            }
        }
        #endregion
        #region[Buttons]
        private void showHideButton_Click(object sender, EventArgs e)
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

        private void completeRegistrationButton_Click(object sender, EventArgs e)
        {
            errorPasswordLabel.Visible = false;
            loginErrorLabel.Visible = false;
            if (!validator.IsMatch(passwordTextBox.Text))
            {
                passwordTextBox.BorderColor = Color.Red;
                errorPasswordLabel.Visible = true;
            }
            else
            {
                using (db)
                {
                    User newUser = new User()
                    {
                        Fullname = $"{lastnameTextBox.Text} {firstnameTextBox.Text} {middlenameTextBox.Text}",
                        JobPost = jobPostTextBox.Text,
                        Departmant = departmentTextBox.Text,
                        University = universityTextBox.Text,
                        Login = loginTextBox.Text,
                        Password = passwordTextBox.Text,
                    };
                    if (!CheckLogin(newUser.Login))
                    {
                        db.Users.Add(newUser);
                        db.SaveChanges();
                        gratsLabel.Visible = true;

                        MainForm form = new MainForm(FindIdByLogin(), db);
                        this.Hide();
                        form.Show();
                    }
                    else
                    {
                        loginErrorLabel.Visible = true;
                        loginTextBox.BorderColor = Color.Red;
                    }
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

        private void completeCommonInformationButton_Click(object sender, EventArgs e)
        {
            commonInfoComplete = true;

            ShowNHide();
            headerLabel.Text = "Входные данные";
        }
        #endregion
        #region[HideAndShow]
        public void ShowNHide()
        {
            lastnameTextBox.Hide();
            firstnameTextBox.Hide();
            middlenameTextBox.Hide();
            universityTextBox.Hide();
            jobPostTextBox.Hide();
            departmentTextBox.Hide();
            completeCommonInformationButton.Hide();

            completeRegistrationButton.Show();
            passwordTextBox.Show();
            loginTextBox.Show();
            showHidePasswordButton.Show();
        }

        public void DefaultHide()
        {
            passwordTextBox.Hide();
            loginTextBox.Hide();
            completeRegistrationButton.Hide();
            showHidePasswordButton.Hide();
        }
        #endregion
        #region[Validators]
        private void lastnameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (lastnameTextBox.Text != string.Empty && lastnameTextBox.Text.Length < 17 && lastnameTextBox.Text.Length > 1) { progressBar.Value += 10; }
        }

        private void firstnameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (firstnameTextBox.Text != string.Empty && firstnameTextBox.Text.Length < 17 && firstnameTextBox.Text.Length > 1) { progressBar.Value += 10; }
        }

        private void middlenameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (middlenameTextBox.Text != string.Empty && middlenameTextBox.Text.Length < 17 && middlenameTextBox.Text.Length > 1) { progressBar.Value += 10; }
        }

        private void jobPostTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (jobPostTextBox.Text != string.Empty && jobPostTextBox.Text.Length < 17 && jobPostTextBox.Text.Length > 1) { progressBar.Value += 10; }
        }

        private void departmentTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (departmentTextBox.Text != string.Empty && departmentTextBox.Text.Length < 17 && departmentTextBox.Text.Length > 1) { progressBar.Value += 10; }
        }

        private void universityTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (universityTextBox.Text != string.Empty && universityTextBox.Text.Length < 17 && universityTextBox.Text.Length > 1) { progressBar.Value += 10; }
        }

        private void loginTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (loginTextBox.Text != string.Empty && loginTextBox.Text.Length < 17 && lastnameTextBox.Text.Length > 4) { progressBar.Value += 15; }
        }

        private void passwordTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (validator.IsMatch(passwordTextBox.Text)) { progressBar.Value += 15; }
        }
        #endregion
    }
}
