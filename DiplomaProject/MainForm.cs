using DiplomaProject.Model;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Reporting.WinForms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using TheArtOfDevHtmlRenderer.Core;

namespace DiplomaProject
{
    public partial class MainForm : Form
    {
        public int userID;
        DiplomaBDContext db;

        public bool showHideCheckerOld = false;
        public bool showHideCheckerNew = false;
        public bool showChangeGroupBox = false;

        public string schedulePath = "Schedule.xlsx";
        public string workloadPath = "Workload.xlsx";
        public Regex validator = new Regex("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&].{8,15}$");
        #region[Lists&Dictionaties]
        List<DayOfWeek> dayOfWeeks = new List<DayOfWeek>()
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

        public Dictionary<int, string> months = new Dictionary<int, string>()
        {
            {1, "Январь" },
            {2, "Февраль" },
            {3, "Март" },
            {4, "Апрель" },
            {5, "Май" },
            {6, "Июнь" },
            {7, "Июль" },
            {8, "Август" },
            {9, "Сентябрь" },
            {10, "Октябрь" },
            {11, "Ноябрь" },
            {12, "Декабрь" }
        };



        public Dictionary<string, string> typeOfWeeks = new Dictionary<string, string>()
        {
            {"Еженедельно", ""},
            {"Нечетная", "*"},
            {"Четная", "**"},
        };

        Dictionary<DayOfWeek, string> dayDictionary = new Dictionary<DayOfWeek, string>()
        {
            {DayOfWeek.Monday, "Понедельник" },
            {DayOfWeek.Tuesday, "Вторник" },
            {DayOfWeek.Wednesday, "Среда" },
            {DayOfWeek.Thursday, "Четверг" },
            {DayOfWeek.Friday, "Пятница" },
            {DayOfWeek.Saturday, "Суббота" },

        };

        public Dictionary<string, string> typeOfLessons = new Dictionary<string, string>()
        {
            {"Лекции", "ЛК"},
            {"Практич. и семинарские занятия", "ПЗиСЗ"},
            {"Лабораторные занятия", "ЛЗ"},
            {"Курсовое проектирование", "КП"},
            {"Консультации", "К"},
            {"Зачеты", "ЗАЧ"},
            {"Экзамены", "ЭКЗ" },
            {"Руководство аспирантами", "РА" },
            {"Дипломное проектирование", "ДП" },
            {"ГЭК", "ГЭК" },
            {"Учебные и произв. практики", "УиПП" },
            {"Руководство магистрантами", "РМ" },
            {"Контрольные работы и РГР", "КРиРГР" }
        };

        List<TimeSpan> timeOfLessons;
        List<ExcelDocumentInfo> documentInfo;
        #endregion

        public MainForm(int id, DiplomaBDContext context)
        {
            InitializeComponent();
            this.db = context;
            this.userID = id;
            AddValuesBoot();
            profileGroupBox.Show();
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
            FillWorkloadView(SortWorkload());
            //RemoveExpiredLessons();
        }
        #region[Validators]
        private void addGroupsTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            addInfoLabel.Visible = true;
            if (!IsValid(addGroupsTextBox.Text))
            {
                addGroupsTextBox.BorderColor = System.Drawing.Color.Red;
                addInfoLabel.Text = "Поле должно быть длиной от 4 до 36 символов";
                addInfoLabel.Visible = true;
            }
            else { addGroupsTextBox.BorderColor = System.Drawing.Color.Green; }
        }

        private void addDisciplineTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            addInfoLabel.Visible = true;
            if (!IsValid(addDisciplineTextBox.Text))
            {
                addDisciplineTextBox.BorderColor = System.Drawing.Color.Red;
                addInfoLabel.Text = "Поле должно быть длиной от 4 до 36 символов";
                addInfoLabel.Visible = true;
            }
            else { addDisciplineTextBox.BorderColor = System.Drawing.Color.Green; }
        }

        private void addPlaceOfLessonTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            addInfoLabel.Visible = true;
            if (!IsValid(addPlaceOfLessonTextBox.Text))
            {
                addPlaceOfLessonTextBox.BorderColor = System.Drawing.Color.Red;
                addInfoLabel.Text = "Поле должно быть длиной от 4 до 36 символов";
                addInfoLabel.Visible = true;
            }
            else { addPlaceOfLessonTextBox.BorderColor = System.Drawing.Color.Green; }
        }

        public bool IsValid(string s)
        {
            return s != null && s.Length > 4 && s.Length < 36;
        }

        private void timeAddLessonTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsTimeValid(timeAddLessonTextBox.Text))
            {
                timeAddLessonTextBox.BorderColor = System.Drawing.Color.Red;
            }
            else { timeAddLessonTextBox.BorderColor = System.Drawing.Color.Green; }
        }

        public bool IsTimeValid(string time)
        {
            string pattern = @"^([01]?[0-9]|2[0-4]):[0-5][0-9]$";
            return time != null && Regex.IsMatch(time, pattern);
        }
        #endregion
        public string GetPathForExcelFiles()
        {
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

            // Проверяем, содержит ли путь к папке "bin"
            int index = projectDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                // Удаляем подстроку "bin"
                projectDirectory = projectDirectory.Remove(index, 3);

                // Удаляем последний разделитель, если он был добавлен после удаления "bin"
                projectDirectory = projectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            string excelFilesPath = Path.Combine(projectDirectory, "ExcelFiles");
            return excelFilesPath;
        }
        public void RemoveExpiredLessons()
        {
            if (db.BackupLessons != null)
            {
                foreach (var backup in db.BackupLessons)
                {
                    foreach (var lesson in db.Lessons)
                    {
                        if (backup.LessonId == lesson.Id && DateTime.Now > backup.ExpireDateTime)
                        {
                            db.Lessons.Remove(lesson);
                        }
                        return;
                    }
                }
            }
            return;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            oldPasswordTextBoxChange.UseSystemPasswordChar = true;
            this.timeOfLessons = GetTimeOfLessons(GetLessonsForTimeSpans());
            workloadGroupBox.Hide();
            scheduleGroupBox.Hide();
            addInfoGroupBox.Hide();
            lessonGroupBox.Hide();
            profileGroupBox.Show();

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
            workloadGroupBox.Hide();
        }
        private void addCommonInfoGroupBoxButton_Click(object sender, EventArgs e)
        {
            addInfoGroupBox.Show();

            profileGroupBox.Hide();
            lessonGroupBox.Hide();
            scheduleGroupBox.Hide();
            workloadGroupBox.Hide();
        }

        private void scheduleGroupBoxButton_Click(object sender, EventArgs e)
        {
            scheduleGroupBox.Show();
            scheduleDataGridView.Refresh();
            scheduleDataGridView.Update();

            lessonGroupBox.Hide();
            addInfoGroupBox.Hide();
            profileGroupBox.Hide();
            workloadGroupBox.Hide();
        }
        private void lessonGroupBoxButton_Click(object sender, EventArgs e)
        {
            lessonGroupBox.Show();
            scheduleDataGridView.Update();

            addInfoGroupBox.Hide();
            profileGroupBox.Hide();
            scheduleGroupBox.Hide();
            workloadGroupBox.Hide();
        }

        private void workloadGroupBoxButton_Click(object sender, EventArgs e)
        {
            workloadGroupBox.Show();
            workloadDataGridView.Update();

            scheduleGroupBox.Hide();
            profileGroupBox.Hide();
            lessonGroupBox.Hide();
            addInfoGroupBox.Hide();
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

        #region[AddValuesForComboBoxes]
        public void AddValuesBoot()
        {
            AddMonthProfileGroupBox();
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
            addInfoLabel.Text = "";
            PlaceOfLesson placeOfLesson = new PlaceOfLesson()
            {
                User = GetUserById(userID),
                Place = addPlaceOfLessonTextBox.Text
            };
            db.PlaceOfLessons.Add(placeOfLesson);
            db.SaveChanges();
            deletePlaceOfLessonComboBox.Items.Clear();
            AddUpdatePlace();
            addInfoLabel.Text = $"Место занятия {placeOfLesson.Place} было добавлено";
        }

        private void addDisciplineButton_Click(object sender, EventArgs e)
        {
            addInfoLabel.Text = "";
            Discipline discipline = new Discipline()
            {
                User = GetUserById(userID),
                NameOfDiscipline = addDisciplineTextBox.Text
            };
            db.Disciplines.Add(discipline);
            db.SaveChanges();
            deleteDisciplineComboBox.Items.Clear();
            AddUpdateDiscipline();
            addInfoLabel.Text = $"Дисциплина {discipline.NameOfDiscipline} была добавлена";
        }

        private void addGroupButton_Click(object sender, EventArgs e)
        {
            addInfoLabel.Text = "";
            Model.Group group = new Model.Group()
            {
                User = GetUserById(userID),
                GroupNumber = addGroupsTextBox.Text
            };
            db.Groups.Add(group);
            db.SaveChanges();
            deleteGroupComboBox.Items.Clear();
            AddUpdateGroup();
            addInfoLabel.Text = $"Группа {group.GroupNumber} была добавлена";
        }
        #endregion

        #region[ComboBoxForDelete]
        private void deleteGroupButton_Click(object sender, EventArgs e)
        {
            deleteInfoLabel.Text = "";
            if (deleteGroupComboBox.SelectedItem != null)
            {
                db.Groups.Remove(db.Groups.FirstOrDefault(g => g.UserId == userID && g.GroupNumber == deleteGroupComboBox.SelectedItem));
            }
            db.SaveChanges();
            deleteInfoLabel.Text = $"Группа {deleteGroupComboBox.SelectedItem} была удалена";
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
            deleteInfoLabel.Text = $"Дисциплина {deleteDisciplineComboBox.SelectedItem} была удалена";
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
            deleteInfoLabel.Text = $"Место занятия {deletePlaceOfLessonComboBox.SelectedItem} было удалена";
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
        //добавить валидации чучуть
        #region[AddValuesForComboBoxesDeleteAdd]

        public void AddMonthProfileGroupBox()
        {
            foreach (var month in months.Values)
            {
                monthComboBox.Items.Add(month);
            }
        }

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
            var list = GetTimeOfLessons(db.Lessons.Where(l => l.UserId == userID).ToList());
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    timeDeleteLessonComboBox.Items.Add(list[i].ToString());
                }
            }
        }
        #endregion

        #region[AddUpdateLessons]
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
                weeksAddLabel.Visible = true;
                weeksUpDown.Visible = true;
                updateLessonButton.Visible = true;

                addLessonButton.Visible = false;

                searchResultTextBox.Clear();
                searchResultTextBox.Text = $"В выбранное вами время уже существует занятие!" + Environment.NewLine +
                    $"Это {RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day).TypeOfLesson}" + Environment.NewLine +
                    $"{RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day).Discipline}" + Environment.NewLine +
                    $"{RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day).Group}" + Environment.NewLine +
                    $"Заменить его?";
            }
        }
        private void updateLessonButton_Click(object sender, EventArgs e)
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
            if (!IsRepeated(newLesson.TimeOfLesson, newLesson.Day, newLesson) && RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day) != null)
            {
                db.Lessons.Add(newLesson);
                db.SaveChanges();
                BackupLessons oldBackupLessons = new BackupLessons()
                {
                    Lesson = RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day),
                    LessonId = RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day).Id,
                    NextLessonTime = GetNextLessonDate(RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day)),
                    ExpireDateTime = GetNextLessonDate(RepeatedLesson(newLesson.TimeOfLesson, newLesson.Day)).AddDays((double)weeksUpDown.Value * 7),
                    IsNew = false,
                };
                db.BackupLessons.Add(oldBackupLessons);
                BackupLessons newBackupLessons = new BackupLessons()
                {
                    Lesson = newLesson,
                    LessonId = newLesson.Id,
                    NextLessonTime = GetNextLessonDate(newLesson),
                    ExpireDateTime = GetNextLessonDate(newLesson).AddDays((double)weeksUpDown.Value * 7),
                    IsNew = true
                };
                db.BackupLessons.Add(newBackupLessons);
                db.SaveChanges();
            }
        }

        public DateTime GetNextLessonDate(Lesson lesson)
        {
            DayOfWeek nextDayOfLesson = (DayOfWeek)lesson.DayOfWeek;
            int daysUntilNextLesson = (nextDayOfLesson - DateTime.Now.DayOfWeek + 7) % 7;

            return DateTime.Now.AddDays(daysUntilNextLesson);
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

        public Lesson? GetLastAddedLesson(TimeSpan? time, string day)
        {
            return db.Lessons.LastOrDefault(l => l.TimeOfLesson == time && l.Day == day && l.UserId == userID);
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
        #endregion

        public List<Lesson> GetLessons()
        {
            var sortedData = db.Lessons
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.TimeOfLesson).Where(x => x.UserId == userID)
            .ToList();

            //scheduleDataGridView.DataSource = sortedData;
            var backup = db.BackupLessons.ToList();
            var now = DateTime.Now;
            if (backup != null)
            {
                List<Lesson> finalList = new List<Lesson>();
                foreach (var lesson in sortedData)
                {
                    var hasBackup = backup.Any((item) => HasBackup(lesson, item));
                    if (!hasBackup)
                    {
                        finalList.Add(lesson);
                    }
                }

                return finalList;
            }

            return sortedData;
        }

        private bool HasBackup(Lesson lesson, BackupLessons backupLesson)
        {
            var now = DateTime.Now;
            return backupLesson.LessonId == lesson.Id &&
                (backupLesson.ExpireDateTime < now && backupLesson.IsNew
                ||
                backupLesson.ExpireDateTime > now && !backupLesson.IsNew);
        }
        #region[WorkloadLogic]
        public void FillWorkloadView(List<Workload> workload)
        {
            workloadDataGridView.DataSource = workload;
        }

        public List<Workload> SortWorkload()
        {
            var lessons = db.Lessons.Where(l => l.UserId == userID).ToList();
            var sortedData = db.Workloads
            .OrderBy(x => x.Day)
            .Where(x => lessons.Contains(x.Lesson))
            .ToList();

            var finalList = new List<Workload>();
            if (sortedData != null)
            {
                foreach (var filling in sortedData)
                {
                    if (filling.Month == DateTime.Now.Month && filling.Year == DateTime.Now.Year)
                    {
                        finalList.Add(filling);
                    }
                }
                return finalList;
            }
            return sortedData;
        }

        public void FillSchedule(List<Lesson> finalList)
        {
            scheduleDataGridView.DataSource = finalList;
        }



        private void createFillingFromScheduleButton_Click(object sender, EventArgs e)
        {
            firstFillingGratsLabel.Text = "";
            FillWorkLoadTable(GetLessons());
            firstFillingGratsLabel.Text = "Перенос расписания на нагрузку был успешен";
            firstFillingGratsLabel.Visible = true;
        }

        public void FillWorkLoadTable(List<Lesson> lessons)
        {
            foreach (var lesson in lessons)
            {
                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); day++)
                {
                    DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, day);

                    if (lesson.BackupLessons != null && lesson.BackupLessons.Any(b => b.IsNew ? b.ExpireDateTime < date : b.ExpireDateTime > date)) continue;
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
                            db.Workloads.Add(new Workload
                            {
                                Day = day,
                                Month = DateTime.Now.Month,
                                Year = DateTime.Now.Year,
                                Hours = 2,
                                Disciplenes = lesson.Discipline,
                                TypeOfLesson = lesson.TypeOfLesson,
                                Lesson = lesson,

                            });
                        }
                    }
                }
            }
            db.SaveChanges();
        }

        public List<Workload> GetWorkload()
        {
            var lessons = db.Lessons.Where(l => l.UserId == userID).ToList();
            return db.Workloads.Where(l => lessons.Contains(l.Lesson) && l.Month == DateTime.Now.Month && l.Year == DateTime.Now.Year).ToList();
        }

        private void updateWorkloadButton_Click(object sender, EventArgs e)
        {
            workloadGratsLabel.Text = "";
            List<Lesson> lessons = db.Lessons.Where(l => l.UserId == userID).ToList();

            var oldWorloads = db.Workloads.Where(w => lessons.Contains(w.Lesson)).ToList();
            db.Workloads.RemoveRange(oldWorloads);
            FillWorkLoadTable(lessons);
            FillWorkloadView(SortWorkload());

            workloadGratsLabel.Text = "Нагрузка была успешно обновлена";
            workloadGratsLabel.Visible = true;
        }
        #endregion

        #region[Schedule&Excel]

        public void CreateScheduleOnExcel()
        {
            //закалить пояснения
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Расписание");

            int startRow = 2;
            int startCol = 2;

            worksheet.Cells[1, 1].Value = "Время \\ Дни";
            worksheet.Cells[dayDictionary.Count + startCol, 1].Value = "*";
            worksheet.Cells[dayDictionary.Count + startCol, 2].Value = "Нечетная";
            worksheet.Cells[dayDictionary.Count + startCol + 1, 1].Value = "**";
            worksheet.Cells[dayDictionary.Count + startCol + 1, 2].Value = "Четная";
            for (int i = 0; i < dayDictionary.Count; i++)
            {
                worksheet.Cells[i + startCol, 1].Value = dayDictionary[dayOfWeeks[i]];
                for (int j = 0; j < timeOfLessons.Count; j++)
                {
                    worksheet.Cells[1, j + startRow].Value = timeOfLessons[j].ToString();
                }
            }

            package.SaveAs(new FileInfo(GetFilePath(CreateFileNameSchedule(DateTime.Now.Year))));
        }

        public void FillDataInExcelSchedule()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage package = new ExcelPackage(GetFilePath(CreateFileNameSchedule(DateTime.Now.Year)));
            List<Lesson> lessons = GetLessons(); // Здесь GetLessonsData() - ваш метод для получения данных из таблицы
            ExcelWorksheet worksheet = package.Workbook.Worksheets["Расписание"];
            int startRow = 2;
            int startCol = 2;
            Action clearRows = () =>
            {
                for (int i = startRow; i <= worksheet.Dimension.Rows; i++)
                {
                    for (int j = startCol; j <= worksheet.Dimension.Columns; j++)
                    {
                        worksheet.Cells[i, j].Value = "";
                    }
                }
            };

            Func<Lesson, int> getRow = (lesson) =>
            {
                var day = lesson.Day;
                for (int i = startRow; i <= worksheet.Dimension.Rows; i++)
                {
                    if (day == worksheet.Cells[i, 1].Value.ToString())
                    {
                        return i;
                    }
                }

                return -1;
            };

            Func<Lesson, int> getCol = (lesson) =>
            {
                var timeOfLesson = lesson.TimeOfLesson;
                for (int j = startCol; j <= worksheet.Dimension.Columns; j++)
                {
                    if (timeOfLesson.ToString() == worksheet.Cells[1, j].Value.ToString())
                    {
                        return j;
                    }
                }

                return -1;
            };

            clearRows();

            foreach (var lesson in lessons)
            {
                var col = getCol(lesson);
                var row = getRow(lesson);

                if (col != -1 && row != -1)
                {
                    if (worksheet.Cells[row, col].Value.ToString() != String.Empty)
                    {
                        worksheet.Cells[row, col].Value += Environment.NewLine;
                    }
                    for (int i = 0; i < typeOfLessons.Count; i++)
                    {
                        for (int j = 0; j < typeOfWeeks.Count; j++)
                        {
                            if (lesson.TypeOfWeek == typeOfWeeks.ElementAt(j).Key && lesson.TypeOfLesson == typeOfLessons.ElementAt(i).Key)
                            {
                                worksheet.Cells[row, col].Value +=
                                    $"{typeOfWeeks.ElementAt(j).Value}{typeOfLessons.ElementAt(i).Value}" + Environment.NewLine +
                                    $"{lesson.Discipline}" + $" {lesson.PlaceOfLesson}" + $" {lesson.Group}";
                            }
                        }
                    }

                }
            }

            package.Save();
        }

        public List<Lesson> GetLessonsForTimeSpans()
        {
            return db.Lessons.Where(l => l.UserId == userID).ToList();
        }

        public List<TimeSpan> GetTimeOfLessons(List<Lesson> lessons)
        {
            return lessons.Select(l => l.TimeOfLesson).OfType<TimeSpan>().Distinct().OrderBy(t => t).ToList();
        }

        private void createTemplateOfScheduleButton_Click(object sender, EventArgs e)
        {
            firstFillingGratsLabel.Text = "";
            CreateScheduleOnExcel();
            firstFillingGratsLabel.Text = "Шаблон расписания был успешно создан";
            firstFillingGratsLabel.Visible = true;
        }

        private void fillScheduleExcelButton_Click(object sender, EventArgs e)
        {
            firstFillingGratsLabel.Text = "";
            FillDataInExcelSchedule();
            firstFillingGratsLabel.Text = "Файл Excel был успешно заполнен";
            firstFillingGratsLabel.Visible = true;
        }
        #endregion

        #region[Workload&Excel]
        public void CreateTemplateWorkload()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Нагрузка");

            int startRow = 4;
            int startCol = 2;

            worksheet.Cells[3, 1].Value = "Дата";
            worksheet.Cells[3, 1].Worksheet.Column(1).Width = 5.78;
            worksheet.Cells[3, 1, 4, 1].Merge = true;
            for (int i = 0; i <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - 1; i++)
            {
                //заполнение дней
                worksheet.Cells[i + startRow + 1, 1].Value = i + 1;
                worksheet.Cells[i + startRow + 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[startRow + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + 1, 1].Value = "Итого:";
                worksheet.Cells[startRow - 1, typeOfLessons.Count + startCol, startRow, typeOfLessons.Count + startCol].Merge = true;
                worksheet.Cells[startRow - 1, typeOfLessons.Count + startCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[startRow - 1, typeOfLessons.Count + startCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[startRow - 1, typeOfLessons.Count + startCol].Value = "Наименование дисциплины";
                worksheet.Cells[startRow - 1, typeOfLessons.Count + startCol].Style.WrapText = true;
                worksheet.Cells[startRow - 1, typeOfLessons.Count + startCol].Worksheet.Column(typeOfLessons.Count + startCol).Width = 14;
                FillInfoInWorksheet(worksheet);

                for (int j = 0; j < typeOfLessons.Count; j++)
                {
                    //worksheet.Cells[1, j + startRow].Value = typeOfLessons[j].ToString();
                    worksheet.Cells[startRow, j + startCol].Value = typeOfLessons.ElementAt(j).Key;
                    worksheet.Cells[startRow, j + startCol].Style.TextRotation = 90;
                    worksheet.Cells[startRow, j + startCol].Worksheet.Row(startRow).Height = 95;
                    worksheet.Cells[startRow, j + startCol].Worksheet.Column(j + startCol).Width = 5;
                    worksheet.Cells[startRow, j + startCol].Style.WrapText = true;
                }
            }

            package.SaveAs(new FileInfo(GetFilePath(CreateFileNameWorkload(DateTime.Now.Year, DateTime.Now.Month))));
        }

        public void FillInfoInWorksheet(ExcelWorksheet worksheet)
        {
            this.documentInfo = FillListExcelDocumentInfo(worksheet);
            for (int i = 0; i < documentInfo.Count; i++)
            {
                var temp = documentInfo[i];
                if (i < 2)
                {
                    worksheet.Cells[temp.fromRow, temp.fromCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[temp.fromRow, temp.fromCol, temp.toRow, temp.toCol].Merge = true;
                    worksheet.Cells[temp.fromRow, temp.fromCol].Value = temp.infoText;
                }
                else if (i >= 2 && i < 5)
                {
                    worksheet.Cells[temp.fromRow, temp.fromCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[temp.fromRow, temp.fromCol, temp.toRow, temp.toCol].Merge = true;
                    worksheet.Cells[temp.fromRow, temp.fromCol].Value = temp.infoText;
                    worksheet.Cells[temp.fromRow, temp.toCol + 2].Value = "-";
                    worksheet.Cells[temp.toRow, worksheet.Dimension.Columns].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                else if (i == 5)
                {
                    worksheet.Cells[temp.fromRow, temp.fromCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[temp.fromRow, temp.fromCol, temp.toRow, temp.toCol].Merge = true;
                    worksheet.Cells[temp.fromRow, temp.fromCol].Value = temp.infoText;
                    worksheet.Cells[temp.fromRow, temp.toCol + 2, temp.fromRow, temp.toCol + 4].Merge = true;
                    worksheet.Cells[temp.fromRow, temp.toCol + 2, temp.fromRow, temp.toCol + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[temp.toRow, worksheet.Dimension.Columns].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                else if (i > 5 && i < 8)
                {
                    worksheet.Cells[temp.fromRow, temp.fromCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[temp.fromRow, temp.fromCol, temp.toRow, temp.toCol].Merge = true;
                    worksheet.Cells[temp.fromRow, temp.fromCol].Value = temp.infoText;
                    worksheet.Cells[temp.fromRow, temp.toCol + 2, temp.fromRow, temp.toCol + 5].Merge = true;
                    worksheet.Cells[temp.fromRow, temp.toCol + 2, temp.fromRow, temp.toCol + 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[temp.fromRow, temp.toCol + 2].Worksheet.Row(temp.fromRow).Height = 20;
                }
                else
                {
                    worksheet.Cells[temp.fromRow, temp.fromCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[temp.fromRow, temp.fromCol, temp.toRow, temp.toCol].Merge = true;
                    worksheet.Cells[temp.fromRow, temp.fromCol].Value = temp.infoText;
                }
            }
        }

        public List<WorkloadCell> GetWorkloadCells(List<Workload> workloads)
        {
            var workloadCells = workloads.GroupBy((wl) => wl.TypeOfLesson + wl.Day).Select((wlGroup) => wlGroup.Aggregate(new WorkloadCell(), (cell, wl) =>
            {
                cell.TypeOfLesson = wl.TypeOfLesson;
                cell.Day = wl.Day;

                var hours = cell.Hours == null ? wl.Hours : cell.Hours + wl.Hours;
                var disciplines = cell.Discipline == null || cell.Discipline == wl.Disciplenes ? wl.Disciplenes : cell.Discipline + ", " + wl.Disciplenes;
                cell.Discipline = disciplines;
                cell.Hours = hours;
                return cell;
            })).ToList();

            return workloadCells;
        }
        public Dictionary<string, int> GetLessonHours(List<WorkloadCell> workloadCells)
        {
            return workloadCells.GroupBy((wc) => wc.TypeOfLesson).Select((wc) =>
            new
            {
                Key = wc.Key,
                Value = wc.Aggregate(0, (total, wc) => total += (int)wc.Hours)
            }).ToDictionary((kv) => kv.Key, (kv) => kv.Value);
        }

        public void FillDataInWorkload()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage package = new ExcelPackage(GetFilePath(CreateFileNameWorkload(DateTime.Now.Year, DateTime.Now.Month)));
            List<Workload> workloads = GetWorkload();
            ExcelWorksheet worksheet = package.Workbook.Worksheets["Нагрузка"];
            int startRow = 5;
            int startCol = 2;

            int startTableRow = 3;
            int startTableCol = 1;
            int endTableRow = 36;
            int endTableCol = 15;
            Action clearRows = () =>
            {
                for (int i = startRow; i <= worksheet.Dimension.Rows; i++)
                {
                    for (int j = startCol; j <= worksheet.Dimension.Columns; j++)
                    {
                        worksheet.Cells[i, j].Value = "";
                    }
                }
            };

            Func<WorkloadCell, int> getRow = (workload) =>
            {
                var day = workload.Day;
                for (int i = startRow; i <= worksheet.Dimension.Rows; i++)
                {
                    if (day.ToString() == worksheet.Cells[i, startCol - 1].Value.ToString())
                    {
                        return i;
                    }
                }

                return -1;
            };

            Func<WorkloadCell, int> getCol = (workload) =>
            {
                var lessonType = workload.TypeOfLesson;
                for (int j = startCol; j <= worksheet.Dimension.Columns; j++)
                {
                    if (lessonType.ToString() == worksheet.Cells[startRow - 1, j].Value.ToString())
                    {
                        return j;
                    }
                }

                return -1;
            };

            clearRows();

            var workloadCells = this.GetWorkloadCells(workloads);
            var totalHours = this.GetLessonHours(workloadCells);

            foreach (var cell in workloadCells)
            {
                var col = getCol(cell);
                var row = getRow(cell);
                worksheet.Cells[DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + startRow, col].Value = totalHours[cell.TypeOfLesson];
                //Наименование дисциплины заполнение;
                worksheet.Cells[row, typeOfLessons.Count + startCol].Value = $"{cell.Discipline}";
                worksheet.Cells[DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + startRow, typeOfLessons.Count + startCol].Value = "Всего: " + totalHours.Values.Sum();
                if (col != -1 && row != -1)
                {
                    worksheet.Cells[row, col].Value = cell.Hours;
                }
            }
            ExcelRange range = worksheet.Cells[startTableRow, startTableCol, endTableRow, endTableCol];
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            package.Save();
        }

        public List<ExcelDocumentInfo> FillListExcelDocumentInfo(ExcelWorksheet worksheet)
        {
            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            return new List<ExcelDocumentInfo>()
            {
                new ExcelDocumentInfo() { infoText = "6. ВЫПОЛНЕНИЕ НАГРУЗКИ ПО МЕСЯЦАМ",fromCol = 1, fromRow = 1, toRow = 1, toCol = 15},
                new ExcelDocumentInfo() { infoText = "Учебная работа в часах за месяц", fromCol = 2, fromRow = 3, toRow = 3, toCol = 1 + typeOfLessons.Count},
                new ExcelDocumentInfo() { infoText = "Учебно-методическая", fromRow = daysInMonth + 7, fromCol = 1, toRow = daysInMonth + 7, toCol = 6 },
                new ExcelDocumentInfo() { infoText = "Орагизационно-методическая", fromRow = daysInMonth + 8, fromCol = 1, toRow = daysInMonth + 8, toCol = 6 },
                new ExcelDocumentInfo() { infoText = "Научно-исследовательская", fromRow =daysInMonth + 9, fromCol = 1, toRow = daysInMonth + 9, toCol = 6  },
                new ExcelDocumentInfo() { infoText = "ВСЕГО выполнено работ за месяц", fromRow = daysInMonth + 10, fromCol = 1, toRow = daysInMonth + 10, toCol = 8  },
                new ExcelDocumentInfo() { infoText = "Подпись преподавателя", fromRow = daysInMonth + 12, fromCol = 1, toRow = daysInMonth + 12, toCol = 6  },
                new ExcelDocumentInfo() { infoText = "Заведующий кафедрой", fromRow = daysInMonth + 13, fromCol = 1, toRow = daysInMonth + 13, toCol = 6 }
            };
        }
        public string GetFilePath(string fileName)
        {
            return Path.Combine(GetPathForExcelFiles(), fileName);
        }

        public string CreateFileNameSchedule(int year)
        {
            var user = GetUserById(userID);
            string[] fullname = user.Fullname.Split(' ');
            string firstLetterMiddleName = fullname.Length > 2 ? fullname[2].Substring(0, 1) : "";
            return $"{fullname[0]}{fullname[1].Substring(0, 1)}{firstLetterMiddleName}-{year}-{schedulePath}";
        }

        public string CreateFileNameWorkload(int year, int month)
        {
            var user = GetUserById(userID);
            string[] fullname = user.Fullname.Split(' ');
            string firstLetterMiddleName = fullname.Length > 2 ? fullname[2].Substring(0, 1) : "";
            return $"{fullname[0]}{fullname[1].Substring(0, 1)}{firstLetterMiddleName}-{year}-{month}-{workloadPath}";
        }

        private void createWorkloadTemplateButton_Click(object sender, EventArgs e)
        {
            workloadGratsLabel.Text = "";
            CreateTemplateWorkload();
            workloadGratsLabel.Text = "Шаблон нагрузки был успешно создан";
            workloadGratsLabel.Visible = true;
        }

        private void fillWorkloadButton_Click(object sender, EventArgs e)
        {
            workloadGratsLabel.Text = "";
            FillDataInWorkload();
            workloadGratsLabel.Text = "Перенос данных в файл Excel был успешен";
            workloadGratsLabel.Visible = true;
        }
        #endregion

        #region[OpenExcelFiles]
        private void openScheduleButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo excel = new ProcessStartInfo();
            excel.UseShellExecute = true;
            excel.FileName = "EXCEL.EXE";
            var filePath = GetFilePath(CreateFileNameSchedule(DateTime.Now.Year));
            excel.Arguments = $"{filePath}";
            if (excel.Arguments != null)
            {
                Process.Start(excel);
            }
            findScheduleErrorLabel.Visible = true;
            return;
        }

        private void openWorkloadButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo excel = new ProcessStartInfo();
            excel.UseShellExecute = true;
            excel.FileName = "EXCEL.EXE";
            var filePath = GetFilePath(CreateFileNameWorkload((int)yearNumericUpDown.Value, months.FirstOrDefault(m => m.Value == monthComboBox.SelectedItem.ToString()).Key));
            excel.Arguments = $"{filePath}";
            if (excel.Arguments != GetFilePath(""))
            {
                Process.Start(excel);
            }
            findWorkloadErrorLabel.Visible = true;
            return;
        }
        #endregion

        #region[Classes]
        public class WorkloadCell
        {
            public int? Hours;
            public string TypeOfLesson;
            public int Day;
            public string Discipline;
        }

        public class DayOfWeekItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ExcelDocumentInfo
        {
            public int fromRow;
            public int fromCol;
            public int toRow;
            public int toCol;
            public string infoText;
        }
        #endregion

    }

}
