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
using DocumentFormat.OpenXml.Wordprocessing;

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
                passwordTextBox.BorderColor = System.Drawing.Color.Red;
                errorPasswordLabel.Visible = true;
            }
            else
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
                    loginTextBox.BorderColor = System.Drawing.Color.Red;
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

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        #region[Validators]
        public bool IsValid(string s)
        {
            return s != null && s.Length < 17 && s.Length > 2 && !ContainsSpecialSymbolsAndDigits(s);
        }

        public bool ContainsSpecialSymbolsAndDigits(string s)
        {
            foreach (char c in s)
            {
                if (char.IsDigit(c) || !char.IsLetterOrDigit(c))
                {
                    return true;
                }
            }
            return false;
        }

        private void lastnameTextBox_Validated(object sender, EventArgs e)
        {
        }

        private void lastnameTextBox_Validating(object sender, CancelEventArgs e)
        {
            fieldErrorLabel.Visible = false;
            if (!IsValid(lastnameTextBox.Text))
            {
                lastnameTextBox.BorderColor = System.Drawing.Color.Red;
                fieldErrorLabel.Visible = true;
            }
            else { lastnameTextBox.BorderColor = System.Drawing.Color.Green;}
            return;
        }

        private void firstnameTextBox_Validated(object sender, EventArgs e)
        {
        }

        private void firstnameTextBox_Validating(object sender, CancelEventArgs e)
        {
            fieldErrorLabel.Visible = false;
            if (!IsValid(firstnameTextBox.Text))
            {
                firstnameTextBox.BorderColor = System.Drawing.Color.Red;
                fieldErrorLabel.Visible = true;
            }
            else { firstnameTextBox.BorderColor = System.Drawing.Color.Green; }
            return;
        }

        private void loginTextBox_Validated(object sender, EventArgs e)
        {
        }
        public bool IsLoginValid(string login)
        {
            return login != null && login.Length > 4 && login.Length < 17;
        }
        private void loginTextBox_Validating(object sender, CancelEventArgs e)
        {
            loginErrorLabel.Visible = false;
            if (!IsLoginValid(loginTextBox.Text))
            {
                loginTextBox.BorderColor = System.Drawing.Color.Red;
                loginErrorLabel.Visible = true;
            }
            else { loginTextBox.BorderColor = System.Drawing.Color.Green; }
            return;
        }

        private void passwordTextBox_Validated(object sender, EventArgs e)
        {
        }

        private void passwordTextBox_Validating(object sender, CancelEventArgs e)
        {
            errorPasswordLabel.Visible = false;
            if (validator.IsMatch(passwordTextBox.Text))
            {
                passwordTextBox.BorderColor = System.Drawing.Color.Red;
                errorPasswordLabel.Visible = true;
            }
            else { passwordTextBox.BorderColor = System.Drawing.Color.Green; }
            return;
        }

        private void departmentTextBox_Validated(object sender, EventArgs e)
        {
        }

        private void departmentTextBox_Validating(object sender, CancelEventArgs e)
        {
            fieldErrorLabel.Visible = false;
            if (!IsValid(departmentTextBox.Text))
            {
                departmentTextBox.BorderColor = System.Drawing.Color.Red;
                fieldErrorLabel.Visible = true;
            }
            else { departmentTextBox.BorderColor = System.Drawing.Color.Green; }
            return;
        }

        private void universityTextBox_Validated(object sender, EventArgs e)
        {
        }

        private void universityTextBox_Validating(object sender, CancelEventArgs e)
        {
            fieldErrorLabel.Visible = false;
            if (!IsValid(universityTextBox.Text))
            {
                
                universityTextBox.BorderColor = System.Drawing.Color.Red;
                fieldErrorLabel.Visible = true;
            }
            else { universityTextBox.BorderColor = System.Drawing.Color.Green; }
            return;
        }

        private void middlenameTextBox_Validated(object sender, EventArgs e)
        {
        }

        private void middlenameTextBox_Validating(object sender, CancelEventArgs e)
        {
            fieldErrorLabel.Visible = false;
            if (!IsValid(middlenameTextBox.Text))
            {
                middlenameTextBox.BorderColor = System.Drawing.Color.Red;
                fieldErrorLabel.Visible = true;
            }
            else { middlenameTextBox.BorderColor = System.Drawing.Color.Green; }
            return;
        }

        private void jobPostTextBox_Validated(object sender, EventArgs e)
        {

        }

        private void jobPostTextBox_Validating(object sender, CancelEventArgs e)
        {
            fieldErrorLabel.Visible = false;
            if (!IsValid(jobPostTextBox.Text))
            {
                jobPostTextBox.BorderColor = System.Drawing.Color.Red;
                fieldErrorLabel.Visible = true;
            }
            else { jobPostTextBox.BorderColor = System.Drawing.Color.Green; }
            return;
        }
        #endregion
    }
}
