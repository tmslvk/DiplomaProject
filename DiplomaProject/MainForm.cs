using DiplomaProject.Model;
using System.Text.RegularExpressions;


namespace DiplomaProject
{
    public partial class MainForm : Form
    {
        public int userID;
        DiplomaBDContext db;

        public bool showHideCheckerOld = false;
        public bool showHideCheckerNew = false;
        public bool showChangeGroupBox = false;

        public Regex validator = new Regex("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&].{8,15}$");

        public MainForm(int id, DiplomaBDContext context)
        {
            InitializeComponent();
            this.db = context;
            this.userID = id;
            AddValuesBoot();
            idLabel.Text = userID.ToString();
            var user = GetUserById(userID);
            if (user != null)
            {
                fullnameLabel.Text = user.Fullname;
                departmentLabel.Text = user.Departmant;
                universityLabel.Text = user.University;
                jobPostLabel.Text = user.JobPost;
            }
            FillSchedule(GetLessons());
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            oldPasswordTextBoxChange.UseSystemPasswordChar = true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        #region[GroupBoxShowHide]
        private void profileGroupBoxButton_Click(object sender, EventArgs e)
        {
            profileGroupBox.Show();

            addInfoGroupBox.Hide();
            lessonGroupBox.Hide();
            scheduleGroupBox.Hide();
        }
        private void addCommonInfoGroupBoxButton_Click(object sender, EventArgs e)
        {
            addInfoGroupBox.Show();

            profileGroupBox.Hide();
            lessonGroupBox.Hide();
            scheduleGroupBox.Hide();
        }

        private void scheduleGroupBoxButton_Click(object sender, EventArgs e)
        {
            scheduleGroupBox.Show();
            scheduleDataGridView.Refresh();

            lessonGroupBox.Hide();
            addInfoGroupBox.Hide();
            profileGroupBox.Hide();

        }
        private void lessonGroupBoxButton_Click(object sender, EventArgs e)
        {
            lessonGroupBox.Show();

            addInfoGroupBox.Hide();
            profileGroupBox.Hide();
            scheduleGroupBox.Hide();
        }

        private void burdenGroupBoxButton_Click(object sender, EventArgs e)
        {

        }
        #endregion

        private void oldPasswordTextBoxChange_TextChanged(object sender, EventArgs e)
        {

        }

        public User? GetUserById(int id)
        {

            return db.Users.FirstOrDefault(u => u.Id == id);
        }

        private void forgotPasswordLabelButton_Click(object sender, EventArgs e)
        {
            changePasswordGroupBox.Visible = true;
        }

        #region[ShowHidePasswords]
        private void showHideOldPasswordButton_Click(object sender, EventArgs e)
        {
            if (!showHideCheckerOld)
            {
                showHideOldPasswordButton.Image = Properties.Resources.showPic as Image;
                oldPasswordTextBoxChange.PasswordChar = '\0';
                oldPasswordTextBoxChange.UseSystemPasswordChar = false;
                showHideCheckerOld = true;
                return;
            }
            if (showHideCheckerOld)
            {
                showHideOldPasswordButton.Image = Properties.Resources.hidePic as Image;
                oldPasswordTextBoxChange.PasswordChar = '●';
                oldPasswordTextBoxChange.UseSystemPasswordChar = true;
                showHideCheckerOld = false;
                return;
            }
        }

        private void showHideNewPasswordButton_Click(object sender, EventArgs e)
        {
            if (!showHideCheckerNew)
            {
                showHideNewPasswordButton.Image = Properties.Resources.showPic as Image;
                newPasswordTextBoxChange.PasswordChar = '\0';
                newPasswordTextBoxChange.UseSystemPasswordChar = false;
                showHideCheckerNew = true;
                return;
            }
            if (showHideCheckerNew)
            {
                showHideNewPasswordButton.Image = Properties.Resources.hidePic as Image;
                newPasswordTextBoxChange.PasswordChar = '●';
                newPasswordTextBoxChange.UseSystemPasswordChar = true;
                showHideCheckerNew = false;
                return;
            }
        }
        #endregion

        private void hideChangeGroupBoxButton_Click(object sender, EventArgs e)
        {
            changePasswordGroupBox.Visible = false;
        }

        private void changePasswordButton_Click(object sender, EventArgs e)
        {
            var user = GetUserById(userID);
            if (user != null)
            {
                if (user.Password == oldPasswordTextBoxChange.Text && validator.IsMatch(newPasswordTextBoxChange.Text))
                {
                    using (db)
                    {
                        user.Password = newPasswordTextBoxChange.Text;
                        db.SaveChanges();
                    }
                    changePasswoedGratsLabel.Visible = true;
                    return;
                }
                else if (user.Password != oldPasswordTextBoxChange.Text)
                {
                    errorLabel.Text = "Старый пароль неверен";
                    errorLabel.Visible = true;
                    return;
                }
                else if (newPasswordTextBoxChange.Text == string.Empty)
                {
                    errorLabel.Text = "Пароль неверен";
                    errorLabel.Visible = true;
                    return;
                }
                else
                {
                    errorLabel.Visible = true;
                    return;
                }
            }
            else
            {
                errorLabel.Text = "Пользователя не существует";
                errorLabel.Visible = true;
            }

        }

        private void backButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm form = new LoginForm();
            form.Show();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public Lesson? GetScheduleById()
        {
            return db.Lessons.FirstOrDefault<Lesson?>(s => s.UserId == userID);
        }

        private void addLessonButtonSchedule_Click(object sender, EventArgs e)
        {
            scheduleBindingSource.AddNew();
        }

        private void saveLessonButtonSchedule_Click(object sender, EventArgs e)
        {

        }

        private void ScheduleGroupBox_Click(object sender, EventArgs e)
        {

        }
        #region[AddValuesForComboBoxes]
        public void AddValuesBoot()
        {
            AddUpdateDiscipline();
            AddUpdateGroup();
            AddUpdatePlace();
            AddLessonPlaceComboBox();
            AddLessonDisciplineComboBox();
            AddLessonGroupComboBox();
            InitializationValuesForComboBoxDay();
            DeleteTimeGroupComboBox();
        }
        private void addPlaceOfLessonButton_Click(object sender, EventArgs e)
        {
            PlaceOfLesson placeOfLesson = new PlaceOfLesson()
            {
                User = GetUserById(userID),
                Place = addPlaceOfLessonTextBox.Text
            };
            db.PlaceOfLessons.Add(placeOfLesson);
            db.SaveChanges();
            deletePlaceOfLessonComboBox.Items.Clear();
            AddUpdatePlace();
        }

        private void addDisciplineButton_Click(object sender, EventArgs e)
        {

            Discipline discipline = new Discipline()
            {
                User = GetUserById(userID),
                NameOfDiscipline = addDisciplineTextBox.Text
            };
            db.Disciplines.Add(discipline);
            db.SaveChanges();
            deleteDisciplineComboBox.Items.Clear();
            AddUpdateDiscipline();
        }

        private void addGroupButton_Click(object sender, EventArgs e)
        {

            Model.Group group = new Model.Group()
            {
                User = GetUserById(userID),
                GroupNumber = addGroupsTextBox.Text
            };
            db.Groups.Add(group);
            db.SaveChanges();
            deleteGroupComboBox.Items.Clear();
            AddUpdateGroup();
        }
        #endregion

        #region[ComboBoxForDelete]
        private void deleteGroupButton_Click(object sender, EventArgs e)
        {
            if (deleteGroupComboBox.SelectedItem != null)
            {
                db.Groups.Remove(db.Groups.FirstOrDefault(g => g.UserId == userID && g.GroupNumber == deleteGroupComboBox.SelectedItem));
            }
            db.SaveChanges();
            deleteGroupComboBox.Items.Clear();
            AddUpdateGroup();
        }

        private void deleteDisciplineButton_Click(object sender, EventArgs e)
        {
            if (deleteDisciplineComboBox.SelectedItem != null)
            {
                db.Disciplines.Remove(db.Disciplines.FirstOrDefault(g => g.UserId == userID && g.NameOfDiscipline == deleteDisciplineComboBox.SelectedItem));
            }
            db.SaveChanges();
            deleteDisciplineComboBox.Items.Clear();
            AddUpdateDiscipline();
        }

        private void deletePlaceButton_Click(object sender, EventArgs e)
        {
            if (deletePlaceOfLessonComboBox.SelectedItem != null)
            {
                db.PlaceOfLessons.Remove(db.PlaceOfLessons.FirstOrDefault(g => g.UserId == userID && g.Place == deletePlaceOfLessonComboBox.SelectedItem));
            }
            db.SaveChanges();
            deletePlaceOfLessonComboBox.Items.Clear();
            AddUpdatePlace();
        }

        public void AddUpdatePlace()
        {
            List<PlaceOfLesson> places = db.PlaceOfLessons.ToList();
            for (int i = 0; i < places.Count; i++)
            {
                if (places[i].UserId == userID)
                {
                    deletePlaceOfLessonComboBox.Items.Add(places[i].Place);
                }
            }
        }
        public void AddUpdateGroup()
        {
            List<Model.Group> groups = db.Groups.ToList();
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].UserId == userID)
                {
                    deleteGroupComboBox.Items.Add(groups[i].GroupNumber);
                }
            }
        }
        public void AddUpdateDiscipline()
        {
            List<Discipline> disciplines = db.Disciplines.ToList();
            for (int i = 0; i < disciplines.Count; i++)
            {
                if (disciplines[i].UserId == userID)
                {
                    deleteDisciplineComboBox.Items.Add(disciplines[i].NameOfDiscipline);
                }
            }
        }
        #endregion

        #region[AddValuesForComboBoxesDeleteAdd]
        public void AddLessonGroupComboBox()
        {
            List<Model.Group> groups = db.Groups.ToList();

            if (groups != null)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    if (groups[i].UserId == userID)
                    {
                        groupAddLessonComboBox.Items.Add(groups[i].GroupNumber);
                    }
                }
            }

        }
        public void AddLessonDisciplineComboBox()
        {
            List<Discipline> disciplines = db.Disciplines.ToList();
            for (int i = 0; i < disciplines.Count; i++)
            {
                if (disciplines[i].UserId == userID)
                {
                    disciplineAddLessonComboBox.Items.Add(disciplines[i].NameOfDiscipline);
                }
            }
        }
        public void AddLessonPlaceComboBox()
        {
            List<PlaceOfLesson> places = db.PlaceOfLessons.ToList();
            for (int i = 0; i < places.Count; i++)
            {
                if (places[i].UserId == userID)
                {
                    placeAddLessonComboBox.Items.Add(places[i].Place);
                }
            }
        }
        public void InitializationValuesForComboBoxDay()
        {
            List<DayOfWeekItem> daysOfWeek = new List<DayOfWeekItem>
            {
                new DayOfWeekItem { Id = 1, Name = "Понедельник" },
                new DayOfWeekItem { Id = 2, Name = "Вторник" },
                new DayOfWeekItem { Id = 3, Name = "Среда" },
                new DayOfWeekItem { Id = 4, Name = "Четверг" },
                new DayOfWeekItem { Id = 5, Name = "Пятница" },
                new DayOfWeekItem { Id = 6, Name = "Суббота" },
            };

            dayAddLessonComboBox.DataSource = daysOfWeek;
            dayAddLessonComboBox.DisplayMember = "Name";
            dayAddLessonComboBox.ValueMember = "Id";
        }

        public void DeleteTimeGroupComboBox()
        {
            List<Lesson> lessons = db.Lessons.ToList();
            if (lessons != null)
            {
                for (int i = 0; i < lessons.Count; i++)
                {
                    if (lessons[i].UserId == userID)
                    {
                        timeDeleteLessonComboBox.Items.Add(lessons[i].TimeOfLesson);
                    }
                }
            }
        }
        #endregion

        private void addLessonButton_Click(object sender, EventArgs e)
        {
            var timeOfLesson = DateTime.Parse(timeAddLessonTextBox.Text).TimeOfDay;
            DayOfWeekItem selectedDay = (DayOfWeekItem)dayAddLessonComboBox.SelectedItem;
            string selectedDayName = selectedDay.Name;
            Lesson newLesson = new Lesson()
            {
                Day = selectedDayName,
                DayOfWeek = (int)dayAddLessonComboBox.SelectedValue,
                TypeOfWeek = typeOfWeekAddLessonComboBox.SelectedItem.ToString(),
                Discipline = disciplineAddLessonComboBox.SelectedItem.ToString(),
                Group = groupAddLessonComboBox.SelectedItem.ToString(),
                PlaceOfLesson = placeAddLessonComboBox.SelectedItem.ToString(),
                TimeOfLesson = timeOfLesson,
                TypeOfLesson = typeLessonAddLessonComboBox.SelectedItem.ToString(),
                User = GetUserById(userID),
            };
            if (IsRepeated(newLesson.TimeOfLesson, newLesson.Day, newLesson) && RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day) == null)
            {
                db.Lessons.Add(newLesson);
                db.SaveChanges();
                return;
            }
            else
            {

                BackupLessons backupLessons = new BackupLessons()
                {
                    Lesson = RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day),
                    TimeOfChange = DateTime.Now,
                    LessonId = RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day).Id

                };
                db.BackupLessons.Add(backupLessons);
                db.Lessons.Add(newLesson);
                db.SaveChanges();
                searchResultTextBox.Clear();
                searchResultTextBox.Text = "Занятие было перезаписано";
                return;
            }
        }

        private void findLessonButton_Click(object sender, EventArgs e)
        {
            var lesson = FindLesson(typeOfWeekDeleteLessonComboBox.SelectedItem.ToString(), dayDeleteLessonComboBox.SelectedItem.ToString(), TimeSpan.Parse(timeDeleteLessonComboBox.SelectedItem.ToString()));
            if (lesson != null)
            {
                searchResultTextBox.Text =
                    $"{lesson.Day}, {lesson.TimeOfLesson}" + Environment.NewLine +
                    $"Неделя: {lesson.TypeOfWeek}" + Environment.NewLine +
                    $"{lesson.TypeOfLesson} " + Environment.NewLine +
                    $"Дисциплина: {lesson.Discipline} " + Environment.NewLine +
                    $"Группа: {lesson.Group} " + Environment.NewLine +
                    $"Аудитория: {lesson.PlaceOfLesson}";
                return;
            }
            searchResultTextBox.Clear();
            searchResultTextBox.Text = "Такого занятия не найдено";
        }

        public Lesson? FindLesson()
        {
            return db.Lessons.FirstOrDefault(s => s.UserId == userID);
        }

        public Lesson? FindLesson(string typeOfWeek, string day, TimeSpan time)
        {
            return db.Lessons.OrderBy(e => e.Id).LastOrDefault(
                s => s.UserId == userID &&
                s.TimeOfLesson == time &&
                s.TypeOfWeek == typeOfWeek &&
                s.Day == day);
        }

        public Lesson? RepeatedLesson(TimeSpan? time, string day)
        {
            return db.Lessons.FirstOrDefault(l => l.TimeOfLesson == time && l.Day == day && l.UserId == userID);
        }

        public bool IsRepeated(TimeSpan? time, string day, Lesson newLesson)
        {
            if (RepeatedLesson(time, day) != null)
            {
                if (RepeatedLesson(time, day).TimeOfLesson != newLesson.TimeOfLesson && RepeatedLesson(time, day).Day != newLesson.Day && RepeatedLesson(time, day).UserId == userID)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void viewScheduleButton_Click(object sender, EventArgs e)
        {
            ScheduleForm scheduleForm = new ScheduleForm();
            scheduleForm.Show();
        }

        public List<Lesson> GetLessons()
        {
            var sortedData = db.Lessons
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.TimeOfLesson).Where(x => x.UserId == userID)
            .ToList();

            //scheduleDataGridView.DataSource = sortedData;
            var backup = db.BackupLessons.ToList();
            List<Lesson> finalList = new List<Lesson>();
            if(backup != null)
            {
                for (int i = 0; i < backup.Count; i++)
                {
                    for (int j = 0; j < sortedData.Count; j++)
                    {
                        if (backup[i].LessonId != sortedData[j].Id)
                        {
                            finalList.Add(sortedData[j]);
                        }
                        return finalList;
                    }
                    return finalList;
                }
            }
            
            return sortedData;
        }

        public void FillSchedule(List<Lesson> finalList)
        {
            scheduleDataGridView.DataSource = finalList;
        }

        public class DayOfWeekItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private void createFillingFromScheduleButton_Click(object sender, EventArgs e)
        {
            FillWorkLoadTable(GetLessons());
        }

        public void FillWorkLoadTable(List<Lesson> lessons)
        {
            foreach (var lesson in lessons)
            {
                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); day++)
                {
                    DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, day);
                    if ((int)date.DayOfWeek == lesson.DayOfWeek)
                    {
                        bool addLesson = false;
                        if (lesson.TypeOfWeek == "Еженедельно")
                        {
                            addLesson = true;
                        }
                        else if (lesson.TypeOfWeek == "Четная" && day % 2 == 0)
                        {
                            addLesson = true;
                        }
                        else if (lesson.TypeOfWeek == "Нечетная" && day % 2 != 0)
                        {
                            addLesson = true;
                        }

                        if (addLesson)
                        {
                            db.Fillings.Add(new Filling
                            {
                                Day = day,
                                Month = DateTime.Now.Month,
                                Year = DateTime.Now.Year,
                                Hours = 2, // фиксированное количество в 2 часа
                                Disciplenes = lesson.Discipline,
                                TypeOfLesson = lesson.TypeOfLesson,
                                User = GetUserById(userID)
                                
                            });
                        }
                    }
                }
            }
            db.SaveChanges();
        }
    }
}
